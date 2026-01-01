# Dawning Identity Gateway

<p align="center">
  <img src="apps/admin/src/assets/images/logo-full.svg" alt="Dawning Logo" width="320" />
</p>

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Vue](https://img.shields.io/badge/Vue-3.5-4FC08D?logo=vue.js)
![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript)
![MySQL](https://img.shields.io/badge/MySQL-8.x-4479A1?logo=mysql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7.x-DC382D?logo=redis&logoColor=white)
![Kafka](https://img.shields.io/badge/Kafka-3.x-231F20?logo=apachekafka)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)
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
- 🐳 **Container Deployment** - Docker Compose one-click deployment

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

# Copy environment configuration
cp .env.example .env

# Start infrastructure
docker compose up -d mysql redis zookeeper kafka

# Start all services
docker compose --profile all up -d --build
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

# Copy environment configuration
cp .env.example .env

# Start infrastructure
docker compose up -d mysql redis zookeeper kafka

# Start all services
docker compose --profile all up -d --build

# Stop services
docker compose down

# Clean up data
docker compose down -v
```

## ☸️ Kubernetes One-Click Deployment

Deploy to a local Kubernetes cluster with a single command.

### Prerequisites

- Docker Desktop or Colima (running)
- Kind (`brew install kind`)
- kubectl (`brew install kubectl`)

### One-Click Start

```bash
# Run the one-click deployment script
chmod +x deploy/k8s/setup-cluster.sh
./deploy/k8s/setup-cluster.sh
```

This script will automatically:
1. Create a Kind cluster named `dawning` (1 control-plane + 3 worker nodes)
2. Build and load all Docker images
3. Deploy infrastructure (MySQL, Redis, Zookeeper, Kafka)
4. Deploy application services (Identity API, Gateway API, Frontend)
5. Configure Ingress for local access

### Access Services

After deployment completes, add hosts entries:

```bash
# Using command (recommended)
sudo sh -c 'echo "127.0.0.1 dawning.local api.dawning.local auth.dawning.local" >> /etc/hosts'
```

Access URLs:
- Frontend: http://dawning.local
- API Gateway: http://api.dawning.local
- Identity API: http://auth.dawning.local

### Useful Commands

```bash
# Check cluster status
kubectl get nodes

# Check all pods
kubectl get pods -n dawning -o wide

# View logs
kubectl logs -n dawning -l app=identity-api -f

# Delete cluster and clean up
kind delete cluster --name dawning
```

See [K8s Deployment Guide](deploy/k8s/README.md) for advanced configuration.

## 🔄 GitOps Deployment (ArgoCD + Kustomize)

Automate deployments using ArgoCD for continuous delivery from Git repository.

### Prerequisites

- Kind cluster running (see Kubernetes One-Click Deployment above)
- kubectl configured
- ArgoCD CLI (`brew install argocd`)

### Quick Setup

**⚠️ Important:** Make sure the Kind cluster `dawning` is running before proceeding.

```bash
# 1. Create Kind cluster first (if not done yet)
chmod +x deploy/k8s/setup-cluster.sh
./deploy/k8s/setup-cluster.sh

# 2. Install ArgoCD
chmod +x deploy/argocd/install-argocd.sh
./deploy/argocd/install-argocd.sh

# 3. Access ArgoCD UI
# Open: https://localhost:8080
# Username: admin
# Password: (displayed by install script)

# 4. Deploy Applications
kubectl apply -f deploy/argocd/application-dev.yaml      # Dev (auto-sync)
kubectl apply -f deploy/argocd/application-staging.yaml  # Staging (manual)
kubectl apply -f deploy/argocd/application-prod.yaml     # Production (manual)
```

### GitOps Workflow

```
Developer → Git Push → ArgoCD Detects Change → Auto/Manual Sync → K8s Cluster
```

**Benefits:**
- ✅ Git as single source of truth
- ✅ Automated sync for dev environment
- ✅ Manual approval for staging/prod
- ✅ Visual deployment tracking
- ✅ One-click rollback
- ✅ Complete audit trail

See [ArgoCD Guide](deploy/argocd/README.md) for details.

## �🔗 Business System Integration

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
