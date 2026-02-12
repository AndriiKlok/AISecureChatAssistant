using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealTimeAiChat.Api.Data;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Tests.IntegrationTests.Database;

public class AppDbContextTests : IDisposable
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task SaveChanges_CreatesSession()
    {
        // Arrange
        var session = new ChatSession
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test Session",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.ChatSessions.FindAsync(session.Id);
        savedSession.Should().NotBeNull();
        savedSession!.Title.Should().Be("Test Session");
    }

    [Fact]
    public async Task SaveChanges_CreatesMessage()
    {
        // Arrange
        var session = new ChatSession { Title = "Test" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var message = new Message
        {
            SessionId = session.Id,
            Role = "user",
            Content = "Hello",
            Timestamp = DateTime.UtcNow
        };

        // Act
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Assert
        var savedMessage = await _context.Messages.FindAsync(message.Id);
        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().Be("Hello");
    }

    [Fact]
    public async Task Delete_CascadesMessages()
    {
        // Arrange
        var session = new ChatSession { Title = "Test" };
        _context.ChatSessions.Add(session);

        var message1 = new Message { SessionId = session.Id, Role = "user", Content = "Hi" };
        var message2 = new Message { SessionId = session.Id, Role = "assistant", Content = "Hello" };
        _context.Messages.AddRange(message1, message2);
        await _context.SaveChangesAsync();

        // Act
        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();

        // Assert
        var messages = await _context.Messages
            .Where(m => m.SessionId == session.Id)
            .ToListAsync();
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Messages_AreLinkToSession()
    {
        // Arrange
        var session = new ChatSession { Title = "Test" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var message = new Message
        {
            SessionId = session.Id,
            Role = "user",
            Content = "Test"
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Act
        var loadedSession = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstAsync(s => s.Id == session.Id);

        // Assert
        loadedSession.Messages.Should().HaveCount(1);
        loadedSession.Messages.First().Content.Should().Be("Test");
    }

    [Fact]
    public async Task CanQueryMessagesWithIndex()
    {
        // Arrange
        var session = new ChatSession { Title = "Test" };
        _context.ChatSessions.Add(session);

        for (int i = 0; i < 5; i++)
        {
            _context.Messages.Add(new Message
            {
                SessionId = session.Id,
                Role = "user",
                Content = $"Message {i}",
                Timestamp = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var messages = await _context.Messages
            .Where(m => m.SessionId == session.Id)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        // Assert
        messages.Should().HaveCount(5);
        messages.First().Content.Should().Be("Message 0");
    }

    [Fact]
    public async Task CanUpdateSession()
    {
        // Arrange
        var session = new ChatSession
        {
            Title = "Original Title",
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        session.Title = "Updated Title";
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var updatedSession = await _context.ChatSessions.FindAsync(session.Id);
        updatedSession!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task MultipleSessionsCanExist()
    {
        // Arrange & Act
        for (int i = 0; i < 3; i++)
        {
            _context.ChatSessions.Add(new ChatSession
            {
                Title = $"Session {i}",
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Assert
        var sessions = await _context.ChatSessions.ToListAsync();
        sessions.Should().HaveCount(3);
    }

    [Fact]
    public async Task MessageMetadata_CanStoreJson()
    {
        // Arrange
        var session = new ChatSession { Title = "Test" };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        var metadata = "{\"confidence\": 0.95, \"model\": \"llama3.2\"}";
        var message = new Message
        {
            SessionId = session.Id,
            Role = "assistant",
            Content = "Response",
            Metadata = metadata
        };

        // Act
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Assert
        var savedMessage = await _context.Messages.FindAsync(message.Id);
        savedMessage!.Metadata.Should().Be(metadata);
    }

    [Fact]
    public async Task Session_CanHaveNullUserId()
    {
        // Arrange & Act
        var session = new ChatSession
        {
            Title = "Anonymous Session",
            UserId = null
        };
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.ChatSessions.FindAsync(session.Id);
        savedSession!.UserId.Should().BeNull();
    }

	public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
