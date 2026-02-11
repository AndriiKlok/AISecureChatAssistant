using RealTimeAiChat.Application.DTOs;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Application.Services;

/// <summary>
/// Application service for managing chat sessions and related operations
/// </summary>
public interface IChatApplicationService
{
    /// <summary>
    /// Get all chat sessions as DTOs
    /// </summary>
    Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync();

    /// <summary>
    /// Get a specific chat session as DTO by ID
    /// </summary>
    Task<ChatSessionDto?> GetSessionAsync(string id);

    /// <summary>
    /// Create a new chat session
    /// </summary>
    Task<ChatSessionDto> CreateSessionAsync(string? title = null);

    /// <summary>
    /// Update a chat session title
    /// </summary>
    Task<bool> UpdateSessionAsync(string id, string? title);

    /// <summary>
    /// Delete a chat session
    /// </summary>
    Task<bool> DeleteSessionAsync(string id);

    /// <summary>
    /// Get messages for a session as DTOs
    /// </summary>
    Task<IEnumerable<MessageDto>> GetSessionHistoryAsync(string sessionId, int maxMessages = 50);
}
