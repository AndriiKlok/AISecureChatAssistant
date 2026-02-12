# Backend API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/RealTimeAiChat.Api/RealTimeAiChat.Api.csproj", "src/RealTimeAiChat.Api/"]
COPY ["src/RealTimeAiChat.Application/RealTimeAiChat.Application.csproj", "src/RealTimeAiChat.Application/"]
COPY ["src/RealTimeAiChat.Domain/RealTimeAiChat.Domain.csproj", "src/RealTimeAiChat.Domain/"]

# Restore dependencies
RUN dotnet restore "src/RealTimeAiChat.Api/RealTimeAiChat.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/RealTimeAiChat.Api"
RUN dotnet build "RealTimeAiChat.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealTimeAiChat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/chat.db"

ENTRYPOINT ["dotnet", "RealTimeAiChat.Api.dll"]
