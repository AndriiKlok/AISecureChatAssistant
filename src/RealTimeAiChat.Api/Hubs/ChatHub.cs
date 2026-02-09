using Microsoft.AspNetCore.SignalR;
using RealTimeAiChat.Domain;
using RealTimeAiChat.Api.Data;
using RealTimeAiChat.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace RealTimeAiChat.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time chat communication
/// Handles WebSocket connections and streams AI responses
/// </summary>
public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    private readonly IChatService _chatService;
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        AppDbContext context,
        IChatService chatService,
        IOllamaService ollamaService,
        ILogger<ChatHub> logger)
    {
        _context = context;
        _chatService = chatService;
        _ollamaService = ollamaService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message and get streaming AI response
    /// </summary>
    public async Task SendMessage(string sessionId, string userMessage)
    {
        try
        {
            _logger.LogInformation(
                "User {ConnectionId} sending message to session {SessionId}",
                Context.ConnectionId, sessionId);

            // Save user message
            var userMsg = await _chatService.SaveMessageAsync(sessionId, "user", userMessage);

            // Broadcast user message to all clients in the session
            await Clients.Group(sessionId).SendAsync("ReceiveMessage", new
            {
                userMsg.Id,
                userMsg.SessionId,
                userMsg.Role,
                userMsg.Content,
                userMsg.Timestamp
            });

            // Notify clients that AI is thinking
            await Clients.Group(sessionId).SendAsync("AiThinking", true);

            // Stream AI response
            var fullResponse = new StringBuilder();
            var assistantMessageId = Guid.NewGuid().ToString();

            // Notify start of streaming
            await Clients.Group(sessionId).SendAsync("StreamStart", new
            {
                Id = assistantMessageId,
                SessionId = sessionId,
                Role = "assistant"
            });

            await foreach (var chunk in _ollamaService.GetStreamingResponseAsync(sessionId, userMessage))
            {
                fullResponse.Append(chunk);

                // Stream each chunk to clients
                await Clients.Group(sessionId).SendAsync("StreamChunk", new
                {
                    Id = assistantMessageId,
                    Content = chunk
                });
            }

            // Save complete AI response
            var assistantMsg = await _chatService.SaveMessageAsync(
                sessionId,
                "assistant",
                fullResponse.ToString());

            // Notify clients stream is complete
            await Clients.Group(sessionId).SendAsync("StreamComplete", new
            {
                assistantMsg.Id,
                assistantMsg.SessionId,
                assistantMsg.Role,
                assistantMsg.Content,
                assistantMsg.Timestamp
            });

            // Turn off thinking indicator
            await Clients.Group(sessionId).SendAsync("AiThinking", false);

            _logger.LogInformation(
                "AI response completed for session {SessionId}. Response length: {Length}",
                sessionId, fullResponse.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message in session {SessionId}", sessionId);

            await Clients.Group(sessionId).SendAsync("Error", new
            {
                Message = "An error occurred while processing your message.",
                Details = ex.Message
            });

            await Clients.Group(sessionId).SendAsync("AiThinking", false);
        }
    }

    /// <summary>
    /// Join a chat session (adds connection to SignalR group)
    /// </summary>
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        _logger.LogInformation(
            "Connection {ConnectionId} joined session {SessionId}",
            Context.ConnectionId, sessionId);

        // Load and send chat history
        var history = await _context.Messages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Id,
                m.SessionId,
                m.Role,
                m.Content,
                m.Timestamp
            })
            .ToListAsync();

        await Clients.Caller.SendAsync("LoadHistory", history);

        _logger.LogInformation(
            "Loaded {Count} messages for session {SessionId}",
            history.Count, sessionId);
    }

    /// <summary>
    /// Leave a chat session (removes connection from SignalR group)
    /// </summary>
    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

        _logger.LogInformation(
            "Connection {ConnectionId} left session {SessionId}",
            Context.ConnectionId, sessionId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Client disconnected: {ConnectionId}. Exception: {Exception}",
            Context.ConnectionId, exception?.Message ?? "None");
        await base.OnDisconnectedAsync(exception);
    }
}
