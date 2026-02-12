using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealTimeAiChat.Api.Data;
using RealTimeAiChat.Api.Services;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Tests.UnitTests.Services;

public class ChatServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<ChatService>> _mockLogger;
    private readonly ChatService _service;

    public ChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<ChatService>>();
        _service = new ChatService(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task SaveMessageAsync_SavesToDatabase()
    {
        // Arrange
        var session = new ChatSession { Id = Guid.NewGuid().ToString(), Title = "Test" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var message = await _service.SaveMessageAsync(session.Id, "user", "Hello");

        // Assert
        message.Should().NotBeNull();
        message.SessionId.Should().Be(session.Id);
        message.Role.Should().Be("user");
        message.Content.Should().Be("Hello");

        var savedMessage = await _context.Messages.FindAsync(message.Id);
        savedMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveMessageAsync_UpdatesSessionTimestamp()
    {
        // Arrange
        var session = new ChatSession
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test",
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var oldTimestamp = session.UpdatedAt;

        // Act
        await _service.SaveMessageAsync(session.Id, "user", "Hello");

        // Assert
        var updatedSession = await _context.ChatSessions.FindAsync(session.Id);
        updatedSession!.UpdatedAt.Should().BeAfter(oldTimestamp);
    }

    [Fact]
    public async Task GetSessionHistoryAsync_ReturnsMessagesInChronologicalOrder()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new ChatSession { Id = sessionId, Title = "Test" };
        _context.ChatSessions.Add(session);

        var message1 = new Message
        {
            SessionId = sessionId,
            Role = "user",
            Content = "First",
            Timestamp = DateTime.UtcNow.AddMinutes(-10)
        };
        var message2 = new Message
        {
            SessionId = sessionId,
            Role = "assistant",
            Content = "Second",
            Timestamp = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.Messages.AddRange(message1, message2);
        await _context.SaveChangesAsync();

        // Act
        var messages = await _service.GetSessionHistoryAsync(sessionId);

        // Assert
        messages.Should().HaveCount(2);
        messages.First().Content.Should().Be("First");
        messages.Last().Content.Should().Be("Second");
    }

    [Fact]
    public async Task GetSessionHistoryAsync_RespectsMaxMessages()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new ChatSession { Id = sessionId, Title = "Test" };
        _context.ChatSessions.Add(session);

        for (int i = 0; i < 10; i++)
        {
            _context.Messages.Add(new Message
            {
                SessionId = sessionId,
                Role = "user",
                Content = $"Message {i}",
                Timestamp = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var messages = await _service.GetSessionHistoryAsync(sessionId, maxMessages: 5);

        // Assert
        messages.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateSessionAsync_CreatesInDatabase()
    {
        // Act
        var session = await _service.CreateSessionAsync("My Chat");

        // Assert
        session.Should().NotBeNull();
        session.Title.Should().Be("My Chat");
        session.Id.Should().NotBeNullOrEmpty();

        var savedSession = await _context.ChatSessions.FindAsync(session.Id);
        savedSession.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSessionAsync_WithNullTitle_UsesDefault()
    {
        // Act
        var session = await _service.CreateSessionAsync(null);

        // Assert
        session.Title.Should().Be("New Chat");
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsSessionWithMessages()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new ChatSession { Id = sessionId, Title = "Test" };
        _context.ChatSessions.Add(session);
        _context.Messages.Add(new Message
        {
            SessionId = sessionId,
            Role = "user",
            Content = "Hello"
        });
        await _context.SaveChangesAsync();

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
        // Act
        var result = await _service.GetSessionAsync("invalid-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSessionAsync_UpdatesTitleAndTimestamp()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new ChatSession
        {
            Id = sessionId,
            Title = "Old Title",
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var oldTimestamp = session.UpdatedAt;

        // Act
        await _service.UpdateSessionAsync(sessionId, "New Title");

        // Assert
        var updatedSession = await _context.ChatSessions.FindAsync(sessionId);
        updatedSession!.Title.Should().Be("New Title");
        updatedSession.UpdatedAt.Should().BeAfter(oldTimestamp);
    }

    [Fact]
    public async Task DeleteSessionAsync_RemovesSessionAndMessages()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var session = new ChatSession { Id = sessionId, Title = "Test" };
        _context.ChatSessions.Add(session);
        _context.Messages.Add(new Message { SessionId = sessionId, Role = "user", Content = "Hi" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteSessionAsync(sessionId);

        // Assert
        result.Should().BeTrue();

        var deletedSession = await _context.ChatSessions.FindAsync(sessionId);
        deletedSession.Should().BeNull();

        var messages = await _context.Messages.Where(m => m.SessionId == sessionId).ToListAsync();
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllSessionsAsync_ReturnsOrderedByUpdatedAt()
    {
        // Arrange
        var session1 = new ChatSession { Title = "Old", UpdatedAt = DateTime.UtcNow.AddHours(-2) };
        var session2 = new ChatSession { Title = "New", UpdatedAt = DateTime.UtcNow };
        _context.ChatSessions.AddRange(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var sessions = await _service.GetAllSessionsAsync();

        // Assert
        sessions.Should().HaveCount(2);
        sessions.First().Title.Should().Be("New");
        sessions.Last().Title.Should().Be("Old");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
