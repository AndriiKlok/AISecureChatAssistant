# RealTimeAiChat.Tests

## Purpose

Unit and integration tests for the RealTime AI Chat application.

## Current Status

🚧 **Test infrastructure ready, tests to be implemented**

## Testing Stack

- **xUnit** - Testing framework
- **Should** / **FluentAssertions** - Test assertions (to be added)
- **Moq** - Mocking framework (to be added)

## Test Structure (Planned)

```
tests/
├── Unit/
│   ├── Services/
│   │   ├── ChatServiceTests.cs
│   │   └── OllamaServiceTests.cs
│   └── Domain/
│       └── MessageTests.cs
├── Integration/
│   ├── ChatHubTests.cs
│   └── ApiEndpointsTests.cs
└── README.md
```

## Running Tests

```bash
cd tests/RealTimeAiChat.Tests
dotnet test
```

## Future Tests

### Unit Tests
- [ ] ChatService CRUD operations
- [ ] OllamaService streaming logic
- [ ] Message validation
- [ ] Session management

### Integration Tests  
- [ ] SignalR hub communication
- [ ] Database operations
- [ ] API endpoints

## Contributing

When adding tests:
1. Follow AAA pattern (Arrange, Act, Assert)
2. Use descriptive test names
3. One assertion per test when possible
4. Mock external dependencies
