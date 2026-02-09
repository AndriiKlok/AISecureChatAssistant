using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RealTimeAiChat.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace RealTimeAiChat.Api.Services;

/// <summary>
/// Service for integrating with Ollama AI (local LLaMA model)
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(
        HttpClient httpClient,
        AppDbContext context,
        IConfiguration configuration,
        ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(
        string sessionId,
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ollamaUrl = _configuration["OllamaUrl"] ?? "http://localhost:11434";
        var model = _configuration["OllamaModel"] ?? "llama3.2";

        // Try to get conversation history
        List<Domain.Message>? history = null;
        string? errorMessage = null;
        
        try
        {
            history = await _context.Messages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .Take(20)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading conversation history");
            errorMessage = "Error: Failed to load conversation history.";
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
            yield break;
        }

        // Build and send request
        await foreach (var chunk in StreamFromOllamaInternalAsync(
            ollamaUrl, model, history!, userMessage, cancellationToken))
        {
            yield return chunk;
        }
    }

    private async IAsyncEnumerable<string> StreamFromOllamaInternalAsync(
        string ollamaUrl,
        string model,
        List<Domain.Message> history,
        string userMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Build Ollama request
        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = "You are a helpful AI assistant. You provide clear, accurate, and helpful responses. You can format your responses using markdown."
            }
        };

        foreach (var msg in history)
        {
            messages.Add(new { role = msg.Role, content = msg.Content });
        }

        messages.Add(new { role = "user", content = userMessage });

        var requestBody = new
        {
            model,
            messages,
            stream = true,
            options = new { temperature = 0.7, top_p = 0.9 }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to Ollama: {Url}/api/chat", ollamaUrl);

        HttpResponseMessage? response = null;
        string? errorMsg = null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{ollamaUrl}/api/chat")
            {
                Content = content
            };

            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error connecting to Ollama");
            errorMsg = $"Error: Cannot connect to Ollama at {ollamaUrl}. Please ensure Ollama is running.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request to Ollama");
            errorMsg = "Error: Failed to send request to AI service.";
        }

        if (errorMsg != null)
        {
            yield return errorMsg;
            yield break;
        }

        // Stream response
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            stream = await response!.Content.ReadAsStreamAsync(cancellationToken);
            reader = new StreamReader(stream);
            var fullResponse = new StringBuilder();

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                using var jsonResponse = JsonDocument.Parse(line);
                var root = jsonResponse.RootElement;

                if (root.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
                {
                    _logger.LogInformation("Ollama stream completed. Total: {Length} chars", fullResponse.Length);
                    break;
                }

                if (root.TryGetProperty("message", out var messageElement) &&
                    messageElement.TryGetProperty("content", out var contentElement))
                {
                    var chunk = contentElement.GetString();
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        fullResponse.Append(chunk);
                        yield return chunk;
                    }
                }
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
        }
    }
}

