export interface Message {
  id: string;
  sessionId: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  metadata?: string;
}

export interface ChatSession {
  id: string;
  title: string;
  createdAt: Date;
  updatedAt: Date;
  userId?: string;
}

export interface CreateSessionDto {
  title?: string;
}

export interface UpdateSessionDto {
  title?: string;
}
