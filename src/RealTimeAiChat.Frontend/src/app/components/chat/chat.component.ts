import { Component, OnInit, OnDestroy, signal, effect, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SignalRService } from '../../services/signalr.service';
import { ChatService } from '../../services/chat.service';
import { Message, ChatSession } from '../../models/chat.models';
import { Subscription } from 'rxjs';
import { marked } from 'marked';
import DOMPurify from 'dompurify';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy {
  @ViewChild('messageContainer') messageContainer?: ElementRef;
  @ViewChild('messageInput') messageInput?: ElementRef;

  private signalR = inject(SignalRService);
  private chatService = inject(ChatService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  
  // State
  currentSession = signal<ChatSession | null>(null);
  messages = signal<Message[]>([]);
  userMessage = signal('');
  
  // Streaming state
  streamingMessage = signal<Message | null>(null);
  
  // UI state
  isConnected = this.signalR.connected;
  isAiThinking = this.signalR.aiThinking;
  isSending = signal(false);
  
  private subscriptions: Subscription[] = [];

  constructor() {
    marked.setOptions({
      gfm: true,
      breaks: true
    });

    // Auto-scroll on message changes
    effect(() => {
      this.messages();
      this.streamingMessage();
      setTimeout(() => this.scrollToBottom(), 100);
    });
  }

  async ngOnInit() {
    // Connect to SignalR
    try {
      await this.signalR.connect();
    } catch (err) {
      console.error('Failed to connect to SignalR:', err);
    }

    // Subscribe to SignalR events
    this.subscriptions.push(
      this.chatService.sessionUpdated$.subscribe(updated => {
        const current = this.currentSession();
        if (current && current.id === updated.id) {
          this.currentSession.set({ ...current, title: updated.title, updatedAt: updated.updatedAt });
        }
      }),
      this.signalR.historyLoaded$.subscribe(history => {
        this.messages.set(history);
      }),
      
      this.signalR.messageReceived$.subscribe(msg => {
        this.messages.update(msgs => [...msgs, msg]);
      }),
      
      this.signalR.streamStart$.subscribe(info => {
        this.streamingMessage.set({
          id: info.id,
          sessionId: info.sessionId,
          role: 'assistant',
          content: '',
          timestamp: new Date()
        });
      }),
      
      this.signalR.streamChunk$.subscribe(chunk => {
        this.streamingMessage.update(msg => {
          if (msg && msg.id === chunk.id) {
            return { ...msg, content: msg.content + chunk.content };
          }
          return msg;
        });
      }),
      
      this.signalR.streamComplete$.subscribe(msg => {
        this.messages.update(msgs => {
          const existingIndex = msgs.findIndex(m => m.id === msg.id);
          if (existingIndex >= 0) {
            const newMsgs = [...msgs];
            newMsgs[existingIndex] = msg;
            return newMsgs;
          }
          return [...msgs, msg];
        });
        this.streamingMessage.set(null);
      }),
      
      this.signalR.error$.subscribe(error => {
        alert(`Error: ${error.message}`);
        this.isSending.set(false);
      })
    );

    // Load session from route
    this.route.params.subscribe(async params => {
      const sessionId = params['id'];
      if (sessionId) {
        await this.loadSession(sessionId);
      }
    });
  }

  async loadSession(sessionId: string) {
    try {
      // Leave previous session
      const prevSession = this.currentSession();
      if (prevSession) {
        await this.signalR.leaveSession(prevSession.id);
      }

      // Load new session
      const session = await this.chatService.getSession(sessionId).toPromise();
      if (session) {
        this.currentSession.set(session);
        await this.signalR.joinSession(sessionId);
      }
    } catch (err) {
      console.error('Failed to load session:', err);
      alert('Failed to load chat session');
    }
  }

  async sendMessage() {
    const message = this.userMessage().trim();
    const session = this.currentSession();
    
    if (!message || !session || this.isSending()) {
      return;
    }

    try {
      this.isSending.set(true);

      if (session.title.trim().toLowerCase() === 'new chat') {
        const newTitle = this.generateTitleFromMessage(message);
        try {
          await this.chatService.updateSession(session.id, { title: newTitle }).toPromise();
          const updatedSession = { ...session, title: newTitle, updatedAt: new Date() };
          this.currentSession.set(updatedSession);
          this.chatService.emitSessionUpdated(updatedSession);
        } catch (err) {
          console.error('Failed to auto-title session:', err);
        }
      }

      await this.signalR.sendMessage(session.id, message);
      this.userMessage.set('');
      this.messageInput?.nativeElement.focus();
    } catch (err) {
      console.error('Failed to send message:', err);
      alert('Failed to send message');
    } finally {
      this.isSending.set(false);
    }
  }

  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private scrollToBottom() {
    if (this.messageContainer) {
      const element = this.messageContainer.nativeElement;
      element.scrollTop = element.scrollHeight;
    }
  }

  private generateTitleFromMessage(message: string): string {
    const cleaned = message.replace(/\s+/g, ' ').trim();
    if (!cleaned) return 'New Chat';

    const words = cleaned.split(' ');
    const maxWords = 6;
    let title = words.slice(0, maxWords).join(' ');
    if (words.length > maxWords) {
      title += '...';
    }

    const maxLen = 60;
    if (title.length > maxLen) {
      title = `${title.slice(0, maxLen - 3).trim()}...`;
    }

    return title;
  }

  renderMarkdown(content: string): string {
    const html = marked.parse(content ?? '') as string;
    return DOMPurify.sanitize(html);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    const session = this.currentSession();
    if (session) {
      this.signalR.leaveSession(session.id);
    }
  }
}
