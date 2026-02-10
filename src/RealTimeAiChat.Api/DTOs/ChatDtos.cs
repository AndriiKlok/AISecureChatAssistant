namespace RealTimeAiChat.Api.DTOs;

/// <summary>
/// DTO for creating a new chat session
/// </summary>
public class CreateSessionDto
{
    public string? Title { get; set; }
}

/// <summary>
/// DTO for updating a chat session
/// </summary>
public class UpdateSessionDto
{
    public string? Title { get; set; }
}

/// <summary>
/// DTO for sending a message via SignalR
/// </summary>
public class SendMessageDto
{
    public required string SessionId { get; set; }
    public required string Content { get; set; }
}

/// <summary>
/// DTO for a message in chat response
/// </summary>
public class MessageDto
{
    public required string Id { get; set; }
    public required string SessionId { get; set; }
    public required string Role { get; set; }
    public required string Content { get; set; }
    public required DateTime Timestamp { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO for a chat session response
/// </summary>
public class ChatSessionDto
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public string? UserId { get; set; }
    public required IEnumerable<MessageDto> Messages { get; set; }
}
