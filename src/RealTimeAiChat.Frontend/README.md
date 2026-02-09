# RealTime AI Chat - Angular Frontend

## 🎨 Design

Minimalist, professional design with clean UI:
- **Sidebar** - Dark panel on the left with chat list
- **Chat Area** - Light area for messages with minimalist bubbles
- **Streaming** - Real-time AI response typing with cursor
- **Responsive** - Adaptive design for different screen sizes

## ✨ Features

### Implemented Features:
- ✅ **Real-time SignalR connection** to backend
- ✅ **Create new chats** ("New Chat" button)
- ✅ **List all chats** with update dates
- ✅ **Switch between chats** with automatic history loading
- ✅ **Send messages** (Enter to send, Shift+Enter for new line)
- ✅ **Streaming AI responses** word-by-word with typing animation
- ✅ **AI Thinking indicator** (three animated dots)
- ✅ **Delete chats** with confirmation
- ✅ **Auto-scroll** to latest message
- ✅ **Connection status** - Backend connection indicator
- ✅ **Error handling** with user-friendly messages

## 🛠️ Technologies

- **Angular 21** - Latest version with standalone components
- **RxJS** - Reactive programming for streams
- **SignalR Client** - WebSocket communication
- **TypeScript** - Type-safe code
- **Pure CSS** - No UI libraries, clean minimalist design
- **Ollama LLaMA 3.2** - AI model

## 📦 Structure

```
src/app/
├── components/
│   ├── chat/
│   │   ├── chat.component.ts        # Main chat component
│   │   ├── chat.component.html      # Template with messages
│   │   └── chat.component.css       # Message styles
│   └── sidebar/
│       ├── sidebar.component.ts     # Sidebar with sessions list
│       ├── sidebar.component.html   # Sidebar template
│       └── sidebar.component.css    # Sidebar styles
├── services/
│   ├── signalr.service.ts           # SignalR WebSocket service
│   └── chat.service.ts              # HTTP REST API service
├── models/
│   └── chat.models.ts               # TypeScript interfaces
├── app.ts                           # Root component
├── app.routes.ts                    # Routing configuration
└── app.config.ts                    # App configuration
```

## 🚀 Quick Start

### 1. Install dependencies:
```bash
npm install
```

### 2. Make sure Backend is running:
```bash
cd ../RealTimeAiChat.Api
dotnet run
# Should be available at https://localhost:7001
```

### 3. Start Frontend:
```bash
npm start
```

### 4. Open browser:
```
http://localhost:4200
```

## 🎯 How to Use

### Create new chat:
1. Click "**+ New Chat**" in sidebar
2. New session automatically created
3. Start typing messages immediately

### Send message:
1. Type text in input field at bottom
2. Press **Enter** (or click ➤ button)
3. Shift+Enter - new line without sending

### AI response:
1. Message sent to backend
2. "AI thinking..." indicator appears (three dots)
3. AI starts streaming response word-by-word
4. Text types in real-time with cursor |
5. After completion - full response saved

### Delete chat:
1. Hover over chat in sidebar
2. **×** button appears on the right
3. Click × → confirm deletion

### Switch chats:
1. Click any chat in sidebar
2. History automatically loads
3. Continue conversation

## 🎨 Color Scheme

### Light Mode:
- **Background:** white (#ffffff)
- **Sidebar:** dark gray (#1f2937)
- **User messages:** blue (#3b82f6)
- **AI messages:** light gray (#f3f4f6)
- **Accent:** blue (#3b82f6)

### UI Elements:
- **Border radius:** 0.75rem (rounded corners)
- **Shadows:** soft shadows for depth
- **Animations:** smooth transitions (0.2s)
- **Fonts:** system fonts (-apple-system, Segoe UI, etc.)

## 🔌 API Integration

### REST API:
- `GET /api/chatsessions` - list sessions
- `POST /api/chatsessions` - create session
- `DELETE /api/chatsessions/{id}` - delete session

### SignalR Hub: `/chathub`
- `JoinSession(sessionId)` - join session
- `SendMessage(sessionId, message)` - send message
- `LeaveSession(sessionId)` - leave session

**Server events:**
- `LoadHistory` - message history
- `ReceiveMessage` - new message
- `AiThinking` - AI is thinking
- `StreamStart` - start of streaming
- `StreamChunk` - response chunk
- `StreamComplete` - completion
- `Error` - error occurred

## 📝 Configuration

### Environment:
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001'  // Backend URL
};
```

For production, change `apiUrl` to actual domain.

## 🐛 Troubleshooting

### "Cannot connect to SignalR"
**Problem:** Backend not running
**Solution:** 
```bash
cd ../RealTimeAiChat.Api
dotnet run
```

### "CORS error"
**Problem:** Backend doesn't allow requests from localhost:4200
**Solution:** Check `appsettings.json` in API:
```json
"AllowedOrigins": "http://localhost:4200"
```

### Empty chat list
**Problem:** Database is empty
**Solution:** Create new chat with "+ New Chat" button

## 🔥 Features in Action

### Real-Time Streaming:
```
User: "Explain Docker"
       ↓
AI: "Docker " 
    "is "
    "a "
    "platform "
    ...
(each word appears in real-time)
```

### Auto-Scroll:
Automatically scrolls to bottom when new message received.

### Responsive Design:
- Desktop: full width with sidebar
- Tablet: adaptive layout
- Mobile: responsive (may need further optimization)

## 📊 Performance

- **Build size:** ~351 KB (optimized production build)
- **Initial load:** < 1s
- **SignalR reconnect:** automatic with backoff
- **Memory:** efficient with RxJS cleanup

## 🚀 Ready to Demo!

Frontend is fully ready for client demonstrations:
1. Quick start (npm start)
2. Minimalist design
3. All features working
4. Real-time streaming
5. Professional appearance

---

**Created for Upwork Portfolio Project** 🎯
