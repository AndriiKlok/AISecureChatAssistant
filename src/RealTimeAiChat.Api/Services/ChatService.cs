using Microsoft.EntityFrameworkCore;
using RealTimeAiChat.Api.Data;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Api.Services;

/// <summary>
/// Service for managing chat sessions and messages
/// </summary>
public class ChatService(AppDbContext context, ILogger<ChatService> logger) : IChatService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ChatService> _logger = logger;

	public async Task<Message> SaveMessageAsync(
        string sessionId,
        string role,
        string content,
        string? metadata = null)
    {
        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionId,
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Update session's UpdatedAt timestamp
        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Saved message {MessageId} for session {SessionId}, role: {Role}",
            message.Id, sessionId, role);

        return message;
    }

    public async Task<List<Message>> GetSessionHistoryAsync(string sessionId, int maxMessages = 50)
    {
        return await _context.Messages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.Timestamp)
            .Take(maxMessages)
            .OrderBy(m => m.Timestamp) // Re-order chronologically
            .ToListAsync();
    }

    public async Task<ChatSession> CreateSessionAsync(string? title = null)
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid().ToString(),
            Title = title ?? "New Chat",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new session {SessionId} with title: {Title}", session.Id, session.Title);

        return session;
    }

    public async Task<ChatSession?> GetSessionAsync(string sessionId)
    {
        return await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task UpdateSessionAsync(string sessionId, string? title = null)
    {
        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found for update", sessionId);
            return;
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            session.Title = title;
        }

        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated session {SessionId}", sessionId);
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        var session = await _context.ChatSessions.FindAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found for deletion", sessionId);
            return false;
        }

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted session {SessionId}", sessionId);
        return true;
    }

    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        return await _context.ChatSessions
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }
}
