using Microsoft.AspNetCore.Mvc;
using RealTimeAiChat.Application.DTOs;
using RealTimeAiChat.Application.Services;

namespace RealTimeAiChat.Api.Controllers;

/// <summary>
/// Controller for managing chat messages
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessagesController(
	IChatApplicationService chatApplicationService,
	ILogger<MessagesController> logger) : ControllerBase
{
    private readonly IChatApplicationService _chatApplicationService = chatApplicationService;
    private readonly ILogger<MessagesController> _logger = logger;

	/// <summary>
	/// Get all messages for a specific session
	/// </summary>
	[HttpGet("session/{sessionId}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(
        string sessionId,
        [FromQuery] int maxMessages = 50)
    {
        var messages = await _chatApplicationService.GetSessionHistoryAsync(sessionId, maxMessages);
        _logger.LogInformation(
            "Retrieved {Count} messages for session {SessionId}",
            messages.Count(), sessionId);
        return Ok(messages);
    }
}
