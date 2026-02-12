# 🐳 Docker Deployment Guide

## Prerequisites

⚠️ **Important:** Ollama must be running on your host machine!

```bash
# Install Ollama (if not installed)
# Windows/Mac: https://ollama.com/download
# Linux: curl -fsSL https://ollama.com/install.sh | sh

# Start Ollama service
ollama serve

# Pull LLaMA model
ollama pull llama3.2

# Verify Ollama is running
curl http://localhost:11434/api/version
```

---

## Quick Start

### 1️⃣ Build and Run All Services:
```bash
# Start backend + frontend containers
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### 2️⃣ Access Application:
- **Frontend:** http://localhost:4200
- **Backend API:** http://localhost:7001
- **Swagger UI:** http://localhost:7001/ (root path)
- **Ollama (Host):** http://localhost:11434

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────┐
│   Browser (http://localhost:4200)      │
└────────────────┬────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────┐
│   Frontend (Nginx + Angular)           │
│   Container: ai-chat-frontend          │
│   Port: 4200:80                        │
└────────────────┬────────────────────────┘
                 │ HTTP/WebSocket
                 ↓
┌─────────────────────────────────────────┐
│   Backend (.NET API + SignalR)         │
│   Container: ai-chat-backend           │
│   Port: 7001:80                        │
│   Volume: chat-db → /app/data          │
│   Connects to host via:                │
│   host.docker.internal:11434           │
└────────────────┬────────────────────────┘
                 │ HTTP Streaming
                 ↓
┌─────────────────────────────────────────┐
│   Ollama AI (HOST MACHINE)             │
│   Service: ollama serve                │
│   Port: 11434                          │
│   Model: llama3.2 (~4.7 GB)            │
└─────────────────────────────────────────┘
```

---

## 📦 Individual Service Commands

### Backend Only:
```bash
cd src/RealTimeAiChat.Api
docker build -t ai-chat-backend .
docker run -p 7001:80 \
  --add-host=host.docker.internal:host-gateway \
  -e OllamaUrl=http://host.docker.internal:11434 \
  ai-chat-backend
```

### Frontend Only:
```bash
cd src/RealTimeAiChat.Frontend
docker build -t ai-chat-frontend .
docker run -p 4200:80 ai-chat-frontend
```

---

## 🔧 Environment Variables

### Backend Container:
| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Development | ASP.NET environment (Development enables Swagger) |
| `OllamaUrl` | http://host.docker.internal:11434 | Ollama on host |
| `OllamaModel` | llama3.2 | AI model name |
| `AllowedOrigins` | http://localhost:4200 | CORS origins |
| `ConnectionStrings__DefaultConnection` | Data Source=/app/data/chat.db | SQLite path |

### Linux Users:
If `host.docker.internal` doesn't work on Linux, replace in docker-compose.yml:
```yaml
environment:
  - OllamaUrl=http://172.17.0.1:11434
```

---

## 📊 Monitoring & Logs

### View Logs:
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
```

### Health Checks:
```bash
# Frontend health
curl http://localhost:4200

# Backend health
curl http://localhost:7001/api/chatsessions

# Ollama health (host machine)
curl http://localhost:11434/api/version
```

---

## 🗄️ Data Persistence

### Volume: chat-db
- **Location:** Docker volume (managed by Docker)
- **Content:** SQLite database with all chat sessions and messages
- **Size:** ~50 KB initially, grows with conversations

### Backup Database:
```bash
# Backup
docker cp ai-chat-backend:/app/data/chat.db ./backup/chat-$(date +%Y%m%d).db

# Restore
docker cp ./backup/chat-20260212.db ai-chat-backend:/app/data/chat.db
docker-compose restart backend
```

### Reset Database:
```bash
# Delete volume and recreate
docker-compose down -v
docker-compose up -d
```

---

## 🚀 Production Deployment

### 1. HTTPS Configuration

**Option A: Use Reverse Proxy (Recommended)**
```yaml
services:
  nginx-proxy:
    image: nginxproxy/nginx-proxy
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - /var/run/docker.sock:/tmp/docker.sock:ro
      - ./certs:/etc/nginx/certs
    
  backend:
    environment:
      - VIRTUAL_HOST=api.yourdomain.com
      - VIRTUAL_PORT=80
```

**Option B: Traefik**
```yaml
services:
  traefik:
    image: traefik:v2.10
    command:
      - "--providers.docker=true"
      - "--entrypoints.web.address=:80"
      - "--entrypoints.websecure.address=:443"
    ports:
      - "80:80"
      - "443:443"
```

### 2. Use PostgreSQL Instead of SQLite

For production with multiple backend instances:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: aichat
      POSTGRES_USER: chatuser
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
  
  backend:
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=aichat;Username=chatuser;Password=${DB_PASSWORD}
    depends_on:
      - postgres
```

