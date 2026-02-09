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
