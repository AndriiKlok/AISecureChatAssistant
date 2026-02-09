# RealTime AI Chat Assistant - Backend API

## 🎯 Description

ASP.NET Core 9.0 Web API with SignalR support for real-time AI chat with Ollama.

## ✨ Features

- ✅ **SignalR Hub** for real-time WebSocket communication
- ✅ **Ollama Integration** with streaming AI responses
- ✅ **SQLite Database** with Entity Framework Core
- ✅ **CRUD Controllers** for sessions and messages
- ✅ **Dependency Injection** for all services
- ✅ **Swagger UI** for API testing
- ✅ **CORS Configuration** for Angular frontend

## 🛠️ Technologies

- **ASP.NET Core 9.0** - Web API framework
- **SignalR** - Real-time WebSocket communication
- **Entity Framework Core 8** - ORM for database operations
- **SQLite** - Lightweight database
- **Swashbuckle** - Swagger/OpenAPI documentation
- **System.Text.Json** - JSON serialization

## 📦 Project Structure

```
RealTimeAiChat.Api/
├── Controllers/
│   ├── ChatSessionsController.cs   # CRUD for chat sessions
│   └── MessagesController.cs       # Get message history
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext
├── DTOs/
│   └── ChatDtos.cs                 # Data Transfer Objects
├── Hubs/
│   └── ChatHub.cs                  # SignalR Hub for real-time chat
├── Migrations/
│   └── *_InitialCreate.cs          # EF Core migrations
├── Services/
│   ├── IOllamaService.cs           # Ollama interface
│   ├── OllamaService.cs            # Ollama AI integration
│   ├── IChatService.cs             # Business logic interface
│   └── ChatService.cs              # Chat management service
├── appsettings.json                # Configuration
└── Program.cs                      # Application startup
```

## 🚀 Running the Project

### Prerequisites

1. **.NET 9 SDK**: https://dotnet.microsoft.com/download
2. **Ollama** (running locally):
   ```bash
   # Windows (via winget)
   winget install Ollama.Ollama
   
   # Or Docker
   docker run -d -p 11434:11434 --name ollama ollama/ollama
   docker exec ollama ollama pull llama3.2
   ```

### Steps to Run

1. **Navigate to API folder**:
   ```bash
   cd src/RealTimeAiChat.Api
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Apply migrations** (automatic on startup):
   ```bash
   dotnet ef database update
   ```

4. **Run API**:
   ```bash
   dotnet run
   ```

5. **Open Swagger UI**:
   - HTTPS: https://localhost:7001
   - HTTP: http://localhost:5000

## 🔌 API Endpoints

### REST API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/chatsessions` | Get all chat sessions |
| GET | `/api/chatsessions/{id}` | Get session by ID |
| POST | `/api/chatsessions` | Create new session |
| PATCH | `/api/chatsessions/{id}` | Update session |
| DELETE | `/api/chatsessions/{id}` | Delete session |
| GET | `/api/chatsessions/{id}/messages` | Get session messages |

### SignalR Hub

**Endpoint:** `/chathub`

**Client → Server Methods:**
- `JoinSession(sessionId)` - Join chat session
- `SendMessage(sessionId, message)` - Send message
- `LeaveSession(sessionId)` - Leave session

**Server → Client Events:**
- `LoadHistory(messages)` - Load chat history
- `ReceiveMessage(message)` - Receive new message
- `AiThinking(isThinking)` - AI processing indicator
- `StreamStart()` - Start of AI response stream
- `StreamChunk(chunk)` - Chunk of AI response
- `StreamComplete(message)` - Complete AI response
- `Error(errorMessage)` - Error notification

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=chat.db"
  },
  "OllamaUrl": "http://localhost:11434",
  "OllamaModel": "llama3.2",
  "AllowedOrigins": "http://localhost:4200,https://localhost:4200",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.SignalR": "Debug"
    }
  }
}
```

## 🔒 Security

- **CORS** configured for Angular frontend (localhost:4200)
- **HTTPS** enabled by default for development
- **SignalR** with authentication support (ready for implementation)

## 📊 Database Schema

### ChatSessions
- `Id` - Primary key
- `Title` - Session title (auto-generated from first message)
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp
- `UserId` - User identifier (nullable, for future auth)

### Messages
- `Id` - Primary key
- `SessionId` - Foreign key to ChatSessions
- `Role` - 'user' or 'assistant'
- `Content` - Message text
- `Timestamp` - Message timestamp
- `Metadata` - JSON metadata (optional)

## 🧪 Testing

### Manual Testing with Swagger

1. Open https://localhost:7001/swagger
2. Create session: `POST /api/chatsessions`
3. Get sessions: `GET /api/chatsessions`
4. Get messages: `GET /api/chatsessions/{id}/messages`

### Testing SignalR

Use SignalR client or frontend application to test:
1. Connect to `https://localhost:7001/chathub`
2. Call `JoinSession(sessionId)`
3. Call `SendMessage(sessionId, "Hello AI")`
4. Receive streaming response

## 🐛 Troubleshooting

### Cannot connect to Ollama
**Solution:** Check Ollama is running - `ollama serve`

### Database migration error
**Solution:** Delete `chat.db` and restart application

### CORS error
**Solution:** Check `AllowedOrigins` in `appsettings.json`

### SignalR connection failed
**Solution:** Verify HTTPS certificate is trusted

## 📝 Development Notes

- Database migrations applied automatically on startup
- Swagger UI available in Development environment
- Structured logging with detailed SignalR events
- Auto-generated session titles from first user message

---

**Part of Real-Time AI Chat Assistant project**
