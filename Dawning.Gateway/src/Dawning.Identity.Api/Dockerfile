# =============================================
# Dawning Identity API - Multi-stage Dockerfile
# =============================================

# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY ["Dawning.Gateway.sln", "./"]
COPY ["src/Dawning.Identity.Api/Dawning.Identity.Api.csproj", "src/Dawning.Identity.Api/"]
COPY ["src/Dawning.Identity.Application/Dawning.Identity.Application.csproj", "src/Dawning.Identity.Application/"]
COPY ["src/Dawning.Identity.Domain/Dawning.Identity.Domain.csproj", "src/Dawning.Identity.Domain/"]
COPY ["src/Dawning.Identity.Domain.Core/Dawning.Identity.Domain.Core.csproj", "src/Dawning.Identity.Domain.Core/"]
COPY ["src/Dawning.Identity.Infra.Data/Dawning.Identity.Infra.Data.csproj", "src/Dawning.Identity.Infra.Data/"]
COPY ["src/Dawning.Identity.Infra.CrossCutting.IoC/Dawning.Identity.Infra.CrossCutting.IoC.csproj", "src/Dawning.Identity.Infra.CrossCutting.IoC/"]
COPY ["src/Dawning.Identity.Infra.CrossCutting.Mapping/Dawning.Identity.Infra.CrossCutting.Mapping.csproj", "src/Dawning.Identity.Infra.CrossCutting.Mapping/"]
COPY ["src/Shared/Dawning.Shared.Authentication/Dawning.Shared.Authentication.csproj", "src/Shared/Dawning.Shared.Authentication/"]
COPY ["src/Shared/Dawning.Shared.Core/Dawning.Shared.Core.csproj", "src/Shared/Dawning.Shared.Core/"]
COPY ["src/Shared/Dawning.Shared.Dapper.Contrib/Dawning.Shared.Dapper.Contrib.csproj", "src/Shared/Dawning.Shared.Dapper.Contrib/"]
COPY ["src/Shared/Dawning.Shared.Logging/Dawning.Shared.Logging.csproj", "src/Shared/Dawning.Shared.Logging/"]

# Restore dependencies
RUN dotnet restore "src/Dawning.Identity.Api/Dawning.Identity.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Dawning.Identity.Api"
RUN dotnet build "Dawning.Identity.Api.csproj" -c Release -o /app/build

# ===== Publish Stage =====
FROM build AS publish
RUN dotnet publish "Dawning.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Runtime Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r dawning && useradd -r -g dawning dawning

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/Logs && chown -R dawning:dawning /app

# Switch to non-root user
USER dawning

# Expose ports
EXPOSE 5001
EXPOSE 5000

# Environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TZ=Asia/Shanghai

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Dawning.Identity.Api.dll"]
