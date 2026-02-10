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
public class ChatSessionsController(
	IChatService chatService,
	ILogger<ChatSessionsController> logger) : ControllerBase
{
    private readonly IChatService _chatService = chatService;
    private readonly ILogger<ChatSessionsController> _logger = logger;

    /// <summary>
    /// Maps a ChatSession domain model to a ChatSessionDto
    /// </summary>
    private static ChatSessionDto MapToDto(ChatSession session)
    {
        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            UserId = session.UserId,
            Messages = session.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                SessionId = m.SessionId,
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp,
                Metadata = m.Metadata
            }).ToList()
        };
    }

	/// <summary>
	/// Get all chat sessions
	/// </summary>
	[HttpGet]
    public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetSessions()
    {
        var sessions = await _chatService.GetAllSessionsAsync();
        var dtos = sessions.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific chat session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ChatSessionDto>> GetSession(string id)
    {
        var session = await _chatService.GetSessionAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found", id);
            return NotFound(new { message = $"Session {id} not found" });
        }
        var dto = MapToDto(session);
        return Ok(dto);
    }

    /// <summary>
    /// Create a new chat session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionDto dto)
    {
        var session = await _chatService.CreateSessionAsync(dto.Title);
        _logger.LogInformation("Created new session {SessionId}", session.Id);
        var responseDto = MapToDto(session);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, responseDto);
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
