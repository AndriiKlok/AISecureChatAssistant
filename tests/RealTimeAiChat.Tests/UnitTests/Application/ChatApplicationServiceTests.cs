using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RealTimeAiChat.Application.Services;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Tests.UnitTests.Application;

public class ChatApplicationServiceTests
{
    private readonly Mock<IChatDomainService> _mockDomainService;
    private readonly Mock<ILogger<ChatApplicationService>> _mockLogger;
    private readonly ChatApplicationService _service;

    public ChatApplicationServiceTests()
    {
        _mockDomainService = new Mock<IChatDomainService>();
        _mockLogger = new Mock<ILogger<ChatApplicationService>>();
        _service = new ChatApplicationService(_mockDomainService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllSessionsAsync_ReturnsAllSessions()
    {
        // Arrange
        var sessions = new List<ChatSession>
        {
            new() { Id = "1", Title = "Session 1", UpdatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = "2", Title = "Session 2", UpdatedAt = DateTime.UtcNow }
        };
        _mockDomainService.Setup(x => x.GetAllSessionsAsync())
            .ReturnsAsync(sessions);

        // Act
        var result = await _service.GetAllSessionsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be("2");
        result.Last().Id.Should().Be("1");
    }

    [Fact]
    public async Task GetSessionAsync_WithValidId_ReturnsSession()
    {
        // Arrange
        var sessionId = "test-id";
        var session = new ChatSession
        {
            Id = sessionId,
            Title = "Test Session",
            Messages =
			[
				new() { Id = "m1", Role = "user", Content = "Hello" }
            ]
        };
        _mockDomainService.Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _service.GetSessionAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sessionId);
        result.Messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSessionAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var sessionId = "invalid-id";
        _mockDomainService.Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync((ChatSession?)null);

        // Act
        var result = await _service.GetSessionAsync(sessionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_WithTitle_CreatesSession()
    {
        // Arrange
        var title = "New Chat";
        var session = new ChatSession { Id = "new-id", Title = title };
        _mockDomainService.Setup(x => x.CreateSessionAsync(title))
            .ReturnsAsync(session);

        // Act
        var result = await _service.CreateSessionAsync(title);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.Id.Should().Be("new-id");
    }

    [Fact]
    public async Task CreateSessionAsync_WithoutTitle_CreatesWithDefaultTitle()
    {
        // Arrange
        var session = new ChatSession { Id = "new-id", Title = "New Chat" };
        _mockDomainService.Setup(x => x.CreateSessionAsync(null))
            .ReturnsAsync(session);

        // Act
        var result = await _service.CreateSessionAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("new-id");
    }

    [Fact]
    public async Task UpdateSessionAsync_ExistingSession_ReturnsTrue()
    {
        // Arrange
        var sessionId = "test-id";
        var newTitle = "Updated Title";
        _mockDomainService.Setup(x => x.UpdateSessionAsync(sessionId, newTitle))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateSessionAsync(sessionId, newTitle);

        // Assert
        result.Should().BeTrue();
        _mockDomainService.Verify(x => x.UpdateSessionAsync(sessionId, newTitle), Times.Once);
    }

    [Fact]
    public async Task DeleteSessionAsync_ExistingSession_ReturnsTrue()
    {
        // Arrange
        var sessionId = "test-id";
        _mockDomainService.Setup(x => x.DeleteSessionAsync(sessionId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSessionAsync(sessionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSessionAsync_NonExistingSession_ReturnsFalse()
    {
        // Arrange
        var sessionId = "invalid-id";
        _mockDomainService.Setup(x => x.DeleteSessionAsync(sessionId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteSessionAsync(sessionId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetSessionHistoryAsync_ReturnsOrderedMessages()
    {
        // Arrange
        var sessionId = "test-id";
        var messages = new List<Message>
        {
            new() { Id = "m2", Timestamp = DateTime.UtcNow.AddMinutes(5) },
            new() { Id = "m1", Timestamp = DateTime.UtcNow }
        };
        _mockDomainService.Setup(x => x.GetSessionHistoryAsync(sessionId, 50))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetSessionHistoryAsync(sessionId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be("m1"); // Chronological order
        result.Last().Id.Should().Be("m2");
    }

    [Fact]
    public async Task MapToDto_PreventCircularReferences()
    {
        // Arrange
        var session = new ChatSession
        {
            Id = "test-id",
            Title = "Test",
            Messages =
			[
				new()
                {
                    Id = "m1",
                    SessionId = "test-id",
                    Role = "user",
                    Content = "Hello",
                    Session = null! 
                }
            ]
        };
        _mockDomainService.Setup(x => x.GetSessionAsync("test-id"))
            .ReturnsAsync(session);

        // Act
        var result = await _service.GetSessionAsync("test-id");

        // Assert
        result.Should().NotBeNull();
        result!.Messages.Should().HaveCount(1);
    }
}
