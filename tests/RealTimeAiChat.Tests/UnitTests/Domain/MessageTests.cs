using FluentAssertions;
using RealTimeAiChat.Domain;

namespace RealTimeAiChat.Tests.UnitTests.Domain;

public class MessageTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var message = new Message();

        // Assert
        message.Id.Should().NotBeNullOrEmpty();
        message.SessionId.Should().Be(string.Empty);
        message.Role.Should().Be(string.Empty);
        message.Content.Should().Be(string.Empty);
        message.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        message.Metadata.Should().BeNull();
    }

    [Fact]
    public void Role_CanBeUser()
    {
		// Arrange
		var message = new Message
		{
			// Act
			Role = "user"
		};

		// Assert
		message.Role.Should().Be("user");
    }

    [Fact]
    public void Role_CanBeAssistant()
    {
		// Arrange
		var message = new Message
		{
			// Act
			Role = "assistant"
		};

		// Assert
		message.Role.Should().Be("assistant");
    }

    [Fact]
    public void Content_CanStoreText()
    {
        // Arrange
        var message = new Message();
        var content = "Hello, how can I help you?";

        // Act
        message.Content = content;

        // Assert
        message.Content.Should().Be(content);
    }

    [Fact]
    public void Metadata_CanBeNull()
    {
        // Arrange & Act
        var message = new Message { Metadata = null };

        // Assert
        message.Metadata.Should().BeNull();
    }

    [Fact]
    public void Metadata_CanStoreJson()
    {
        // Arrange
        var message = new Message();
        var metadata = "{\"confidence\": 0.95}";

        // Act
        message.Metadata = metadata;

        // Assert
        message.Metadata.Should().Be(metadata);
    }

    [Fact]
    public void SessionId_LinksToSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
		var message = new Message
		{
			// Act
			SessionId = sessionId
		};

		// Assert
		message.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void Timestamp_IsUtcTime()
    {
        // Arrange & Act
        var message = new Message();

        // Assert
        message.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Id_IsGeneratedAsGuid()
    {
        // Arrange & Act
        var message1 = new Message();
        var message2 = new Message();

        // Assert
        message1.Id.Should().NotBe(message2.Id);
        Guid.TryParse(message1.Id, out _).Should().BeTrue();
    }
}
