using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Api.Services;

/// <summary>
/// Interface for chat service business logic
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Save a message to the database
    /// </summary>
    Task<Message> SaveMessageAsync(string sessionId, string role, string content, string? metadata = null);

    /// <summary>
    /// Get chat session history
    /// </summary>
    Task<IEnumerable<Message>> GetSessionHistoryAsync(string sessionId, int maxMessages = 50);

    /// <summary>
    /// Create a new chat session
    /// </summary>
    Task<ChatSession> CreateSessionAsync(string? title = null);

    /// <summary>
    /// Get chat session by ID
    /// </summary>
    Task<ChatSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Update session title and UpdatedAt timestamp
    /// </summary>
    Task UpdateSessionAsync(string sessionId, string? title = null);

    /// <summary>
    /// Delete chat session and all its messages
    /// </summary>
    Task<bool> DeleteSessionAsync(string sessionId);

    /// <summary>
    /// Get all sessions ordered by UpdatedAt
    /// </summary>
    Task<IEnumerable<ChatSession>> GetAllSessionsAsync();
}
