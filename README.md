# Dawning Identity Gateway

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Vue](https://img.shields.io/badge/Vue-3.5-4FC08D?logo=vue.js)
![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript)
![MySQL](https://img.shields.io/badge/MySQL-8.x-4479A1?logo=mysql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7.x-DC382D?logo=redis&logoColor=white)
![Kafka](https://img.shields.io/badge/Kafka-3.x-231F20?logo=apachekafka)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)
![Kubernetes](https://img.shields.io/badge/Kubernetes-Helm-326CE5?logo=kubernetes)
![License](https://img.shields.io/badge/License-MIT-green)

**Enterprise-grade Unified Identity Authentication & API Gateway Management Platform**

[中文](README.zh-CN.md) | English

</div>

---

## 📖 Introduction

Dawning Identity Gateway is a modern identity authentication and API gateway management system built with .NET 8 and Vue 3. It adopts Domain-Driven Design (DDD) architecture and integrates OpenIddict for OAuth 2.0/OpenID Connect standard authentication flows, providing unified identity authentication center and API gateway services for enterprises.

## ✨ Key Features

- 🔐 **OAuth 2.0 / OIDC** - Complete OpenIddict integration with multiple authorization flows
- 👥 **User Management** - User CRUD, role assignment, status management, system user protection
- 🛡️ **RBAC Permissions** - Role-based access control with fine-grained permissions
- 🔒 **Security Policies** - Password policies, login lockout, audit logging
- 📊 **Admin Dashboard** - Modern admin interface based on Arco Design Pro Vue
- 🚀 **High Performance** - Dapper ORM with optimized query builder
- 🌐 **API Gateway** - YARP-based reverse proxy with dynamic routing
- 🔗 **Multi-system Integration** - Unified authentication for other business systems
- 🐳 **Container Deployment** - Docker Compose one-click deployment, Helm Chart for K8s

## 🏗️ Tech Stack

### Backend (Dawning.Gateway)

| Technology | Description |
|------------|-------------|
| .NET 8 | Runtime Framework |
| ASP.NET Core | Web API Framework |
| OpenIddict | OAuth 2.0 / OIDC Server |
| Dapper | Lightweight ORM |
| MySQL | Database |
| AutoMapper | Object Mapping |
| xUnit + Moq | Unit Testing |

### Frontend (dawning-admin)

| Technology | Description |
|------------|-------------|
| Vue 3 | Frontend Framework |
| TypeScript | Type Safety |
| Arco Design | UI Component Library |
| Pinia | State Management |
| Vue Router | Routing |
| Vite | Build Tool |

## 📁 Project Structure

```
dawning/
├── apps/
│   ├── admin/                       # Vue 3 Admin Frontend
│   │   ├── src/
│   │   │   ├── api/                 # API Clients
│   │   │   ├── views/               # Page Views
│   │   │   ├── store/               # State Management
│   │   │   └── router/              # Route Configuration
│   │   └── config/                  # Build Configuration
│   └── gateway/                     # .NET 8 Backend Service
│       ├── src/
│       │   ├── Dawning.Gateway.Api/     # API Gateway (YARP)
│       │   ├── Dawning.Identity.Api/    # Identity Authentication API
│       │   ├── Dawning.Identity.Application/  # Application Layer
│       │   ├── Dawning.Identity.Domain/       # Domain Layer
│       │   └── Dawning.Identity.Domain.Core/  # Domain Core
│       └── docs/                    # API Documentation
├── sdk/                             # Dawning SDK Components
├── deploy/
│   ├── docker/                      # Docker Compose Configuration
│   ├── helm/                        # Kubernetes Helm Chart
│   │   └── dawning/
│   │       ├── Chart.yaml
│   │       ├── values.yaml          # Default Configuration
│   │       ├── values-dev.yaml      # Development Environment
│   │       └── values-prod.yaml     # Production Environment
│   └── scripts/                     # Deployment Scripts
├── docs/                            # Project Documentation
└── .github/workflows/               # CI/CD Configuration
```

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+
- MySQL 8.0+
- pnpm (recommended) or npm
- Docker & Docker Compose (optional)

### Option 1: Docker Compose One-Click Start (Recommended)

```bash
cd deploy/docker

# Start infrastructure
docker-compose up -d mysql redis zookeeper kafka

# Start all services
docker-compose --profile all up -d --build
```

This will start: MySQL, Redis, Zookeeper, Kafka, Kafka UI, and backend services.

### Option 2: Manual Start

**Backend**

```bash
# 1. Navigate to backend directory
cd apps/gateway

# 2. Restore dependencies
dotnet restore

# 3. Configure database connection (edit appsettings.json)

# 4. Run database migrations
mysql -u <user> -p <database> < docs/sql/migrations/001_initial_schema.sql

# 5. Start Identity API
cd src/Dawning.Identity.Api
dotnet run
```

**Frontend**

```bash
# 1. Navigate to frontend directory
cd apps/admin

# 2. Install dependencies
pnpm install

# 3. Start development server
pnpm dev
```

### Default Account

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | System Administrator |

## 🐳 Docker Deployment

```bash
cd deploy/docker

# Start infrastructure
docker-compose up -d mysql redis zookeeper kafka

# Start all services
docker-compose --profile all up -d --build

# Stop services
docker-compose down

# Clean up data
docker-compose down -v
```

## ☸️ Kubernetes Deployment

```bash
# Add Bitnami repository
helm repo add bitnami https://charts.bitnami.com/bitnami

# Update dependencies
cd deploy/helm/dawning && helm dependency update

# Development deployment
helm install dawning ./deploy/helm/dawning -f ./deploy/helm/dawning/values-dev.yaml -n dawning-dev --create-namespace

# Production deployment
helm install dawning ./deploy/helm/dawning -f ./deploy/helm/dawning/values-prod.yaml -n dawning --create-namespace
```

## 🔗 Business System Integration

Other business systems can easily integrate with unified authentication via Dawning SDK:

**1. Add NuGet Package**
```xml
<PackageReference Include="Dawning.Identity" Version="1.2.0" />
```

**2. Configure Authentication**
```csharp
builder.Services.AddDawningAuthentication(builder.Configuration);
app.UseAuthentication();
app.UseAuthorization();
```

**3. Use Authentication**
```csharp
[Authorize]
[HttpGet]
public IActionResult GetData() => Ok();
```

See [Authentication Integration Guide](docs/AUTHENTICATION_INTEGRATION.md) for details.

## 🧪 Testing

```bash
# Run unit tests
cd apps/gateway
dotnet test

# Test coverage: 52 test cases
# - UserServiceTests: 8 tests
# - RoleServiceTests: 11 tests
# - PermissionServiceTests: 9 tests
# - LoginLockoutServiceTests: 9 tests
# - PasswordPolicyServiceTests: 13 tests
```

## 📚 API Documentation

Access Swagger UI after starting the backend:
- Identity API: `http://localhost:5202/swagger`
- Gateway API: `http://localhost:5000/swagger`

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [Authentication Integration](docs/AUTHENTICATION_INTEGRATION.md) | Business system integration guide |
| [Identity API](apps/gateway/docs/IDENTITY_API.md) | OAuth2/OIDC endpoint documentation |
| [Developer Guide](docs/DEVELOPER_GUIDE.md) | Development standards and conventions |
| [Deployment Guide](docs/DEPLOYMENT.md) | Production deployment |
| [Helm Deployment](deploy/helm/README.md) | Kubernetes deployment guide |
| [User Guide](docs/USER_GUIDE.md) | Admin panel usage instructions |

## 🔧 Configuration

Main configuration file: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=dawning_identity;..."
  },
  "Security": {
    "Password": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireDigit": true
    },
    "Lockout": {
      "Enabled": true,
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 30
    }
  }
}
```

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🤝 Contributing

Issues and Pull Requests are welcome!
