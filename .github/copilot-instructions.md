# Dawning Project Copilot Instructions

## Project Overview

Dawning is a microservices gateway system built with .NET 8 and Vue 3. It provides:
- **Gateway**: API Gateway with authentication, rate limiting, request logging
- **Admin**: Frontend admin panel for gateway management
- **SDK**: Reusable NuGet packages for .NET applications

## Technology Stack

### Backend (.NET 8)
- ASP.NET Core 8.0 with Minimal APIs and Controllers
- Dapper for database access (NOT Entity Framework)
- MySQL 8.0+ database
- OpenIddict for OAuth 2.0 / OpenID Connect
- YARP for reverse proxy
- C# 12 with nullable enabled, file-scoped namespaces, primary constructors

### Frontend (Vue 3)
- Vue 3 with Composition API (`<script setup>`)
- TypeScript 5.x
- Arco Design Vue component library
- Pinia for state management
- Vue Router 4
- Vue I18n for internationalization

### SDK Packages (NuGet.org)
- `Dawning.Core` - Exceptions, middleware, unified results
- `Dawning.Extensions` - String, collection, JSON extensions
- `Dawning.Identity` - JWT parsing, user context
- `Dawning.Caching` - Memory cache, Redis
- `Dawning.Logging` - Structured logging, request tracing
- `Dawning.Messaging` - RabbitMQ, Azure Service Bus
- `Dawning.ORM.Dapper` - Dapper enhancements
- `Dawning.Resilience` - Retry, circuit breaker, timeout

## Code Style Requirements

### C# Code Style
```csharp
// ✅ Use file-scoped namespaces
namespace Dawning.Gateway.Controllers;

// ✅ Use primary constructors (C# 12)
public class UserService(IUserRepository repo, ILogger<UserService> logger) : IUserService
{
    // ✅ Use expression body for simple methods
    public async Task<UserDto?> GetByIdAsync(Guid id) =>
        await repo.GetByIdAsync(id);
}

// ✅ Private fields with underscore prefix
private readonly ILogger<UserController> _logger;

// ✅ XML documentation for public APIs
/// <summary>
/// Gets user by ID
/// </summary>
/// <param name="id">User ID</param>
/// <returns>User DTO or null</returns>
public async Task<UserDto?> GetByIdAsync(Guid id);
```

### TypeScript/Vue Code Style
```typescript
// ✅ Use Composition API with <script setup>
<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const loading = ref(false);
</script>

// ✅ Use typed props and emits
const props = defineProps<{ visible: boolean }>();
const emit = defineEmits<{ (e: 'success'): void }>();
```

## Database Requirements

### GUID Primary Key with Timestamp (MANDATORY)
When using GUID as primary key, you **MUST** add a `timestamp` field:

```sql
CREATE TABLE users (
    id              CHAR(36) PRIMARY KEY,
    timestamp       BIGINT NOT NULL,          -- REQUIRED!
    username        VARCHAR(50) NOT NULL,
    is_active       TINYINT(1) DEFAULT 1,
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_timestamp (timestamp)           -- REQUIRED!
);
```

### Entity Base Class
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

## API Response Format

Always use unified API response:
```csharp
public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Code { get; set; } = "OK";
    public string? Message { get; set; }
    public T? Data { get; set; }
    public long Timestamp { get; set; }
    public string? TraceId { get; set; }
}

// Usage
return ApiResult<UserDto>.Success(user);
return ApiResult<UserDto>.NotFound("User not found");
```

## Git Commit Standards

Follow Conventional Commits:
```
<type>(<scope>): <subject>

Types: feat, fix, docs, style, refactor, perf, test, chore, ci
Scopes: admin, gateway, identity, sdk, core, auth, user, deploy
```

Examples:
- `feat(admin): add user management page`
- `fix(gateway): fix rate limit bypass issue`
- `docs(sdk): update integration guide`

## Project Structure Patterns

### Backend Controller
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        return user == null 
            ? NotFound(ApiResult<UserDto>.NotFound("User not found"))
            : Ok(ApiResult<UserDto>.Success(user));
    }
}
```

### Frontend API Wrapper
```typescript
// src/api/user.ts
import axios from '@/api/interceptor';
import type { IPagedData } from './paged-data';

export interface UserInfo {
  id: string;
  username: string;
  isActive: boolean;
}

export function getUserList(params: UserQueryParams) {
  return axios.get<IPagedData<UserInfo>>('/api/user', { params });
}
```

### Frontend Page Component
```vue
<template>
  <div class="page-container">
    <a-table :data="users" :loading="loading" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { getUserList } from '@/api/user';

const loading = ref(false);
const users = ref<UserInfo[]>([]);

const fetchData = async () => {
  loading.value = true;
  try {
    const { data } = await getUserList({});
    users.value = data.items;
  } finally {
    loading.value = false;
  }
};

onMounted(fetchData);
</script>
```

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| C# Classes | PascalCase | `UserService` |
| C# Interfaces | IPascalCase | `IUserRepository` |
| C# Private Fields | _camelCase | `_logger` |
| TypeScript Variables | camelCase | `userName` |
| Constants | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT` |
| Database Tables | snake_case | `user_roles` |
| API URLs | kebab-case | `/api/request-logs` |

## Important Notes

1. **Never hardcode secrets** - Use environment variables or configuration
2. **Always validate input** - Use data annotations or FluentValidation
3. **Use async/await** - All I/O operations should be async
4. **Handle errors gracefully** - Use global exception middleware
5. **Write tests** - SDK packages should have >80% coverage
6. **Document public APIs** - Use XML comments for all public members
7. **Use dependency injection** - Constructor injection preferred
8. **Follow RESTful conventions** - Proper HTTP methods and status codes

## Reference Documentation

- [DEVELOPMENT_STANDARDS.md](docs/DEVELOPMENT_STANDARDS.md) - Full development standards
- [DEVELOPER_GUIDE.md](docs/DEVELOPER_GUIDE.md) - Developer guide
- [AUTHENTICATION_INTEGRATION.md](docs/AUTHENTICATION_INTEGRATION.md) - Auth integration
