using RealTimeAiChat.Application.DTOs;
using RealTimeAiChat.Domain;
using Microsoft.Extensions.Logging;

namespace RealTimeAiChat.Application.Services;

/// <summary>
/// Implementation of chat application service
/// Handles DTO mapping and coordination between multiple domain services
/// </summary>
public class ChatApplicationService(
    IChatDomainService chatDomainService,
    ILogger<ChatApplicationService> logger) : IChatApplicationService
{
    private readonly IChatDomainService _chatDomainService = chatDomainService;
    private readonly ILogger<ChatApplicationService> _logger = logger;

    /// <summary>
    /// Maps a Message domain model to MessageDto (prevents circular references)
    /// </summary>
    private static MessageDto MapMessageToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            SessionId = message.SessionId,
            Role = message.Role,
            Content = message.Content,
            Timestamp = message.Timestamp,
            Metadata = message.Metadata
        };
    }

    /// <summary>
    /// Maps a ChatSession domain model to ChatSessionDto (prevents circular references)
    /// </summary>
    private static ChatSessionDto MapSessionToDto(ChatSession session)
    {
        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            UserId = session.UserId,
            Messages = session.Messages
                .Select(MapMessageToDto)
                .OrderBy(m => m.Timestamp)
                .ToList()
        };
    }

    /// <summary>
    /// Get all chat sessions as DTOs
    /// </summary>
    public async Task<IEnumerable<ChatSessionDto>> GetAllSessionsAsync()
    {
        try
        {
            var sessions = await _chatDomainService.GetAllSessionsAsync();
            return sessions
                .Select(MapSessionToDto)
                .OrderByDescending(s => s.UpdatedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all sessions");
            throw;
        }
    }

    /// <summary>
    /// Get a specific chat session as DTO by ID
    /// </summary>
    public async Task<ChatSessionDto?> GetSessionAsync(string id)
    {
        try
        {
            var session = await _chatDomainService.GetSessionAsync(id);
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", id);
                return null;
            }

            return MapSessionToDto(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new chat session
    /// </summary>
    public async Task<ChatSessionDto> CreateSessionAsync(string? title = null)
    {
        try
        {
            var session = await _chatDomainService.CreateSessionAsync(title);
            _logger.LogInformation("Created new session {SessionId}", session.Id);
            return MapSessionToDto(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            throw;
        }
    }

    /// <summary>
    /// Update a chat session title
    /// </summary>
    public async Task<bool> UpdateSessionAsync(string id, string? title)
    {
        try
        {
            await _chatDomainService.UpdateSessionAsync(id, title);
            _logger.LogInformation("Updated session {SessionId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", id);
            return false;
        }
    }

    /// <summary>
    /// Delete a chat session
    /// </summary>
    public async Task<bool> DeleteSessionAsync(string id)
    {
        try
        {
            var result = await _chatDomainService.DeleteSessionAsync(id);
            if (result)
            {
                _logger.LogInformation("Deleted session {SessionId}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", id);
            return false;
        }
    }

    /// <summary>
    /// Get messages for a session as DTOs
    /// </summary>
    public async Task<IEnumerable<MessageDto>> GetSessionHistoryAsync(string sessionId, int maxMessages = 50)
    {
        try
        {
            var messages = await _chatDomainService.GetSessionHistoryAsync(sessionId, maxMessages);
            return messages
                .Select(MapMessageToDto)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for session {SessionId}", sessionId);
            throw;
        }
    }
}
