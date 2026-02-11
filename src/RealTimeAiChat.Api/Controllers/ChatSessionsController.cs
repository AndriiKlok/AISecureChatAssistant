using Microsoft.AspNetCore.Mvc;
using RealTimeAiChat.Application.DTOs;
using RealTimeAiChat.Application.Services;

namespace RealTimeAiChat.Api.Controllers;

/// <summary>
/// Controller for managing chat sessions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatSessionsController(
	IChatApplicationService chatApplicationService,
	ILogger<ChatSessionsController> logger) : ControllerBase
{
    private readonly IChatApplicationService _chatApplicationService = chatApplicationService;
    private readonly ILogger<ChatSessionsController> _logger = logger;

	/// <summary>
	/// Get all chat sessions
	/// </summary>
	[HttpGet]
    public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetSessions()
    {
        var sessions = await _chatApplicationService.GetAllSessionsAsync();
        return Ok(sessions);
    }

    /// <summary>
    /// Get a specific chat session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ChatSessionDto>> GetSession(string id)
    {
        var session = await _chatApplicationService.GetSessionAsync(id);
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
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionDto dto)
    {
        var session = await _chatApplicationService.CreateSessionAsync(dto.Title);
        _logger.LogInformation("Created new session {SessionId}", session.Id);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    /// <summary>
    /// Update a chat session
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateSession(string id, [FromBody] UpdateSessionDto dto)
    {
        var success = await _chatApplicationService.UpdateSessionAsync(id, dto.Title);
        if (!success)
        {
            return NotFound(new { message = $"Session {id} not found" });
        }

        _logger.LogInformation("Updated session {SessionId}", id);
        return NoContent();
    }

    /// <summary>
    /// Delete a chat session
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSession(string id)
    {
        var result = await _chatApplicationService.DeleteSessionAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Session {id} not found" });
        }
        _logger.LogInformation("Deleted session {SessionId}", id);
        return NoContent();
    }
}
