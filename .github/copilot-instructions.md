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
public class UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger) : IUserService
{
    // ✅ Access repositories through UnitOfWork
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await unitOfWork.User.GetByIdAsync(id);
        return user?.ToDto();  // Use static Mapper extension
    }
}

// ✅ Private fields with underscore prefix (use _unitOfWork, not _uow)
private readonly IUnitOfWork _unitOfWork;
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

## Service Layer Patterns

### UnitOfWork Pattern (Required)
```csharp
// ✅ Correct - Access repositories through UnitOfWork
public class RoleService(IUnitOfWork unitOfWork) : IRoleService
{
    public async Task<RoleDto?> GetByIdAsync(Guid id)
    {
        var role = await unitOfWork.Role.GetByIdAsync(id);
        return role?.ToDto();
    }
}

// ❌ Wrong - Don't inject both Repository and UnitOfWork
public class RoleService(IRoleRepository repo, IUnitOfWork unitOfWork)  // Redundant!
```

### Static Mapper Pattern (Required)
```csharp
// ✅ Correct - Use static Mapper extension methods
var dto = entity.ToDto();        // Entity -> DTO
var entity = dto.ToEntity();     // CreateDto -> Entity
entity.ApplyUpdate(updateDto);   // Apply UpdateDto to Entity

// ❌ Wrong - Don't inject IMapper
public class UserService(IMapper mapper)  // Don't do this!
{
    return _mapper.Map<UserDto>(user);    // Don't do this!
}
```

## Skill 索引

| Skill | 覆盖范围 | 触发关键词 |
|-------|---------|-----------|
| build-project | 构建后端 (.NET 8) 和前端 (Vue 3) | 构建, build, compile, dotnet build, pnpm build |
| git-workflow | 提交规范、pre-commit 检查 | git, commit, push, branch, tag |
| markdown | Markdown/XML 文档规范 | markdown, 写文档, README, API docs |
| changelog | CHANGELOG 格式、release notes | changelog, 变更日志, release notes |
| code-review | 代码审查（两轴：Standards 是否守规范 × Spec 是否忠实实现原始 issue/PRD） | 审查, review, check code, code review |
| code-patterns | 编码模式：静态 AutoMapper、UnitOfWork 服务、异常处理、常量定义 | mapper, AutoMapper, 异常处理, 常量, constants, refactor |
| vertical-slice | 垂直切片纪律：端到端薄切片、HITL/AFK 标注、`create-database → create-api → create-tests → create-vue-page` 串行 | 切片, slice, 端到端, 切多大, HITL, AFK, 跨层 |
| create-api | API 端点脚手架：DTO → Mapper → Service → Controller | 创建 API, create api, 新增接口, endpoint |
| create-vue-page | Vue 3 页面脚手架：Arco Design、i18n、路由 | 创建页面, create page, vue page, 前端页面 |
| create-database | MySQL 表结构 + Entity + Repository 脚手架 | 创建表, create table, 数据库, database, entity |
| create-tests | xUnit 单元测试模板（UnitOfWork Mock 模式） | 创建测试, create test, unit test, 单元测试 |
| create-domain-event | 领域事件 + 处理器脚手架 | 领域事件, domain event, 事件处理 |
| create-sdk-feature | SDK 包功能脚手架：扩展方法、服务、中间件 | SDK, create feature, 新增功能, NuGet |
| deployment | Docker 配置：.NET 8 多阶段构建、Vue 3 Nginx、docker-compose | Docker, 部署, deploy, Dockerfile, compose |
| troubleshooting | 调试六段法：repro → 假设 → 探针 → 修复 → 回归测试 → 清理；反馈回路 < 30s | 调试, debug, 排错, troubleshoot, error, regression |
| performance | 性能分析：N+1 查询、内存分配、异步模式、缓存 | 性能, performance, 优化, optimize, N+1 |
| architecture | 项目架构：DDD 分层、Mermaid 图表、模块边界 | 架构, architecture, 分层, diagram |

## Important Notes

1. **Never hardcode secrets** - Use environment variables or configuration
2. **Always validate input** - Use data annotations or FluentValidation
3. **Use async/await** - All I/O operations should be async
4. **Handle errors gracefully** - Use global exception middleware
5. **Write tests** - SDK packages should have >80% coverage
6. **Document public APIs** - Use XML comments for all public members
7. **Use dependency injection** - Constructor injection preferred
8. **Follow RESTful conventions** - Proper HTTP methods and status codes
9. **Use UnitOfWork** - Access repositories via `_unitOfWork.{Repository}`
10. **Use static Mappers** - Use `entity.ToDto()` not `_mapper.Map<>()`
