namespace RealTimeAiChat.Application.Services;

/// <summary>
/// Domain service interface for chat data persistence
/// This interface abstracts the data access layer from the application layer
/// To be implemented in the API/Infrastructure layer
/// </summary>
public interface IChatDomainService
{
    /// <summary>
    /// Get all chat sessions
    /// </summary>
    Task<IEnumerable<RealTimeAiChat.Domain.ChatSession>> GetAllSessionsAsync();

    /// <summary>
    /// Get a specific chat session by ID
    /// </summary>
    Task<RealTimeAiChat.Domain.ChatSession?> GetSessionAsync(string id);

    /// <summary>
    /// Create a new chat session
    /// </summary>
    Task<RealTimeAiChat.Domain.ChatSession> CreateSessionAsync(string? title = null);

    /// <summary>
    /// Update a chat session
    /// </summary>
    Task UpdateSessionAsync(string id, string? title);

    /// <summary>
    /// Delete a chat session
    /// </summary>
    Task<bool> DeleteSessionAsync(string id);

    /// <summary>
    /// Save a message to database
    /// </summary>
    Task<RealTimeAiChat.Domain.Message> SaveMessageAsync(string sessionId, string role, string content, string? metadata = null);

    /// <summary>
    /// Get messages for a session
    /// </summary>
    Task<IEnumerable<RealTimeAiChat.Domain.Message>> GetSessionHistoryAsync(string sessionId, int maxMessages = 50);
}
