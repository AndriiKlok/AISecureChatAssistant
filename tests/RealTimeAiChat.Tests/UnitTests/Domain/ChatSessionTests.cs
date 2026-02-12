using FluentAssertions;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Tests.UnitTests.Domain;

public class ChatSessionTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var session = new ChatSession();

        // Assert
        session.Id.Should().NotBeNullOrEmpty();
        session.Title.Should().Be(string.Empty);
        session.Messages.Should().NotBeNull();
        session.Messages.Should().BeEmpty();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Title_CanBeSet()
    {
        // Arrange
        var session = new ChatSession();
        var expectedTitle = "Test Chat";

        // Act
        session.Title = expectedTitle;

        // Assert
        session.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void Messages_CanAddMessage()
    {
        // Arrange
        var session = new ChatSession();
        var message = new Message
        {
            SessionId = session.Id,
            Role = "user",
            Content = "Hello"
        };

        // Act
        session.Messages.Add(message);

        // Assert
        session.Messages.Should().HaveCount(1);
        session.Messages.Should().Contain(message);
    }

    [Fact]
    public void UserId_CanBeNull()
    {
        // Arrange & Act
        var session = new ChatSession { UserId = null };

        // Assert
        session.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_CanBeSet()
    {
        // Arrange
        var session = new ChatSession();
        var userId = "user123";

        // Act
        session.UserId = userId;

        // Assert
        session.UserId.Should().Be(userId);
    }

    [Fact]
    public void UpdatedAt_CanBeModified()
    {
        // Arrange
        var session = new ChatSession();
        var newDate = DateTime.UtcNow.AddHours(1);

        // Act
        session.UpdatedAt = newDate;

        // Assert
        session.UpdatedAt.Should().Be(newDate);
    }

    [Fact]
    public void Id_IsGeneratedAsGuid()
    {
        // Arrange & Act
        var session1 = new ChatSession();
        var session2 = new ChatSession();

        // Assert
        session1.Id.Should().NotBe(session2.Id);
        Guid.TryParse(session1.Id, out _).Should().BeTrue();
    }
}
