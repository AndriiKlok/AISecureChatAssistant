using Microsoft.EntityFrameworkCore;
using RealTimeAiChat.Api.Data;
using RealTimeAiChat.Api.Hubs;
using RealTimeAiChat.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RealTime AI Chat API",
        Version = "v1",
        Description = "Real-time chat application with AI assistant powered by Ollama"
    });
});

// Configure SQLite Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
    options.StreamBufferCapacity = 10;
});

// Configure CORS for Angular frontend
var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') 
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// Register HttpClient for Ollama
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Long timeout for AI responses
});

// Register application services
builder.Services.AddScoped<IChatService, ChatService>();

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RealTime AI Chat API v1");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error applying database migrations");
    }
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chathub");

app.Logger.LogInformation("🚀 RealTime AI Chat API is starting...");
app.Logger.LogInformation("📡 SignalR Hub available at: /chathub");
app.Logger.LogInformation("📝 Swagger UI available at: /");
app.Logger.LogInformation("🤖 Ollama URL: {OllamaUrl}", builder.Configuration["OllamaUrl"]);

app.Run();

