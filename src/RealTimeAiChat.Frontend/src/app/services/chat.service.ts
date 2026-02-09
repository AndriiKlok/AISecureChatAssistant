import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatSession, CreateSessionDto, UpdateSessionDto, Message } from '../models/chat.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  // Sessions
  getAllSessions(): Observable<ChatSession[]> {
    return this.http.get<ChatSession[]>(`${this.apiUrl}/api/chatsessions`);
  }

  getSession(id: string): Observable<ChatSession> {
    return this.http.get<ChatSession>(`${this.apiUrl}/api/chatsessions/${id}`);
  }

  createSession(dto: CreateSessionDto = {}): Observable<ChatSession> {
    return this.http.post<ChatSession>(`${this.apiUrl}/api/chatsessions`, dto);
  }

  updateSession(id: string, dto: UpdateSessionDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/api/chatsessions/${id}`, dto);
  }

  deleteSession(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/chatsessions/${id}`);
  }

  // Messages
  getSessionMessages(sessionId: string, maxMessages: number = 50): Observable<Message[]> {
    return this.http.get<Message[]>(
      `${this.apiUrl}/api/messages/session/${sessionId}?maxMessages=${maxMessages}`
    );
  }
}
