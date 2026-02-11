# RealTimeAiChat.Application

## Purpose

This is the **Application Layer** in Clean Architecture pattern, containing all business logic and use cases.

## Current Implementation

✅ **Fully Implemented** with proper separation of concerns.

### Components

#### DTOs (Data Transfer Objects)
Located in `DTOs/ChatDtos.cs`:
- `ChatSessionDto` - Chat session with messages (no circular references)
- `MessageDto` - Individual message data
- `CreateSessionDto` - Request DTO for creating sessions
- `UpdateSessionDto` - Request DTO for updating sessions
- `SendMessageDto` - Request DTO for sending messages

#### Application Services
Located in `Services/`:

**IChatApplicationService** - Main application service interface
```csharp
Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync();
Task<ChatSessionDto?> GetSessionAsync(string id);
Task<ChatSessionDto> CreateSessionAsync(string? title = null);
Task<bool> UpdateSessionAsync(string id, string? title);
Task<bool> DeleteSessionAsync(string id);
Task<IEnumerable<MessageDto>> GetSessionHistoryAsync(string sessionId, int maxMessages = 50);
```

**ChatApplicationService** - Implementation
- Coordinates with IChatDomainService (implemented by ChatService in API layer)
- Handles DTO mapping to prevent circular references
- Applies business logic and ordering
- Provides consistent error handling and logging

**IChatDomainService** - Domain service abstraction
- Abstracts data access layer from application
- Implemented by ChatService in API layer
- Allows API layer to work with Application layer without direct dependency

## Architecture Pattern

### Dependency Flow
```
API Controllers
    ↓ (depend on)
Application Services (IChatApplicationService, ChatApplicationService)
    ↓ (depend on)
Domain Services (IChatDomainService)
    ↓ (implemented by)
API Services (ChatService)
```

This ensures:
- ✓ Application layer is independent of API layer
- ✓ No circular dependencies
- ✓ Clean separation of concerns
- ✓ Easy to test and maintain

## Key Features

1. **DTO Mapping** - Prevents circular references in JSON serialization
2. **Error Handling** - Comprehensive logging for failures
3. **Ordering** - Consistent chronological/recent-first ordering
4. **Async/Await** - Full async support for scalability
5. **DI Integration** - Seamless dependency injection in API layer

## Usage in API Layer

```csharp
// In Program.cs - Dependency Injection Setup
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<IChatDomainService>(provider => 
    provider.GetRequiredService<ChatService>());
builder.Services.AddScoped<IChatApplicationService, ChatApplicationService>();

// In Controllers
public ChatSessionsController(IChatApplicationService chatApplicationService)
{
    // Use application service for all business logic
}
```

## Future Enhancements

When the project grows, add:
- `IValidationService` - Message/session validation rules
- `IPromptService` - AI prompt engineering logic
- `IAuditService` - Action logging and audit trails
- `ICacheService` - Caching layer abstraction

## References

- **Referenced by:** `RealTimeAiChat.Api`
- **References:** `RealTimeAiChat.Domain`, `Microsoft.Extensions.Logging`

## Dependencies

- `Microsoft.Extensions.Logging.Abstractions` - For ILogger interface
