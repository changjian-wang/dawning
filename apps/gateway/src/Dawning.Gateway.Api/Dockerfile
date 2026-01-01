# =============================================
# Dawning Gateway API - Multi-stage Dockerfile
# Build from repository root: docker build -t dawning-gateway-api -f apps/gateway/src/Dawning.Gateway.Api/Dockerfile .
# =============================================

# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Directory.Build.props files (for version and framework settings)
COPY ["sdk/Directory.Build.props", "sdk/"]
COPY ["apps/gateway/Directory.Build.props", "apps/gateway/"]

# Copy SDK projects (dependencies)
COPY ["sdk/Dawning.Identity/Dawning.Identity.csproj", "sdk/Dawning.Identity/"]
COPY ["sdk/Dawning.Core/Dawning.Core.csproj", "sdk/Dawning.Core/"]
COPY ["sdk/Dawning.Extensions/Dawning.Extensions.csproj", "sdk/Dawning.Extensions/"]
COPY ["sdk/Dawning.Logging/Dawning.Logging.csproj", "sdk/Dawning.Logging/"]

# Copy gateway solution and project files
COPY ["apps/gateway/Dawning.Gateway.sln", "apps/gateway/"]
COPY ["apps/gateway/src/Dawning.Gateway.Api/Dawning.Gateway.Api.csproj", "apps/gateway/src/Dawning.Gateway.Api/"]
COPY ["apps/gateway/src/Dawning.Gateway.Middleware/Dawning.Gateway.Middleware.csproj", "apps/gateway/src/Dawning.Gateway.Middleware/"]
COPY ["apps/gateway/src/Dawning.Identity.Application/Dawning.Identity.Application.csproj", "apps/gateway/src/Dawning.Identity.Application/"]
COPY ["apps/gateway/src/Dawning.Identity.Domain/Dawning.Identity.Domain.csproj", "apps/gateway/src/Dawning.Identity.Domain/"]
COPY ["apps/gateway/src/Dawning.Identity.Domain.Core/Dawning.Identity.Domain.Core.csproj", "apps/gateway/src/Dawning.Identity.Domain.Core/"]
COPY ["apps/gateway/src/Dawning.Identity.Infra.Data/Dawning.Identity.Infra.Data.csproj", "apps/gateway/src/Dawning.Identity.Infra.Data/"]
COPY ["apps/gateway/src/Dawning.Identity.Infra.CrossCutting.IoC/Dawning.Identity.Infra.CrossCutting.IoC.csproj", "apps/gateway/src/Dawning.Identity.Infra.CrossCutting.IoC/"]
COPY ["apps/gateway/src/Dawning.Identity.Infra.CrossCutting.Mapping/Dawning.Identity.Infra.CrossCutting.Mapping.csproj", "apps/gateway/src/Dawning.Identity.Infra.CrossCutting.Mapping/"]
COPY ["apps/gateway/src/Dawning.Identity.Infra.Messaging/Dawning.Identity.Infra.Messaging.csproj", "apps/gateway/src/Dawning.Identity.Infra.Messaging/"]

# Copy nuget.config if exists
COPY ["nuget.config", "./"]

# Restore dependencies
RUN dotnet restore "apps/gateway/src/Dawning.Gateway.Api/Dawning.Gateway.Api.csproj"

# Copy all source code
COPY ["sdk/", "sdk/"]
COPY ["apps/gateway/", "apps/gateway/"]

# Build the application
WORKDIR "/src/apps/gateway/src/Dawning.Gateway.Api"
RUN dotnet build "Dawning.Gateway.Api.csproj" -c Release -o /app/build

# ===== Publish Stage =====
FROM build AS publish
RUN dotnet publish "Dawning.Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Runtime Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security with home directory
RUN groupadd -r dawning && useradd -r -g dawning -d /home/dawning -m dawning

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory and ensure proper permissions
RUN mkdir -p /app/Logs && chown -R dawning:dawning /app && chown -R dawning:dawning /home/dawning

# Switch to non-root user
USER dawning

# Expose ports
EXPOSE 5000

# Environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TZ=Asia/Shanghai

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Dawning.Gateway.Api.dll"]
