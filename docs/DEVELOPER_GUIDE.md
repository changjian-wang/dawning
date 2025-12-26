# Dawning Gateway Developer Guide

**Version**: 1.0.0  
**Last Updated**: 2025-12-26

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technical Architecture](#2-technical-architecture)
3. [Development Environment Setup](#3-development-environment-setup)
4. [Project Structure](#4-project-structure)
5. [Backend Development Guide](#5-backend-development-guide)
6. [Frontend Development Guide](#6-frontend-development-guide)
7. [Database Design](#7-database-design)
8. [API Specifications](#8-api-specifications)
9. [Testing Guide](#9-testing-guide)
10. [Deployment Guide](#10-deployment-guide)

---

## 1. Project Overview

### 1.1 Introduction

Dawning Gateway is an enterprise-grade API Gateway management system providing identity authentication, authorization management, API routing, and traffic control.

### 1.2 Core Features

- **Identity Authentication** - OpenIddict-based OAuth 2.0 / OIDC
- **Authorization Management** - RBAC permission system
- **API Gateway** - YARP reverse proxy
- **Traffic Control** - Rate limiting and circuit breaker
- **Monitoring & Alerting** - Real-time metrics and alerts

### 1.3 Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 8, ASP.NET Core |
| Frontend | Vue 3, TypeScript, Arco Design |
| Database | MySQL 8.x |
| Cache | Redis 7.x |
| Message Queue | Apache Kafka |
| Container | Docker, Kubernetes |

---

## 2. Technical Architecture

### 2.1 System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Load Balancer                          │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Dawning Gateway                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  Identity   │  │  Admin API  │  │    YARP Gateway     │  │
│  │   Server    │  │             │  │                     │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
         │                 │                    │
    ┌────┴────┐       ┌────┴────┐         ┌────┴────┐
    │  MySQL  │       │  Redis  │         │  Kafka  │
    └─────────┘       └─────────┘         └─────────┘
```

### 2.2 Authentication Flow

1. Client requests token from `/connect/token`
2. OpenIddict validates credentials
3. Returns JWT access token
4. Client includes token in subsequent requests
5. Gateway validates token and forwards request

---

## 3. Development Environment Setup

### 3.1 Prerequisites

- .NET SDK 8.0+
- Node.js 18+
- pnpm 8+
- MySQL 8.x
- Redis 7.x
- Docker (optional)

### 3.2 Clone Repository

```bash
git clone https://github.com/changjian-wang/dawning.git
cd dawning
```

### 3.3 Backend Setup

```bash
cd apps/gateway/src/Dawning.Gateway
dotnet restore
dotnet build
```

### 3.4 Frontend Setup

```bash
cd apps/admin
pnpm install
pnpm dev
```

### 3.5 Database Setup

```bash
# Create database
mysql -u root -p < apps/gateway/docs/sql/schema/001_create_users_table.sql
# ... execute all schema scripts in order
# Then execute seed scripts
mysql -u root -p < apps/gateway/docs/sql/seed/001_init_admin.sql
```

### 3.6 Configuration

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Database=dawning_identity;User=root;Password=your_password;",
    "Redis": "localhost:6379"
  }
}
```

---

## 4. Project Structure

```
dawning/
├── apps/
│   ├── admin/              # Vue 3 frontend
│   │   ├── src/
│   │   │   ├── api/        # API clients
│   │   │   ├── components/ # Shared components
│   │   │   ├── views/      # Page views
│   │   │   ├── router/     # Vue Router
│   │   │   ├── store/      # Pinia stores
│   │   │   └── locale/     # i18n files
│   │   └── package.json
│   │
│   └── gateway/            # .NET backend
│       └── src/
│           └── Dawning.Gateway/
│               ├── Controllers/
│               ├── Services/
│               ├── Models/
│               ├── Stores/     # Dapper stores
│               └── Program.cs
│
├── sdk/                    # Shared libraries
│   ├── Dawning.Core/
│   ├── Dawning.Identity/
│   ├── Dawning.Logging/
│   └── ...
│
├── deploy/                 # Deployment files
│   ├── docker/
│   └── helm/
│
└── docs/                   # Documentation
```

---

## 5. Backend Development Guide

### 5.1 Adding a New API Endpoint

1. Create controller:

```csharp
[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class MyController : ControllerBase
{
    [HttpGet]
    [RequirePermission("my:read")]
    public async Task<IActionResult> GetAll()
    {
        // Implementation
    }
}
```

2. Create service and store as needed

### 5.2 Permission System

Use `[RequirePermission]` attribute:

```csharp
[RequirePermission("resource:action")]
[RequirePermission("resource:action:scope")]
```

### 5.3 Database Access

Use Dapper with the store pattern:

```csharp
public class MyStore
{
    private readonly IDbConnection _db;
    
    public async Task<MyEntity?> GetByIdAsync(string id)
    {
        return await _db.QueryFirstOrDefaultAsync<MyEntity>(
            "SELECT * FROM my_table WHERE id = @Id",
            new { Id = id });
    }
}
```

---

## 6. Frontend Development Guide

### 6.1 Creating a New Page

1. Create view component in `src/views/`
2. Add route in `src/router/`
3. Create locale files for i18n
4. Add menu item if needed

### 6.2 API Integration

```typescript
// src/api/my-module/index.ts
import axios from 'axios';

export interface MyEntity {
  id: string;
  name: string;
}

export function getAll() {
  return axios.get<MyEntity[]>('/api/admin/my');
}
```

### 6.3 Internationalization

All user-facing text must use i18n:

```vue
<template>
  <span>{{ $t('my.label') }}</span>
</template>
```

Add translations:
- `locale/en-US.ts` - English
- `locale/zh-CN.ts` - Chinese

---

## 7. Database Design

### 7.1 Core Tables

| Table | Description |
|-------|-------------|
| users | User accounts |
| roles | Role definitions |
| user_roles | User-role mappings |
| permissions | Permission definitions |
| openiddict_applications | OAuth clients |
| openiddict_tokens | Access tokens |

### 7.2 Naming Conventions

- Tables: `snake_case` (e.g., `user_roles`)
- Columns: `snake_case` (e.g., `created_at`)
- Primary keys: `id` (UUID)
- Foreign keys: `{table}_id`

---

## 8. API Specifications

### 8.1 Response Format

All APIs return unified response:

```json
{
  "success": true,
  "code": 0,
  "message": "Success",
  "data": { }
}
```

### 8.2 Pagination

```json
{
  "success": true,
  "data": {
    "items": [],
    "total": 100,
    "page": 1,
    "pageSize": 20
  }
}
```

### 8.3 Error Codes

| Code | Description |
|------|-------------|
| 0 | Success |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Internal Error |

---

## 9. Testing Guide

### 9.1 Backend Tests

```bash
cd apps/gateway
dotnet test
```

### 9.2 Frontend Tests

```bash
cd apps/admin
pnpm test:unit
pnpm test:e2e
```

### 9.3 API Testing

Use the Swagger UI at `/swagger` in development mode.

---

## 10. Deployment Guide

### 10.1 Docker

```bash
cd deploy/docker
docker-compose up -d
```

### 10.2 Kubernetes

```bash
cd deploy/helm/dawning
helm install dawning .
```

See [Deployment Guide](DEPLOYMENT.md) for detailed instructions.

---

*For user instructions, see the [User Guide](USER_GUIDE.md).*
*For administration, see the [Administrator Guide](ADMIN_GUIDE.md).*
