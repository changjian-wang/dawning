---
mode: agent
description: 创建 Docker 配置和部署文件
tools: ["read_file", "create_file", "semantic_search"]
---

# 创建 Docker 配置

为服务创建 Dockerfile 和 docker-compose 配置。

## Dockerfile 模板

### .NET 8 应用
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/Dawning.Gateway/Dawning.Gateway.csproj", "src/Dawning.Gateway/"]
RUN dotnet restore "src/Dawning.Gateway/Dawning.Gateway.csproj"

# Copy source and build
COPY . .
WORKDIR "/src/src/Dawning.Gateway"
RUN dotnet build "Dawning.Gateway.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Dawning.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Add non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Dawning.Gateway.dll"]
```

### Vue 3 前端
```dockerfile
# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Install pnpm
RUN npm install -g pnpm

# Copy package files
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copy source and build
COPY . .
RUN pnpm build

# Production stage
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## docker-compose 模板

### 开发环境
```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: dawning-mysql
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-root123}
      MYSQL_DATABASE: dawning
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: dawning-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  gateway:
    build:
      context: ./apps/gateway
      dockerfile: Dockerfile
    container_name: dawning-gateway
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=mysql;Database=dawning;User=root;Password=root123
      - Redis__Connection=redis:6379
    depends_on:
      mysql:
        condition: service_healthy
      redis:
        condition: service_started

  admin:
    build:
      context: ./apps/admin
      dockerfile: Dockerfile
    container_name: dawning-admin
    ports:
      - "8080:80"
    depends_on:
      - gateway

volumes:
  mysql_data:
  redis_data:
```

### 生产环境
```yaml
# docker-compose.yml
version: '3.8'

services:
  gateway:
    image: ${REGISTRY}/dawning-gateway:${TAG:-latest}
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1'
          memory: 1G
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=${DB_CONNECTION}
      - Redis__Connection=${REDIS_CONNECTION}
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  admin:
    image: ${REGISTRY}/dawning-admin:${TAG:-latest}
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '0.5'
          memory: 256M
```

## Nginx 配置

```nginx
# nginx.conf
user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;
    
    sendfile on;
    keepalive_timeout 65;
    gzip on;
    gzip_types text/plain application/json application/javascript text/css;

    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;

        # SPA 路由支持
        location / {
            try_files $uri $uri/ /index.html;
        }

        # API 代理
        location /api/ {
            proxy_pass http://gateway:8080/api/;
            proxy_http_version 1.1;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        }

        # 静态资源缓存
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
```

## .dockerignore

```
# .dockerignore
**/bin
**/obj
**/node_modules
**/dist
**/.git
**/.vs
**/.vscode
**/Dockerfile*
**/*.md
**/docker-compose*.yml
```

## 常用命令

```bash
# 构建镜像
docker build -t dawning-gateway:latest -f apps/gateway/Dockerfile .

# 启动服务
docker-compose -f deploy/docker/docker-compose.dev.yml up -d

# 查看日志
docker-compose logs -f gateway

# 停止服务
docker-compose down

# 清理
docker system prune -a
```
