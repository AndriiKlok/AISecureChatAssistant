using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealTimeAiChat.Api.Data;
using RealTimeAiChat.Api.Hubs;
using RealTimeAiChat.Api.Services;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Tests.IntegrationTests.Hubs;

public class ChatHubTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IOllamaService> _mockOllamaService;
    private readonly Mock<ILogger<ChatHub>> _mockLogger;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ISingleClientProxy> _mockCaller;
    private readonly Mock<IGroupManager> _mockGroupManager;
    private readonly ChatHub _hub;

    public ChatHubTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockChatService = new Mock<IChatService>();
        _mockOllamaService = new Mock<IOllamaService>();
        _mockLogger = new Mock<ILogger<ChatHub>>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockCaller = new Mock<ISingleClientProxy>();
        _mockGroupManager = new Mock<IGroupManager>();

        _mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(x => x.Caller).Returns(_mockCaller.Object);

        _hub = new ChatHub(_context, _mockChatService.Object, _mockOllamaService.Object, _mockLogger.Object)
        {
            Clients = _mockClients.Object,
            Groups = _mockGroupManager.Object,
            Context = CreateMockHubCallerContext()
        };
    }

    [Fact]
    public async Task SendMessage_SavesUserMessage()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var userMessage = "Hello, AI!";
        var savedMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionId,
            Role = "user",
            Content = userMessage
        };

        _mockChatService
            .Setup(x => x.SaveMessageAsync(sessionId, "user", userMessage, null))
            .ReturnsAsync(savedMessage);

        _mockChatService
            .Setup(x => x.SaveMessageAsync(sessionId, "assistant", It.IsAny<string>(), null))
            .ReturnsAsync(new Message { Id = Guid.NewGuid().ToString() });

        _mockOllamaService
            .Setup(x => x.GetStreamingResponseAsync(sessionId, userMessage, It.IsAny<CancellationToken>()))
            .Returns(GetTestAsyncEnumerable(["AI", " response"]));

        // Act
        await _hub.SendMessage(sessionId, userMessage);

        // Assert
        _mockChatService.Verify(
            x => x.SaveMessageAsync(sessionId, "user", userMessage, null), 
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_BroadcastsUserMessageToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var userMessage = "Test message";
        var savedMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionId,
            Role = "user",
            Content = userMessage,
            Timestamp = DateTime.UtcNow
        };

        _mockChatService
            .Setup(x => x.SaveMessageAsync(sessionId, "user", userMessage, null))
            .ReturnsAsync(savedMessage);

        _mockChatService
            .Setup(x => x.SaveMessageAsync(sessionId, "assistant", It.IsAny<string>(), null))
            .ReturnsAsync(new Message { Id = Guid.NewGuid().ToString() });

        _mockOllamaService
            .Setup(x => x.GetStreamingResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(GetTestAsyncEnumerable(["Response"]));

        // Act
        await _hub.SendMessage(sessionId, userMessage);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object[]>(o => o.Length > 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_NotifiesAiThinking()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var userMessage = "Test";

        _mockChatService
            .Setup(x => x.SaveMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync(new Message { Id = Guid.NewGuid().ToString() });

        _mockOllamaService
            .Setup(x => x.GetStreamingResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(GetTestAsyncEnumerable(["Response"]));

        // Act
        await _hub.SendMessage(sessionId, userMessage);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "AiThinking",
                It.Is<object[]>(o => o.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendMessage_StreamsChunksToClients()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var chunks = new[] { "First", " Second", " Third" };

        _mockChatService
            .Setup(x => x.SaveMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync(new Message { Id = Guid.NewGuid().ToString() });

        _mockOllamaService
            .Setup(x => x.GetStreamingResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(GetTestAsyncEnumerable(chunks));

        // Act
        await _hub.SendMessage(sessionId, "Test");

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "StreamChunk",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(chunks.Length));
    }

    [Fact]
    public async Task SendMessage_SavesCompleteAIResponse()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var chunks = new[] { "Hello", " World" };
        var expectedFullResponse = "Hello World";

        _mockChatService
            .Setup(x => x.SaveMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync(new Message { Id = Guid.NewGuid().ToString() });

        _mockOllamaService
            .Setup(x => x.GetStreamingResponseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(GetTestAsyncEnumerable(chunks));

        // Act
        await _hub.SendMessage(sessionId, "Test");

        // Assert
        _mockChatService.Verify(
            x => x.SaveMessageAsync(sessionId, "assistant", expectedFullResponse, null),
            Times.Once);
    }

    [Fact]
    public async Task JoinSession_AddsToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

        // Act
        await _hub.JoinSession(sessionId);

        // Assert
        _mockGroupManager.Verify(
            x => x.AddToGroupAsync(It.IsAny<string>(), sessionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Helper method to create async enumerable for testing streaming
    private static async IAsyncEnumerable<string> GetTestAsyncEnumerable(IEnumerable<string> items)
    {
        await Task.Delay(1); // Simulate async operation
        foreach (var item in items)
        {
            yield return item;
        }
    }

    // Helper method to create mock HubCallerContext
    private static HubCallerContext CreateMockHubCallerContext()
    {
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(x => x.ConnectionId).Returns(Guid.NewGuid().ToString());
        return mockContext.Object;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
