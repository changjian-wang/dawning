# Dawning Development Standards

This document defines development standards and best practices for the Dawning project to ensure code quality, consistency, and maintainability.

## Table of Contents

- [1. Project Structure](#1-project-structure)
- [2. Naming Conventions](#2-naming-conventions)
- [3. Code Style](#3-code-style)
- [4. Git Commit Standards](#4-git-commit-standards)
- [5. Backend Development (C#/.NET)](#5-backend-development-cnet)
- [6. Frontend Development (Vue/TypeScript)](#6-frontend-development-vuetypescript)
- [7. API Design Standards](#7-api-design-standards)
- [8. Database Standards](#8-database-standards)
- [9. Testing Standards](#9-testing-standards)
- [10. Documentation Standards](#10-documentation-standards)
- [11. Security Standards](#11-security-standards)
- [12. SDK Integration Guidelines](#12-sdk-integration-guidelines)

---

## 1. Project Structure

### 1.1 Overall Structure

```
dawning/
├── apps/                   # Applications
│   ├── admin/              # Frontend Admin Panel (Vue 3 + TypeScript)
│   └── gateway/            # Backend Services (ASP.NET Core)
├── sdk/                    # SDK Packages (NuGet)
├── deploy/                 # Deployment Configurations
│   ├── docker/             # Docker Compose
│   ├── k8s/                # Kubernetes
│   └── argocd/             # ArgoCD
├── docs/                   # Documentation
└── .github/workflows/      # CI/CD
```

### 1.2 Backend Layered Architecture (Clean Architecture)

```
Dawning.Identity.Api/                    # Presentation Layer (Controllers)
Dawning.Identity.Application/            # Application Layer (Services, DTOs)
├── Dtos/                                # Data Transfer Objects
├── Interfaces/                          # Service Interfaces
├── Services/                            # Service Implementations
└── Mapping/                             # AutoMapper Configuration
Dawning.Identity.Domain/                 # Domain Layer (Entities, Aggregates)
├── Aggregates/                          # Aggregate Roots
├── Events/                              # Domain Events
├── Interfaces/                          # Repository Interfaces
└── Models/                              # Domain Models
Dawning.Identity.Infra.Data/             # Infrastructure Layer (Repositories)
Dawning.Identity.Infra.CrossCutting.IoC/ # Dependency Injection Configuration
```

### 1.3 Frontend Directory Structure

```
src/
├── api/                    # API Wrappers
├── assets/                 # Static Assets
├── components/             # Shared Components
├── hooks/                  # Composables
├── locale/                 # Internationalization
├── router/                 # Routing Configuration
├── store/                  # State Management (Pinia)
├── utils/                  # Utility Functions
└── views/                  # Page Components
    └── administration/     # Organized by Module
        ├── user/
        ├── role/
        └── gateway/
```

---

## 2. Naming Conventions

### 2.1 General Rules

| Type | Convention | Example |
|------|------------|---------|
| Folders | kebab-case | `user-permission`, `rate-limit` |
| Variables/Functions | camelCase | `getUserInfo`, `isActive` |
| Constants | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `API_BASE_URL` |
| Classes/Interfaces | PascalCase | `UserService`, `IUserRepository` |

### 2.2 C# Naming Conventions

```csharp
// ✅ Correct
public class UserService { }
public interface IUserRepository { }
private readonly ILogger<UserController> _logger;
public string UserName { get; set; }
private const int MaxRetryCount = 3;

// ❌ Incorrect
public class userService { }        // Classes should be PascalCase
public interface UserRepository { } // Interfaces should start with I
private ILogger<UserController> logger; // Private fields should start with _
```

**File Naming**:
- Class files: `UserService.cs`
- Interface files: `IUserRepository.cs`
- DTO files: `UserDto.cs`, `CreateUserDto.cs`
- Controllers: `UserController.cs`

### 2.3 TypeScript/Vue Naming Conventions

```typescript
// Variables and functions: camelCase
const userName = 'admin';
function getUserInfo() { }

// Constants: UPPER_SNAKE_CASE
const MAX_RETRY_COUNT = 3;

// Types and interfaces: PascalCase
interface UserInfo { }
type HttpResponse<T> = { }

// Vue component files: PascalCase or kebab-case
// Recommended: index.vue (in separate folder)
// views/administration/user/index.vue
```

---

## 3. Code Style

### 3.1 C# Code Style

**Base Configuration** (Directory.Build.props):
```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

**Code Format**:
```csharp
// ✅ Use file-scoped namespaces
namespace Dawning.Identity.Application.Services;

// ✅ Use primary constructors (C# 12)
public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger
) : IUserService
{
    // ✅ Use expression body
    public async Task<UserDto?> GetByIdAsync(Guid id) =>
        await userRepository.GetByIdAsync(id);
}

// ✅ XML documentation comments
/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User DTO, null if not found</returns>
    Task<UserDto?> GetByIdAsync(Guid id);
}
```

### 3.2 TypeScript Code Style

**ESLint Configuration**:
- Extends `airbnb-base`
- Uses `@typescript-eslint` plugin
- Integrates Prettier

**Code Example**:
```typescript
// ✅ Use Composition API
<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { UserInfo } from '@/api/user';

const { t } = useI18n();

// Reactive state
const loading = ref(false);
const users = ref<UserInfo[]>([]);

// Computed properties
const activeUsers = computed(() => 
  users.value.filter(u => u.isActive)
);

// Lifecycle
onMounted(() => {
  fetchUsers();
});
</script>
```

---

## 4. Git Commit Standards

### 4.1 Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### 4.2 Type Categories

| Type | Description | Example |
|------|-------------|---------|
| `feat` | New feature | `feat(user): add user import feature` |
| `fix` | Bug fix | `fix(auth): fix token refresh failure` |
| `docs` | Documentation | `docs: update API documentation` |
| `style` | Code formatting | `style: format code` |
| `refactor` | Refactoring | `refactor(core): optimize caching strategy` |
| `perf` | Performance | `perf(db): optimize query performance` |
| `test` | Testing | `test(user): add user service unit tests` |
| `chore` | Build/Tools | `chore: upgrade dependencies` |
| `ci` | CI/CD | `ci: add auto deployment workflow` |

### 4.3 Scope Options

- `admin` - Frontend admin panel
- `gateway` - Gateway service
- `identity` - Identity service
- `sdk` - SDK packages
- `core` - Core modules
- `auth` - Authentication
- `user` - User management
- `deploy` - Deployment config

---

## 5. Backend Development (C#/.NET)

### 5.1 Controller Standards

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get user list
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryDto query)
    {
        var result = await _userService.GetPagedAsync(query);
        return Ok(ApiResponse.Success(result));
    }
}
```

### 5.2 Unified Response Format

```csharp
public class ApiResult<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = "OK";

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}
```

---

## 6. Frontend Development (Vue/TypeScript)

### 6.1 Component Structure

```vue
<template>
  <div class="user-list">
    <!-- Template content -->
  </div>
</template>

<script lang="ts" setup>
// 1. Imports
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { UserInfo } from '@/api/user';

// 2. Props & Emits
const props = defineProps<{
  visible: boolean;
  userId?: string;
}>();

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void;
  (e: 'success'): void;
}>();

// 3. Composables
const { t } = useI18n();

// 4. Reactive state
const loading = ref(false);

// 5. Computed properties
// 6. Methods
// 7. Lifecycle hooks
</script>

<style lang="less" scoped>
.user-list {
  // Styles
}
</style>
```

### 6.2 API Wrapper

```typescript
// src/api/user.ts
import axios from '@/api/interceptor';
import type { IPagedData } from './paged-data';

export interface UserInfo {
  id: string;
  username: string;
  email?: string;
  isActive: boolean;
}

export function getUserList(params: UserQueryParams) {
  return axios.get<IPagedData<UserInfo>>('/api/user', { params });
}

export function createUser(data: CreateUserRequest) {
  return axios.post<{ id: string }>('/api/user', data);
}
```

---

## 7. API Design Standards

### 7.1 RESTful Standards

| Operation | HTTP Method | URL | Description |
|-----------|-------------|-----|-------------|
| List | GET | `/api/users` | Get user list |
| Detail | GET | `/api/users/{id}` | Get single user |
| Create | POST | `/api/users` | Create user |
| Update | PUT | `/api/users/{id}` | Full update |
| Partial Update | PATCH | `/api/users/{id}` | Partial update |
| Delete | DELETE | `/api/users/{id}` | Delete user |

### 7.2 Response Status Codes

| Code | Description | Use Case |
|------|-------------|----------|
| 200 | OK | Success |
| 201 | Created | Creation success |
| 204 | No Content | Deletion success |
| 400 | Bad Request | Invalid parameters |
| 401 | Unauthorized | Not authenticated |
| 403 | Forbidden | No permission |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server error |

---

## 8. Database Standards

### 8.1 Table Naming

```sql
-- ✅ Correct: Use lowercase + underscore
CREATE TABLE users (...)
CREATE TABLE user_roles (...)
CREATE TABLE audit_logs (...)

-- ❌ Incorrect
CREATE TABLE Users (...)      -- No uppercase
CREATE TABLE UserRole (...)   -- No camelCase
CREATE TABLE tbl_user (...)   -- No prefix
```

### 8.2 GUID Primary Key & Timestamp Requirements

**Important**: When using GUID (UUID) as primary key, you **MUST** add a `timestamp` field (BIGINT type) with an index.

**Reasons**:
- GUIDs are unordered, using them directly as clustered index causes severe index fragmentation and performance issues
- `timestamp` field provides ordered incremental values for:
  - Optimistic concurrency control
  - Pagination sorting (avoid sorting by GUID)
  - Data synchronization and incremental updates

```sql
-- ✅ Correct: GUID primary key + timestamp field
CREATE TABLE users (
    id              CHAR(36) PRIMARY KEY,          -- UUID primary key
    timestamp       BIGINT NOT NULL,               -- Required: timestamp/version
    username        VARCHAR(50) NOT NULL UNIQUE,
    is_active       TINYINT(1) DEFAULT 1,          -- Boolean with is_ prefix
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME ON UPDATE CURRENT_TIMESTAMP,
    deleted_at      DATETIME,                      -- Soft delete
    tenant_id       CHAR(36),                      -- Tenant ID
    
    INDEX idx_timestamp (timestamp),               -- Required: timestamp index
    INDEX idx_username (username),
    INDEX idx_tenant_id (tenant_id)
);

-- ❌ Incorrect: Missing timestamp field
CREATE TABLE users (
    id              CHAR(36) PRIMARY KEY,
    username        VARCHAR(50) NOT NULL
    -- Missing timestamp field!
);
```

### 8.3 Field Naming Conventions

| Field Type | Convention | Example |
|------------|------------|---------|  
| Primary Key | `id` | `id CHAR(36)` |
| Timestamp | `timestamp` | `timestamp BIGINT` |
| Foreign Key | `{table}_id` | `user_id`, `tenant_id` |
| Boolean | `is_{name}` / `has_{name}` | `is_active`, `has_permission` |
| DateTime | `{action}_at` | `created_at`, `updated_at`, `deleted_at` |

### 8.4 Index Standards

- Primary key uses `id`
- **GUID primary key tables MUST have `idx_timestamp` index**
- Foreign keys use `{table}_id` format
- Regular indexes use `idx_{column}` format
- Unique indexes use `uk_{column}` format
- Composite indexes use `idx_{col1}_{col2}` format

### 8.5 Entity Class Standards

```csharp
/// <summary>
/// Base entity class
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Optimistic lock timestamp (required)</summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>Created time</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Updated time</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// User entity
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}
```

---

## 9. Testing Standards

### 9.1 Test Method Naming

```csharp
// Format: MethodName_StateUnderTest_ExpectedBehavior
[Fact]
public void Truncate_StringLongerThanMaxLength_ReturnsTruncatedWithSuffix()
{
    // Arrange
    var input = "Hello World";
    
    // Act
    var result = input.Truncate(8);
    
    // Assert
    Assert.Equal("Hello...", result);
}
```

### 9.2 Coverage Targets

- SDK packages: > 80%
- Core business logic: > 70%
- Utility classes: > 90%

---

## 10. Security Standards

### 10.1 Sensitive Information

```csharp
// ✅ Use environment variables or configuration
var connectionString = configuration.GetConnectionString("DefaultConnection");

// ❌ Never hardcode
var password = "123456";
var apiKey = "sk-xxxxx";
```

### 10.2 Input Validation

```csharp
public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$")]
    public string Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
```

---

## 11. Release Standards

### 11.1 Semantic Versioning

Follow [SemVer](https://semver.org/): `MAJOR.MINOR.PATCH`

- **MAJOR**: Incompatible API changes
- **MINOR**: Backward-compatible features
- **PATCH**: Backward-compatible fixes

### 11.2 SDK Release Process

```bash
# 1. Update version (Directory.Build.props)
# 2. Commit changes
git add .
git commit -m "chore(sdk): bump version to 1.2.0"
git push origin main

# 3. Create tag to trigger release
git tag sdk-v1.2.0
git push origin sdk-v1.2.0
```

---

## 12. SDK Integration Guidelines

> This section is for developers of external business systems who need to integrate with Dawning SDK.

### 12.1 SDK Package Overview

Dawning SDK is published to NuGet.org and can be used without authentication:

| Package | Description | Dependencies |
|---------|-------------|--------------|
| `Dawning.Core` | Core library (exceptions, middleware, unified results) | None |
| `Dawning.Extensions` | Extension methods (string, collection, JSON, datetime) | None |
| `Dawning.Identity` | Identity library (JWT parsing, user context) | Dawning.Core |
| `Dawning.Caching` | Cache service (memory cache, Redis) | Dawning.Core |
| `Dawning.Logging` | Logging service (structured logging, request tracing) | Dawning.Core |
| `Dawning.Messaging` | Message queue (RabbitMQ, Azure Service Bus) | Dawning.Core |
| `Dawning.ORM.Dapper` | ORM extensions (Dapper enhancements) | Dawning.Core |
| `Dawning.Resilience` | Resilience policies (retry, circuit breaker, timeout) | Dawning.Core |

### 12.2 Installation

#### Via NuGet CLI

```bash
# Install core package
dotnet add package Dawning.Core

# Install common package combination
dotnet add package Dawning.Core
dotnet add package Dawning.Extensions
dotnet add package Dawning.Identity
dotnet add package Dawning.Logging
dotnet add package Dawning.Caching
```

#### Via PackageReference

```xml
<ItemGroup>
  <PackageReference Include="Dawning.Core" Version="1.0.*" />
  <PackageReference Include="Dawning.Extensions" Version="1.0.*" />
  <PackageReference Include="Dawning.Identity" Version="1.0.*" />
  <PackageReference Include="Dawning.Logging" Version="1.0.*" />
  <PackageReference Include="Dawning.Caching" Version="1.0.*" />
</ItemGroup>
```

### 12.3 Prerequisites

- **.NET Version**: .NET 8.0 or higher
- **C# Version**: 12.0 or higher
- **Optional Dependencies**:
  - Redis (when using Redis features in `Dawning.Caching`)
  - RabbitMQ (when using `Dawning.Messaging`)
  - MySQL/PostgreSQL/SQL Server (when using `Dawning.ORM.Dapper`)

### 12.4 Quick Start

#### 12.4.1 Program.cs Configuration

```csharp
using Dawning.Core.Middleware;
using Dawning.Identity.Extensions;
using Dawning.Logging.Extensions;
using Dawning.Caching;

var builder = WebApplication.CreateBuilder(args);

// 1. Add logging service
builder.Services.AddDawningLogging(options =>
{
    options.EnableRequestLogging = true;
    options.EnableResponseLogging = true;
});

// 2. Add identity authentication
builder.Services.AddDawningIdentity(options =>
{
    options.JwtSecret = builder.Configuration["Jwt:Secret"]!;
    options.Issuer = builder.Configuration["Jwt:Issuer"]!;
    options.Audience = builder.Configuration["Jwt:Audience"]!;
});

// 3. Add caching service
builder.Services.AddDawningCaching(options =>
{
    options.UseRedis = true;
    options.RedisConnection = builder.Configuration.GetConnectionString("Redis")!;
});

var app = builder.Build();

// 4. Use global exception handler
app.UseGlobalExceptionHandler();

// 5. Use request logging middleware
app.UseRequestLogging();

// 6. Use authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

#### 12.4.2 appsettings.json Configuration

```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here",
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "ExpireMinutes": 60
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379",
    "Database": "Server=localhost;Database=mydb;User=root;Password=xxx;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 12.5 Common Usage Examples

#### 12.5.1 Unified API Results

```csharp
using Dawning.Core.Results;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ApiResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return ApiResult<UserDto>.NotFound("User not found");
        }
        return ApiResult<UserDto>.Success(user);
    }

    [HttpGet]
    public async Task<ApiResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetPagedAsync(page, pageSize);
        return ApiResult<PagedResult<UserDto>>.Success(result);
    }
}
```

#### 12.5.2 Current User Context

```csharp
using Dawning.Identity;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public ProfileController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            UserId = _currentUser.Id,
            Username = _currentUser.UserName,
            Roles = _currentUser.Roles
        });
    }
}
```

#### 12.5.3 Caching Usage

```csharp
using Dawning.Caching;

public class UserService
{
    private readonly ICacheService _cache;

    public UserService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"user:{id}";
        
        // Try to get from cache
        var cached = await _cache.GetAsync<UserDto>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // Get from database
        var user = await _repository.GetByIdAsync(id);
        if (user != null)
        {
            // Cache for 30 minutes
            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
        }

        return user;
    }
}
```

#### 12.5.4 Extension Methods Usage

```csharp
using Dawning.Extensions;

// String extensions
string name = "HelloWorld";
name.ToSnakeCase();    // "hello_world"
name.ToKebabCase();    // "hello-world"
name.ToCamelCase();    // "helloWorld"

// Collection extensions
var list = new List<int> { 1, 2, 3 };
list.IsNullOrEmpty();  // false

// JSON extensions
var obj = new { Name = "test" };
obj.ToJson();          // "{\"name\":\"test\"}"
```

### 12.6 Gateway Integration

If your business system needs to be accessed through Dawning Gateway:

#### 12.6.1 Configure Gateway Routes

Add business system routes in Gateway configuration:

```json
{
  "ReverseProxy": {
    "Routes": {
      "business-system": {
        "ClusterId": "business-cluster",
        "Match": {
          "Path": "/api/business/{**catch-all}"
        },
        "Transforms": [
          { "PathRemovePrefix": "/api/business" }
        ]
      }
    },
    "Clusters": {
      "business-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://business-service:8080/"
          }
        }
      }
    }
  }
}
```

#### 12.6.2 Handle User Information from Gateway

```csharp
// Gateway passes user information in request headers
// Business systems can use Dawning.Identity to parse automatically

[Authorize]
public class OrderController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    [HttpPost]
    public async Task<ApiResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        // _currentUser.Id comes from JWT passed by Gateway
        var order = new Order
        {
            UserId = _currentUser.Id,
            // ...
        };
        
        return ApiResult<Order>.Success(order);
    }
}
```

### 12.7 Version Compatibility

| SDK Version | .NET Version | Notes |
|-------------|--------------|-------|
| 1.0.x | .NET 8.0+ | Current stable |
| 1.1.x | .NET 8.0+ | Planned, new features |
| 2.0.x | .NET 9.0+ | Planned, may have breaking changes |

### 12.8 FAQ

#### Q: How to handle SDK package version conflicts?

Ensure all Dawning packages use the same major version:

```xml
<ItemGroup>
  <PackageReference Include="Dawning.Core" Version="1.0.1" />
  <PackageReference Include="Dawning.Identity" Version="1.0.1" />
  <!-- Keep all packages at the same version -->
</ItemGroup>
```

#### Q: How to customize exception handling?

```csharp
app.UseGlobalExceptionHandler(options =>
{
    options.IncludeStackTrace = app.Environment.IsDevelopment();
    options.OnException = (context, exception) =>
    {
        // Custom logging
        logger.LogError(exception, "Unhandled exception");
    };
});
```

#### Q: How to disable logging for certain middleware?

```csharp
builder.Services.AddDawningLogging(options =>
{
    options.ExcludePaths = new[] { "/health", "/metrics" };
    options.EnableResponseLogging = false; // Log requests only, not responses
});
```

---

## Appendix

### A. Recommended Tools

- **IDE**: Visual Studio 2022 / VS Code / Rider
- **Code Formatting**: EditorConfig, Prettier
- **API Testing**: Postman, Thunder Client
- **Database**: MySQL Workbench, DBeaver
- **Containers**: Docker Desktop

### B. References

- [.NET Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Vue Style Guide](https://vuejs.org/style-guide/)
- [RESTful API Design Guide](https://restfulapi.net/)
- [Conventional Commits](https://www.conventionalcommits.org/)
