# Real-Time AI Chat Assistant 🤖💬

> Full-stack application with SignalR + Ollama integration, streaming AI responses, and Clean Architecture

![Project Status](https://img.shields.io/badge/Status-Production%20Ready-brightgreen)
![.NET version](https://img.shields.io/badge/.NET-9.0-blue)
![Angular version](https://img.shields.io/badge/Angular-21-red)
![Tests](https://img.shields.io/badge/Tests-54%20Passing-success)

---

## 📋 About

**Real-Time AI Chat Assistant** is a production-ready system for working with AI through real-time WebSocket communication with comprehensive test coverage:

- ✅ **SignalR** - Real-time WebSocket bidirectional communication
- ✅ **Ollama AI** - Local LLaMA 3.2 model with streaming responses
- ✅ **ASP.NET Core 9.0** - Modern backend with Clean Architecture
- ✅ **Angular 21** - Reactive frontend with standalone components
- ✅ **Entity Framework Core 9.0** - SQLite database with migrations
- ✅ **Clean Architecture** - Domain, Application, API layers properly separated
- ✅ **Unit & Integration Tests** - 54 tests covering all layers (xUnit, Moq, FluentAssertions)

---

## 🎯 Key Features

### 🔥 Real-Time Features:
- **Streaming AI responses** - Word-by-word in real-time (like ChatGPT)
- **SignalR Hub** - Bidirectional communication via WebSocket
- **Live updates** - Instant updates of chat list and messages
- **Connection status** - Connection status indication

### 💬 Chat Features:
- **Multiple chat sessions** - Create, switch, delete conversations
- **Message history** - Automatic save to SQLite database
- **AI thinking indicator** - Animated "AI is thinking..." during processing
- **Markdown support** - Response formatting (ready for integration)
- **Auto-scroll** - Automatic scroll to new messages

### 🎨 UI/UX:
- **Minimalist design** - Clean professional interface
- **Dark sidebar** - Chat list on the left
- **Message bubbles** - User (blue, right), AI (gray, left)
- **Responsive** - Adaptive for different screen sizes
- **Smooth animations** - Smooth transitions and fade-in effects

---

## 🏗️ Architecture

```
AISecureChatAssistant/
├── src/
│   ├── RealTimeAiChat.Api/              # ASP.NET Core Web API + SignalR
│   │   ├── Controllers/                 # REST API controllers
│   │   ├── Hubs/                        # SignalR Hub (ChatHub)
│   │   ├── Services/                    # Infrastructure services (OllamaService, ChatService)
│   │   ├── Data/                        # EF Core DbContext + Migrations
│   │   └── Program.cs                   # DI, CORS, SignalR configuration
│   │
│   ├── RealTimeAiChat.Application/      # Application Layer (Business Logic)
│   │   ├── DTOs/                        # Data Transfer Objects (prevents circular refs)
│   │   └── Services/                    # Application services (ChatApplicationService)
│   │
│   ├── RealTimeAiChat.Domain/           # Domain Layer (Entities)
│   │   ├── ChatSession.cs               # Chat session aggregate
│   │   └── Message.cs                   # Message entity
│   │
│   └── RealTimeAiChat.Frontend/         # Angular 21 SPA
│       ├── src/app/
│       │   ├── components/
│       │   │   ├── chat/                # Main chat component
│       │   │   └── sidebar/             # Sessions sidebar component
│       │   ├── services/
│       │   │   ├── signalr.service.ts   # SignalR WebSocket client
│       │   │   └── chat.service.ts      # HTTP REST API client
│       │   └── models/
│       │       └── chat.models.ts       # TypeScript interfaces
│       └── package.json
│
└── tests/
    └── RealTimeAiChat.Tests/            # Comprehensive test suite (54 tests)
        ├── UnitTests/
        │   ├── Domain/                  # Entity tests (16 tests)
        │   ├── Application/             # Business logic tests (10 tests)
        │   └── Services/                # Service tests (13 tests)
        └── IntegrationTests/
            ├── Hubs/                    # SignalR hub tests (6 tests)
            └── Database/                # EF Core tests (9 tests)
```

---

## 🚀 Quick Start

### Requirements:
- ✅ **.NET SDK 9.0+** - [Download](https://dotnet.microsoft.com/download)
- ✅ **Node.js 18+** - [Download](https://nodejs.org/)
- ✅ **Ollama** - [Download](https://ollama.com/)
- ✅ **LLaMA 3.2 model** - `ollama pull llama3.2`

---

### 🔧 Setup

#### 1️⃣ Start Ollama Service:
```bash
ollama serve
# Should be available at http://localhost:11434
```

**Verify:**
```bash
curl http://localhost:11434/api/version
# Pull model:
ollama pull llama3.2
```

#### 2️⃣ Start Backend API:
```bash
cd src/RealTimeAiChat.Api
dotnet restore
dotnet run
```

✅ API starts on: **http://localhost:7001**  
✅ Swagger UI: **http://localhost:7001/** (root path)  
✅ SignalR Hub: **http://localhost:7001/chathub**  

SQLite database (`chat.db`) is created automatically with migrations.

#### 3️⃣ Start Frontend:
```bash
cd src/RealTimeAiChat.Frontend
npm install
npm start
```

✅ Angular app: **http://localhost:4200**

---

## 📡 API Documentation

### REST API Endpoints:

#### Get All Chat Sessions:
```http
GET /api/chatsessions
```

#### Create New Session:
```http
POST /api/chatsessions
```

#### Delete Session:
```http
DELETE /api/chatsessions/{id}
```

#### Get Session Messages:
```http
GET /api/chatsessions/{id}/messages
```

---

### SignalR Hub: `/chathub`

#### Client → Server Methods:

- `JoinSession(sessionId)` - Join chat session
- `SendMessage(sessionId, message)` - Send user message
- `LeaveSession(sessionId)` - Leave chat session

#### Server → Client Events:

- `LoadHistory(messages)` - Chat history loaded
- `ReceiveMessage(message)` - New message received
- `AiThinking(isThinking)` - AI processing indicator
- `StreamStart()` - Start of streaming response
- `StreamChunk(chunk)` - Part of AI response
- `StreamComplete(message)` - Complete AI response saved
- `Error(errorMessage)` - Error during processing

---

## 🎨 Tech Stack

### Frontend:
- **Angular 21** - Standalone components, signals API
- **TypeScript 5.9** - Strong typing
- **RxJS 7.8** - Reactive programming
- **@microsoft/signalr 8.0** - SignalR client
- **Pure CSS** - Minimalist design

### Backend:
- **ASP.NET Core 9.0** - Web API framework
- **SignalR** - Real-time WebSocket hub
- **Entity Framework Core 8** - ORM for SQLite
- **Ollama AI** - Local LLaMA 3.2 model
- **Swashbuckle** - Swagger/OpenAPI docs

---

## 🗄️ Database Schema

### SQLite Database: `chat.db`

**ChatSessions Table:**
- `Id` (INTEGER, PRIMARY KEY)
- `Title` (TEXT)
- `CreatedAt` (TEXT, ISO 8601)
- `UpdatedAt` (TEXT, ISO 8601)
- `UserId` (TEXT, nullable)

**Messages Table:**
- `Id` (INTEGER, PRIMARY KEY)
- `SessionId` (INTEGER, FOREIGN KEY)
- `Role` (TEXT: 'user' or 'assistant')
- `Content` (TEXT)
- `Timestamp` (TEXT, ISO 8601)
- `Metadata` (TEXT, JSON)

---

## 🔧 Configuration

### Backend: `appsettings.json`
```json
{
  "OllamaUrl": "http://localhost:11434",
  "OllamaModel": "llama3.2",
  "AllowedOrigins": "http://localhost:4200"
}
```

### Frontend: `environment.ts`
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001'
};
```

---

## 🐛 Troubleshooting

### "Cannot connect to SignalR"
**Solution:** Check backend is running on https://localhost:7001

### "Ollama API error"
**Solution:** Start Ollama service - `ollama serve`

### "CORS policy error"
**Solution:** Verify `AllowedOrigins` in `appsettings.json`

### Empty chat list
**Solution:** Create new chat via "+ New Chat" button

---

## 🧪 Testing

Comprehensive test suite with **54 passing tests** covering all layers:

### Unit Tests (39 tests):
- **Domain Layer** (16 tests) - Entity validation, behavior, invariants
- **Application Layer** (10 tests) - Business logic, DTO mapping with Moq
- **Services** (13 tests) - Data access with in-memory database

### Integration Tests (15 tests):
- **SignalR Hubs** (6 tests) - Real-time messaging, streaming, broadcasting
- **Database** (9 tests) - EF Core operations, cascade deletes, querying

**Test Stack:** xUnit 2.9, Moq 4.20, FluentAssertions 7.0, EF Core InMemory

```bash
# Run all tests
cd tests/RealTimeAiChat.Tests
dotnet test

# Test summary: total: 54; failed: 0; succeeded: 54
```

---

## 🐳 Docker Deployment

### Quick Start with Docker Compose:

**Prerequisites:** Ollama must be running on host machine
```bash
# Install and start Ollama (if not installed)
ollama serve

# Pull AI model
ollama pull llama3.2
```

**Start Application:**
```bash
# Build and start containers
docker-compose up -d

# View logs
docker-compose logs -f

# Stop containers
docker-compose down
```

**Access Services:**
- **Frontend:** http://localhost:4200
- **Backend API:** http://localhost:7001
- **Swagger UI:** http://localhost:7001/ (root path)

**Architecture:**
- Frontend container (Nginx + Angular) → Port 4200
- Backend container (.NET API + SignalR) → Port 7001
- Ollama (Host machine) → Port 11434

**Docker Images:**
- Frontend: ~50 MB (nginx:alpine + Angular build)
- Backend: ~235 MB (dotnet/aspnet:9.0 + app)
- Total: ~285 MB

**Volumes:**
- `chat-db` - SQLite database persistence

See [DOCKER.md](DOCKER.md) for detailed deployment guide, troubleshooting, and production configuration.

---

## 🎯 Technical Highlights

✅ **Real-time WebSocket** - SignalR bidirectional communication  
✅ **AI Integration** - Ollama local LLM with streaming responses  
✅ **Clean Architecture** - Proper layer separation (Domain → Application → API)  
✅ **Full-stack .NET** - ASP.NET Core 9 + Entity Framework Core 9  
✅ **Modern Frontend** - Angular 21 with signals and reactive patterns  
✅ **Test-Driven Development** - Comprehensive unit & integration tests  
✅ **SOLID Principles** - Dependency injection, interface segregation  

### Use Cases:
- Real-time Chat/Messaging Applications
- AI Chatbots & Virtual Assistants
- Customer Support Systems
- .NET Core Web API Development
- Angular SPA Development
- SignalR/WebSocket Projects

---

## 📄 License

**MIT License** - Free to use for learning and commercial purposes.

---

## 👨‍💻 Author

**Full-stack .NET Developer**  
📧 Contact: [andrii.klok@gmail.com](mailto:andrii.klok@gmail.com)  
🔗 GitHub: [github.com/AndriiKlok](https://github.com/AndriiKlok)

---

## ⭐ Show Support

If this project helped you understand SignalR + AI integration, give it a star! ⭐

## ⭐ Show Support

If this project helped you understand SignalR + AI integration, give it a star! ⭐

---

**Created with ❤️ for Upwork Portfolio**
