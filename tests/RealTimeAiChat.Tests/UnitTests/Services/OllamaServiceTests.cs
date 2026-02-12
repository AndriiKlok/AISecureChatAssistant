using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RealTimeAiChat.Api.Services;
using RealTimeAiChat.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace RealTimeAiChat.Tests.UnitTests.Services;

public class OllamaServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<OllamaService>> _mockLogger;

    public OllamaServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _httpClient = new HttpClient();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OllamaService>>();

        _mockConfiguration.Setup(x => x["OllamaUrl"]).Returns("http://localhost:11434");
        _mockConfiguration.Setup(x => x["OllamaModel"]).Returns("llama3.2");
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var service = new OllamaService(
            _httpClient,
            _context,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_AcceptsAllDependencies()
    {
        // Arrange & Act
        var service = new OllamaService(
            _httpClient,
            _context,
            _mockConfiguration.Object,
            _mockLogger.Object);

        // Assert
        service.Should().BeAssignableTo<IOllamaService>();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _context.Database.EnsureDeleted();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