### 3. Resource Limits

```yaml
backend:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 2G
      reservations:
        cpus: '0.5'
        memory: 512M
```

### 4. Security Hardening

```yaml
backend:
  read_only: true
  security_opt:
    - no-new-privileges:true
  cap_drop:
    - ALL
  cap_add:
    - NET_BIND_SERVICE
```

---

## 🐛 Troubleshooting

### "Cannot connect to Ollama"

**Symptoms:** Backend logs show connection refused to Ollama

**Solutions:**

1. **Verify Ollama is running on host:**
```bash
curl http://localhost:11434/api/version
```

2. **Check firewall (Windows):**
```powershell
# Allow Ollama port
netsh advfirewall firewall add rule name="Ollama" dir=in action=allow protocol=TCP localport=11434
```

3. **Test from container:**
```bash
docker exec -it ai-chat-backend curl http://host.docker.internal:11434/api/version
```

4. **Linux users - use bridge IP:**
```bash
# Find Docker bridge IP
ip addr show docker0 | grep inet

# Update docker-compose.yml
OllamaUrl=http://172.17.0.1:11434
```

---

### "Frontend can't connect to Backend"

**Solution:** Check CORS configuration
```bash
docker-compose logs backend | grep CORS
```

Update `AllowedOrigins` if needed:
```yaml
environment:
  - AllowedOrigins=http://localhost:4200,http://yourdomain.com
```

---

### "Database locked error"

**Cause:** Multiple backend instances accessing SQLite

**Solution:** Use PostgreSQL for multiple instances (see Production section)

---

### Build fails with npm errors

**Solution:** Clear npm cache and rebuild
```bash
docker-compose down
docker-compose build --no-cache frontend
docker-compose up -d
```

---

## 📈 Scaling

### Horizontal Scaling with PostgreSQL + Redis

```yaml
services:
  postgres:
    image: postgres:16-alpine
    # ... config
  
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
  
  backend:
    deploy:
      replicas: 3
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;...
      - Redis__Configuration=redis:6379
  
  nginx-lb:
    image: nginx:alpine
    ports:
      - "7001:80"
    depends_on:
      - backend
    # ... load balancer config
```

⚠️ **Note:** SignalR needs Redis backplane for multiple instances

---

## 🔒 Security Best Practices

1. **Don't expose unnecessary ports:**
```yaml
# Remove if not needed for debugging
# ports:
#   - "7001:80"

# Access via frontend proxy only
```

2. **Use secrets for passwords:**
```yaml
services:
  backend:
    secrets:
      - db_password
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Password=/run/secrets/db_password

secrets:
  db_password:
    file: ./secrets/db_password.txt
```

3. **Regular updates:**
```bash
# Pull latest base images
docker-compose pull

# Rebuild with updates
docker-compose build --pull

# Recreate containers
docker-compose up -d
```

4. **Network isolation:**
```yaml
networks:
  ai-chat-network:
    internal: true  # No internet access for containers
  
  proxy-network:
    # Only this network exposed to internet
```

---

## 📦 Image Sizes

| Component | Base Image | Build Output | Final Size |
|-----------|------------|--------------|------------|
| Frontend | nginx:alpine | Angular build | **~50 MB** |
| Backend | dotnet/aspnet:9.0 | .NET app | **~235 MB** |

**Total Docker Images:** ~285 MB  
**Volume (chat-db):** ~50 KB (grows with usage)  
**Ollama (Host):** ~4.7 GB (not in Docker)

---

## 🧪 Development Mode

### Hot Reload for Frontend:
```bash
# Don't use Docker for frontend in dev
cd src/RealTimeAiChat.Frontend
npm install
npm start

# Use Docker only for backend
docker-compose up -d backend
```

### Watch Mode for Backend:
```bash
# Run backend locally with hot reload
cd src/RealTimeAiChat.Api
dotnet watch run
```

---

## 🎯 Common Commands Cheat Sheet

```bash
# Start everything
docker-compose up -d

# Restart after code changes
docker-compose up -d --build

# View logs (follow mode)
docker-compose logs -f

# View logs (last 100 lines)
docker-compose logs --tail=100

# Stop everything
docker-compose down

# Stop and remove volumes (full reset)
docker-compose down -v

# Check running containers
docker-compose ps

# Execute command in container
docker exec -it ai-chat-backend bash

# See resource usage
docker stats

# Clean unused images/volumes
docker system prune -a
```

---

## 📞 Support

If you encounter issues:
1. Check logs: `docker-compose logs -f`
2. Verify Ollama: `curl http://localhost:11434/api/version`
3. Check container health: `docker-compose ps`
4. Review environment variables: `docker-compose config`

---

**Production-ready with Docker! 🚀**
