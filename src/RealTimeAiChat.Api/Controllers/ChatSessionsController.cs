using Microsoft.AspNetCore.Mvc;
using RealTimeAiChat.Domain;
using RealTimeAiChat.Api.Services;
using RealTimeAiChat.Api.DTOs;

namespace RealTimeAiChat.Api.Controllers;

/// <summary>
/// Controller for managing chat sessions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatSessionsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatSessionsController> _logger;

    public ChatSessionsController(
        IChatService chatService,
        ILogger<ChatSessionsController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Get all chat sessions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatSession>>> GetSessions()
    {
        var sessions = await _chatService.GetAllSessionsAsync();
        return Ok(sessions);
    }

    /// <summary>
    /// Get a specific chat session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ChatSession>> GetSession(string id)
    {
        var session = await _chatService.GetSessionAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", id);
            return NotFound(new { message = $"Session {id} not found" });
        }
        return Ok(session);
    }

    /// <summary>
    /// Create a new chat session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChatSession>> CreateSession([FromBody] CreateSessionDto dto)
    {
        var session = await _chatService.CreateSessionAsync(dto.Title);
        _logger.LogInformation("Created new session {SessionId}", session.Id);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    /// <summary>
    /// Update a chat session
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateSession(string id, [FromBody] UpdateSessionDto dto)
    {
        var session = await _chatService.GetSessionAsync(id);
        if (session == null)
        {
            return NotFound(new { message = $"Session {id} not found" });
        }

        await _chatService.UpdateSessionAsync(id, dto.Title);
        _logger.LogInformation("Updated session {SessionId}", id);
        return NoContent();
    }

    /// <summary>
    /// Delete a chat session
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(string id)
    {
        var result = await _chatService.DeleteSessionAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Session {id} not found" });
        }
        _logger.LogInformation("Deleted session {SessionId}", id);
        return NoContent();
    }
}
