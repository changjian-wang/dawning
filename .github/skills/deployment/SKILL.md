---
description: |
  Use when: Creating or modifying Dockerfiles (.NET 8 multi-stage, Vue 3 + Nginx), docker-compose configs, or Nginx configuration
  Don't use when:
    - Building the project (use build-project)
    - Writing application code (use code-patterns or create-api)
    - Running tests (use create-tests)
    - Writing documentation (use markdown)
    - Diagnosing runtime errors (use troubleshooting)
  Inputs: Deployment requirement or Docker config change
  Outputs: Dockerfile, docker-compose.yml, or nginx.conf
  Success criteria: Docker images build, containers start, services communicate correctly
---

# Deployment Skill

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

