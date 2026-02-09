import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { Message } from '../models/chat.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection?: signalR.HubConnection;
  
  // Connection state
  connected = signal(false);
  connectionError = signal<string | null>(null);
  
  // AI state
  aiThinking = signal(false);
  
  // Message streams
  messageReceived$ = new Subject<Message>();
  historyLoaded$ = new Subject<Message[]>();
  streamStart$ = new Subject<{ id: string; sessionId: string; role: string }>();
  streamChunk$ = new Subject<{ id: string; content: string }>();
  streamComplete$ = new Subject<Message>();
  error$ = new Subject<{ message: string; details?: string }>();

  async connect(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/chathub`, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();

    try {
      await this.hubConnection.start();
      this.connected.set(true);
      this.connectionError.set(null);
    } catch (err: any) {
      this.connected.set(false);
      this.connectionError.set(err.message);
      console.error('SignalR connection failed:', err);
      throw err;
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // LoadHistory
    this.hubConnection.on('LoadHistory', (messages: any[]) => {
      const parsedMessages = messages.map(m => ({
        ...m,
        timestamp: new Date(m.timestamp)
      }));
      this.historyLoaded$.next(parsedMessages);
    });

    // ReceiveMessage
    this.hubConnection.on('ReceiveMessage', (message: any) => {
      this.messageReceived$.next({
        ...message,
        timestamp: new Date(message.timestamp)
      });
    });

    // AiThinking
    this.hubConnection.on('AiThinking', (isThinking: boolean) => {
      this.aiThinking.set(isThinking);
    });

    // StreamStart
    this.hubConnection.on('StreamStart', (info: any) => {
      this.streamStart$.next(info);
    });

    // StreamChunk
    this.hubConnection.on('StreamChunk', (chunk: any) => {
      this.streamChunk$.next(chunk);
    });

    // StreamComplete
    this.hubConnection.on('StreamComplete', (message: any) => {
      this.streamComplete$.next({
        ...message,
        timestamp: new Date(message.timestamp)
      });
    });

    // Error
    this.hubConnection.on('Error', (error: any) => {
      this.error$.next(error);
    });

    // Connection events
    this.hubConnection.onreconnecting(() => {
      this.connected.set(false);
    });

    this.hubConnection.onreconnected(() => {
      this.connected.set(true);
    });

    this.hubConnection.onclose((error) => {
      this.connected.set(false);
      if (error) {
        console.error('SignalR connection closed:', error);
      }
    });
  }

  async joinSession(sessionId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      await this.connect();
    }
    await this.hubConnection!.invoke('JoinSession', sessionId);
  }

  async sendMessage(sessionId: string, userMessage: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected to SignalR');
    }
    await this.hubConnection.invoke('SendMessage', sessionId, userMessage);
  }

  async leaveSession(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveSession', sessionId);
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connected.set(false);
    }
  }
}
