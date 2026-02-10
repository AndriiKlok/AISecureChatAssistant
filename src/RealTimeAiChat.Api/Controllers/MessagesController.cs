using Microsoft.AspNetCore.Mvc;
using RealTimeAiChat.Domain;
using RealTimeAiChat.Api.Services;
using RealTimeAiChat.Api.DTOs;

namespace RealTimeAiChat.Api.Controllers;

/// <summary>
/// Controller for managing chat messages
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessagesController(
	IChatService chatService,
	ILogger<MessagesController> logger) : ControllerBase
{
    private readonly IChatService _chatService = chatService;
    private readonly ILogger<MessagesController> _logger = logger;

    /// <summary>
    /// Maps a Message domain model to a MessageDto
    /// </summary>
    private static MessageDto MapToDto(Message message)
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
	/// Get all messages for a specific session
	/// </summary>
	[HttpGet("session/{sessionId}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(
        string sessionId,
        [FromQuery] int maxMessages = 50)
    {
        var messages = await _chatService.GetSessionHistoryAsync(sessionId, maxMessages);
        var dtos = messages.Select(MapToDto).ToList();
        _logger.LogInformation(
            "Retrieved {Count} messages for session {SessionId}",
            dtos.Count, sessionId);
        return Ok(dtos);
    }
}
