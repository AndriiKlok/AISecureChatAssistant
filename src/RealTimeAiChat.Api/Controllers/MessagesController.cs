using Microsoft.AspNetCore.Mvc;
using RealTimeAiChat.Domain;
using RealTimeAiChat.Api.Services;

namespace RealTimeAiChat.Api.Controllers;

/// <summary>
/// Controller for managing chat messages
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IChatService chatService,
        ILogger<MessagesController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Get all messages for a specific session
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(
        string sessionId,
        [FromQuery] int maxMessages = 50)
    {
        var messages = await _chatService.GetSessionHistoryAsync(sessionId, maxMessages);
        _logger.LogInformation(
            "Retrieved {Count} messages for session {SessionId}",
            messages.Count, sessionId);
        return Ok(messages);
    }
}
