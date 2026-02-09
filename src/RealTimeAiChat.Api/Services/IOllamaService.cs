namespace RealTimeAiChat.Api.Services;

/// <summary>
/// Interface for Ollama AI service integration
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Get streaming response from Ollama AI
    /// </summary>
    /// <param name="sessionId">Chat session ID for context</param>
    /// <param name="userMessage">User's message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of text chunks from AI</returns>
    IAsyncEnumerable<string> GetStreamingResponseAsync(
        string sessionId, 
        string userMessage, 
        CancellationToken cancellationToken = default);
}
