import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ChatService } from '../../services/chat.service';
import { ChatSession } from '../../models/chat.models';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent implements OnInit {
  private chatService = inject(ChatService);
  private router = inject(Router);
  
  sessions = signal<ChatSession[]>([]);
  isLoading = signal(false);
  isCreating = signal(false);

  ngOnInit() {
    this.loadSessions();
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
      const session = await this.chatService.createSession({ title: 'New Chat' }).toPromise();
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
}
