import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ChatService } from '../../services/chat.service';
import { ChatSession } from '../../models/chat.models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);
  private router = inject(Router);
  
  sessions = signal<ChatSession[]>([]);
  isLoading = signal(false);
  isCreating = signal(false);

  contextMenuVisible = signal(false);
  contextMenuPosition = signal({ x: 0, y: 0 });
  contextMenuSession = signal<ChatSession | null>(null);

  private subscriptions: Subscription[] = [];

  ngOnInit() {
    this.loadSessions();

    this.subscriptions.push(
      this.chatService.sessionUpdated$.subscribe(updated => {
        this.sessions.update(sessions =>
          sessions.map(s => (s.id === updated.id ? { ...s, title: updated.title, updatedAt: updated.updatedAt } : s))
        );
      })
    );
  }

  loadSessions() {
    this.isLoading.set(true);
    this.chatService.getAllSessions().subscribe({
      next: (sessions) => {
        this.sessions.set(sessions);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load sessions:', err);
        this.isLoading.set(false);
      }
    });
  }

  async createNewSession() {
    if (this.isCreating()) return;
    
    this.isCreating.set(true);
    try {
      const session = await this.chatService.createSession().toPromise();
      if (session) {
        this.sessions.update(sessions => [session, ...sessions]);
        await this.router.navigate(['/chat', session.id]);
      }
    } catch (err) {
      console.error('Failed to create session:', err);
      alert('Failed to create new chat');
    } finally {
      this.isCreating.set(false);
    }
  }

  async deleteSession(event: Event, sessionId: string) {
    event.preventDefault();
    event.stopPropagation();

    await this.deleteSessionById(sessionId);
  }

  async deleteSessionById(sessionId: string) {
    
    if (!confirm('Delete this chat?')) {
      return;
    }

    try {
      await this.chatService.deleteSession(sessionId).toPromise();
      this.sessions.update(sessions => sessions.filter(s => s.id !== sessionId));
      
      // Navigate away if current session is deleted
      const currentUrl = this.router.url;
      if (currentUrl.includes(sessionId)) {
        await this.router.navigate(['/']);
      }
    } catch (err) {
      console.error('Failed to delete session:', err);
      alert('Failed to delete chat');
    }
  }

  openContextMenu(event: MouseEvent, session: ChatSession) {
    event.preventDefault();
    event.stopPropagation();

    this.contextMenuSession.set(session);
    this.contextMenuPosition.set({ x: event.clientX, y: event.clientY });
    this.contextMenuVisible.set(true);
  }

  closeContextMenu() {
    this.contextMenuVisible.set(false);
  }

  async renameSessionFromMenu() {
    const session = this.contextMenuSession();
    if (!session) {
      this.closeContextMenu();
      return;
    }

    const newTitle = prompt('New chat title', session.title ?? '');
    if (!newTitle || !newTitle.trim() || newTitle.trim() === session.title) {
      this.closeContextMenu();
      return;
    }

    try {
      const trimmed = newTitle.trim();
      await this.chatService.updateSession(session.id, { title: trimmed }).toPromise();
      const updatedSession = { ...session, title: trimmed, updatedAt: new Date() };
      this.sessions.update(sessions =>
        sessions.map(s => (s.id === session.id ? updatedSession : s))
      );
      this.chatService.emitSessionUpdated(updatedSession);
    } catch (err) {
      console.error('Failed to rename session:', err);
      alert('Failed to rename chat');
    } finally {
      this.closeContextMenu();
    }
  }

  async deleteSessionFromMenu() {
    const session = this.contextMenuSession();
    if (!session) {
      this.closeContextMenu();
      return;
    }

    await this.deleteSessionById(session.id);
    this.closeContextMenu();
  }

  formatDate(date: Date): string {
    const d = new Date(date);
    const now = new Date();
    const diff = now.getTime() - d.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    if (days < 7) return `${days} days ago`;
    return d.toLocaleDateString();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
