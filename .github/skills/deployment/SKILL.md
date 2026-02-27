---
description: "Docker and deployment configuration for Dawning: Dockerfile, docker-compose, Nginx. Trigger: docker, 部署, deploy, dockerfile, docker-compose, 容器, container, nginx"
---

# Deployment Skill

## 目标

为 Dawning 项目创建 Docker 配置和部署文件。

## 触发条件

- **关键词**：docker, 部署, deploy, dockerfile, docker-compose, 容器, container, nginx, k8s
- **文件模式**：`Dockerfile`, `docker-compose*.yml`, `nginx.conf`, `deploy/**`
- **用户意图**：容器化部署、创建 Docker 配置、配置 Nginx

## 编排

- **前置**：`build-project`（构建通过后部署）
- **后续**：无

---

## .NET 8 Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Dawning.Gateway/Dawning.Gateway.csproj", "src/Dawning.Gateway/"]
RUN dotnet restore "src/Dawning.Gateway/Dawning.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Dawning.Gateway"
RUN dotnet publish "Dawning.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Dawning.Gateway.dll"]
```

## Vue 3 Dockerfile

```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
RUN npm install -g pnpm
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY . .
RUN pnpm build

FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## docker-compose 开发环境

```yaml
version: '3.8'
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-root123}
      MYSQL_DATABASE: dawning
    ports: ["3306:3306"]
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]
    volumes:
      - redis_data:/data

volumes:
  mysql_data:
  redis_data:
```

## 验收场景

- **输入**："帮我创建 Gateway 的 Dockerfile"
- **预期**：agent 生成多阶段 Dockerfile（build → publish → runtime），非 root 用户
- **上次验证**：2026-02-27 ✅
