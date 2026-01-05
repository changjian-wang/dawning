# Dawning 开发规范

本文档定义了 Dawning 项目的开发规范和最佳实践，确保代码质量、一致性和可维护性。

## 目录

- [1. 项目结构](#1-项目结构)
- [2. 命名规范](#2-命名规范)
- [3. 代码风格](#3-代码风格)
- [4. Git 提交规范](#4-git-提交规范)
- [5. 后端开发规范 (C#/.NET)](#5-后端开发规范-cnet)
- [6. 前端开发规范 (Vue/TypeScript)](#6-前端开发规范-vuetypescript)
- [7. API 设计规范](#7-api-设计规范)
- [8. 数据库规范](#8-数据库规范)
- [9. 测试规范](#9-测试规范)
- [10. 文档规范](#10-文档规范)
- [11. 安全规范](#11-安全规范)
- [12. 版本发布规范](#12-版本发布规范)
- [13. SDK 集成规范](#13-sdk-集成规范)

---

## 1. 项目结构

### 1.1 总体结构

```
dawning/
├── apps/                   # 应用程序
│   ├── admin/              # 前端管理后台 (Vue 3 + TypeScript)
│   └── gateway/            # 后端服务 (ASP.NET Core)
├── sdk/                    # SDK 包 (NuGet)
├── deploy/                 # 部署配置
│   ├── docker/             # Docker Compose
│   ├── k8s/                # Kubernetes
│   └── argocd/             # ArgoCD
├── docs/                   # 文档
└── .github/workflows/      # CI/CD
```

### 1.2 后端分层架构 (Clean Architecture)

```
Dawning.Identity.Api/                    # 表现层 (Controllers)
Dawning.Identity.Application/            # 应用层 (Services, DTOs)
├── Dtos/                                # 数据传输对象
├── Interfaces/                          # 服务接口
├── Services/                            # 服务实现
└── Mapping/                             # AutoMapper 配置
Dawning.Identity.Domain/                 # 领域层 (Entities, Aggregates)
├── Aggregates/                          # 聚合根
├── Events/                              # 领域事件
├── Interfaces/                          # 仓储接口
└── Models/                              # 领域模型
Dawning.Identity.Infra.Data/             # 基础设施层 (Repositories)
Dawning.Identity.Infra.CrossCutting.IoC/ # 依赖注入配置
```

### 1.3 前端目录结构

```
src/
├── api/                    # API 接口封装
├── assets/                 # 静态资源
├── components/             # 公共组件
├── hooks/                  # 组合式函数
├── locale/                 # 国际化
├── router/                 # 路由配置
├── store/                  # 状态管理 (Pinia)
├── utils/                  # 工具函数
└── views/                  # 页面组件
    └── administration/     # 按模块组织
        ├── user/
        ├── role/
        └── gateway/
```

---

## 2. 命名规范

### 2.1 通用规则

| 类型 | 规范 | 示例 |
|------|------|------|
| 文件夹 | kebab-case | `user-permission`, `rate-limit` |
| 变量/函数 | camelCase | `getUserInfo`, `isActive` |
| 常量 | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `API_BASE_URL` |
| 类/接口 | PascalCase | `UserService`, `IUserRepository` |

### 2.2 C# 命名规范

```csharp
// ✅ 正确
public class UserService { }
public interface IUserRepository { }
private readonly ILogger<UserController> _logger;
public string UserName { get; set; }
private const int MaxRetryCount = 3;

// ❌ 错误
public class userService { }        // 类名应为 PascalCase
public interface UserRepository { } // 接口应以 I 开头
private ILogger<UserController> logger; // 私有字段应以 _ 开头
```

**文件命名**：
- 类文件：`UserService.cs`
- 接口文件：`IUserRepository.cs`
- DTO 文件：`UserDto.cs`, `CreateUserDto.cs`
- 控制器：`UserController.cs`

### 2.3 TypeScript/Vue 命名规范

```typescript
// 变量和函数：camelCase
const userName = 'admin';
function getUserInfo() { }

// 常量：UPPER_SNAKE_CASE
const MAX_RETRY_COUNT = 3;

// 类型和接口：PascalCase
interface UserInfo { }
type HttpResponse<T> = { }

// Vue 组件文件：PascalCase 或 kebab-case
// 推荐：index.vue (在独立文件夹中)
// views/administration/user/index.vue
```

---

## 3. 代码风格

### 3.1 C# 代码风格

**基础配置** (Directory.Build.props)：
```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

**代码格式**：
```csharp
// ✅ 使用文件级命名空间
namespace Dawning.Identity.Application.Services;

// ✅ 使用主构造函数 (C# 12)
public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger
) : IUserService
{
    // ✅ 使用 expression body
    public async Task<UserDto?> GetByIdAsync(Guid id) =>
        await userRepository.GetByIdAsync(id);
}

// ✅ XML 文档注释
/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户DTO，如不存在返回null</returns>
    Task<UserDto?> GetByIdAsync(Guid id);
}
```

### 3.2 TypeScript 代码风格

**ESLint 配置**：
- 继承 `airbnb-base`
- 使用 `@typescript-eslint` 插件
- 集成 Prettier

**代码示例**：
```typescript
// ✅ 使用 Composition API
<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { UserInfo } from '@/api/user';

const { t } = useI18n();

// 响应式状态
const loading = ref(false);
const users = ref<UserInfo[]>([]);

// 计算属性
const activeUsers = computed(() => 
  users.value.filter(u => u.isActive)
);

// 生命周期
onMounted(() => {
  fetchUsers();
});
</script>
```

---

## 4. Git 提交规范

### 4.1 Commit Message 格式

使用 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```
<type>(<scope>): <subject>

<body>

<footer>
```

### 4.2 Type 类型

| Type | 说明 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(user): 添加用户导入功能` |
| `fix` | Bug 修复 | `fix(auth): 修复 token 刷新失败问题` |
| `docs` | 文档更新 | `docs: 更新 API 文档` |
| `style` | 代码格式 | `style: 格式化代码` |
| `refactor` | 重构 | `refactor(core): 优化缓存策略` |
| `perf` | 性能优化 | `perf(db): 优化查询性能` |
| `test` | 测试 | `test(user): 添加用户服务单元测试` |
| `chore` | 构建/工具 | `chore: 升级依赖版本` |
| `ci` | CI/CD | `ci: 添加自动部署流程` |

### 4.3 Scope 范围

- `admin` - 前端管理后台
- `gateway` - 网关服务
- `identity` - 身份认证服务
- `sdk` - SDK 包
- `core` - 核心模块
- `auth` - 认证授权
- `user` - 用户管理
- `deploy` - 部署配置

### 4.4 示例

```bash
# ✅ 正确
feat(admin): 添加网关请求监控页面
fix(identity): 修复 OpenIddict 角色声明映射问题
docs(sdk): 更新 Dawning.Core 使用文档
chore(deps): 升级 .NET 8.0.2

# ❌ 错误
update code                    # 缺少类型和具体描述
feat: 新功能                   # 描述不具体
Fix bug                        # 大写，无 scope
```

---

## 5. 后端开发规范 (C#/.NET)

### 5.1 Controller 规范

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
    /// 获取用户列表
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

### 5.2 Service 规范

```csharp
public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<PagedResult<UserDto>> GetPagedAsync(UserQueryDto query);
    Task<Guid> CreateAsync(CreateUserDto dto);
    Task UpdateAsync(Guid id, UpdateUserDto dto);
    Task DeleteAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    // 使用构造函数注入
    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}
```

### 5.3 DTO 规范

```csharp
/// <summary>
/// 用户DTO（返回给客户端）
/// </summary>
public class UserDto
{
    /// <summary>用户唯一标识</summary>
    public Guid Id { get; set; }

    /// <summary>用户名（登录名）</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>邮箱</summary>
    public string? Email { get; set; }

    /// <summary>是否激活</summary>
    public bool IsActive { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建用户请求DTO
/// </summary>
public class CreateUserDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50之间")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
    public string Password { get; set; } = string.Empty;
}
```

### 5.4 统一响应格式

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

### 5.5 异常处理

```csharp
// 自定义业务异常
public class BusinessException : Exception
{
    public string Code { get; }
    
    public BusinessException(string code, string message) : base(message)
    {
        Code = code;
    }
}

// 全局异常处理中间件
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

---

## 6. 前端开发规范 (Vue/TypeScript)

### 6.1 组件结构

```vue
<template>
  <div class="user-list">
    <!-- 模板内容 -->
  </div>
</template>

<script lang="ts" setup>
// 1. 导入
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Message } from '@arco-design/web-vue';
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

// 3. 组合式函数
const { t } = useI18n();

// 4. 响应式状态
const loading = ref(false);
const users = ref<UserInfo[]>([]);

// 5. 计算属性
const activeUsers = computed(() => 
  users.value.filter(u => u.isActive)
);

// 6. 方法
const fetchUsers = async () => {
  loading.value = true;
  try {
    const result = await getUserList();
    users.value = result.items;
  } catch (error) {
    Message.error(t('common.loadFailed'));
  } finally {
    loading.value = false;
  }
};

// 7. 生命周期
onMounted(() => {
  fetchUsers();
});
</script>

<style lang="less" scoped>
.user-list {
  // 样式
}
</style>
```

### 6.2 API 封装

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

export interface CreateUserRequest {
  username: string;
  password: string;
  email?: string;
}

export interface UserQueryParams {
  keyword?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

// 获取用户列表
export function getUserList(params: UserQueryParams) {
  return axios.get<IPagedData<UserInfo>>('/api/user', { params });
}

// 获取用户详情
export function getUserById(id: string) {
  return axios.get<UserInfo>(`/api/user/${id}`);
}

// 创建用户
export function createUser(data: CreateUserRequest) {
  return axios.post<{ id: string }>('/api/user', data);
}

// 更新用户
export function updateUser(id: string, data: Partial<CreateUserRequest>) {
  return axios.put(`/api/user/${id}`, data);
}

// 删除用户
export function deleteUser(id: string) {
  return axios.delete(`/api/user/${id}`);
}
```

### 6.3 国际化

```typescript
// src/views/administration/user/locale/zh-CN.ts
export default {
  'menu.administration.user': '用户管理',
  'user.username': '用户名',
  'user.email': '邮箱',
  'user.isActive': '状态',
  'user.createSuccess': '用户创建成功',
  'user.deleteConfirm': '确定要删除该用户吗？',
};

// src/views/administration/user/locale/en-US.ts
export default {
  'menu.administration.user': 'User Management',
  'user.username': 'Username',
  'user.email': 'Email',
  'user.isActive': 'Status',
  'user.createSuccess': 'User created successfully',
  'user.deleteConfirm': 'Are you sure you want to delete this user?',
};
```

---

## 7. API 设计规范

### 7.1 RESTful 规范

| 操作 | HTTP Method | URL | 说明 |
|------|-------------|-----|------|
| 列表 | GET | `/api/users` | 获取用户列表 |
| 详情 | GET | `/api/users/{id}` | 获取单个用户 |
| 创建 | POST | `/api/users` | 创建用户 |
| 更新 | PUT | `/api/users/{id}` | 全量更新 |
| 部分更新 | PATCH | `/api/users/{id}` | 部分更新 |
| 删除 | DELETE | `/api/users/{id}` | 删除用户 |

### 7.2 URL 命名规范

```
✅ 正确
GET  /api/users
GET  /api/users/{id}
GET  /api/users/{id}/roles
POST /api/users/{id}/reset-password
GET  /api/request-logs

❌ 错误
GET  /api/getUsers              # 不要用动词
GET  /api/user/{id}             # 资源名应为复数
GET  /api/users/{id}/getRoles   # 不要用 get 前缀
POST /api/users/resetPassword   # 用 kebab-case
```

### 7.3 查询参数

```
# 分页
GET /api/users?page=1&pageSize=20

# 排序
GET /api/users?sortBy=createdAt&sortOrder=desc

# 过滤
GET /api/users?isActive=true&role=admin

# 搜索
GET /api/users?keyword=john

# 时间范围
GET /api/request-logs?startTime=2024-01-01&endTime=2024-01-31
```

### 7.4 响应状态码

| 状态码 | 说明 | 使用场景 |
|--------|------|----------|
| 200 | OK | 成功 |
| 201 | Created | 创建成功 |
| 204 | No Content | 删除成功 |
| 400 | Bad Request | 请求参数错误 |
| 401 | Unauthorized | 未认证 |
| 403 | Forbidden | 无权限 |
| 404 | Not Found | 资源不存在 |
| 409 | Conflict | 资源冲突 |
| 422 | Unprocessable Entity | 验证失败 |
| 500 | Internal Server Error | 服务器错误 |

---

## 8. 数据库规范

### 8.1 表命名

```sql
-- ✅ 正确：使用小写 + 下划线
CREATE TABLE users (...)
CREATE TABLE user_roles (...)
CREATE TABLE audit_logs (...)
CREATE TABLE openiddict_applications (...)

-- ❌ 错误
CREATE TABLE Users (...)      -- 不要用大写
CREATE TABLE UserRole (...)   -- 不要用驼峰
CREATE TABLE tbl_user (...)   -- 不要用前缀
```

### 8.2 GUID 主键与 Timestamp 规范

**重要**：当使用 GUID (UUID) 作为主键时，**必须**添加 `timestamp` 字段（BIGINT 类型）并创建索引。

**原因**：
- GUID 是无序的，直接作为聚集索引会导致严重的索引碎片和性能问题
- `timestamp` 字段提供有序的递增值，用于：
  - 乐观并发控制
  - 分页查询排序（避免使用 GUID 排序）
  - 数据同步和增量更新

```sql
-- ✅ 正确：GUID 主键 + timestamp 字段
CREATE TABLE users (
    id              CHAR(36) PRIMARY KEY,          -- UUID 主键
    timestamp       BIGINT NOT NULL,               -- 必须：时间戳/版本号
    username        VARCHAR(50) NOT NULL UNIQUE,
    email           VARCHAR(255),
    password_hash   VARCHAR(255) NOT NULL,
    is_active       TINYINT(1) DEFAULT 1,          -- 布尔用 is_ 前缀
    role            VARCHAR(50) DEFAULT 'user',
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME ON UPDATE CURRENT_TIMESTAMP,
    deleted_at      DATETIME,                      -- 软删除
    tenant_id       CHAR(36),                      -- 租户ID
    
    INDEX idx_timestamp (timestamp),               -- 必须：timestamp 索引
    INDEX idx_username (username),
    INDEX idx_email (email),
    INDEX idx_tenant_id (tenant_id)
);

-- ❌ 错误：缺少 timestamp 字段
CREATE TABLE users (
    id              CHAR(36) PRIMARY KEY,
    username        VARCHAR(50) NOT NULL
    -- 缺少 timestamp 字段！
);
```

### 8.3 字段命名规范

| 字段类型 | 命名规范 | 示例 |
|----------|----------|------|
| 主键 | `id` | `id CHAR(36)` |
| 时间戳 | `timestamp` | `timestamp BIGINT` |
| 外键 | `{table}_id` | `user_id`, `tenant_id` |
| 布尔 | `is_{name}` / `has_{name}` | `is_active`, `has_permission` |
| 时间 | `{action}_at` | `created_at`, `updated_at`, `deleted_at` |

### 8.4 索引规范

- 主键使用 `id`
- **GUID 主键表必须有 `idx_timestamp` 索引**
- 外键使用 `{table}_id` 格式
- 普通索引使用 `idx_{column}` 格式
- 唯一索引使用 `uk_{column}` 格式
- 复合索引使用 `idx_{col1}_{col2}` 格式

### 8.5 实体类规范

```csharp
/// <summary>
/// 实体基类
/// </summary>
public abstract class BaseEntity
{
    /// <summary>唯一标识</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>乐观锁时间戳（必须）</summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 用户实体
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}
```

---

## 9. 测试规范

### 9.1 测试文件命名

```
// 单元测试
Dawning.Core.Tests/
├── StringExtensionsTests.cs
├── JsonExtensionsTests.cs
└── CacheServiceTests.cs

// 集成测试
Dawning.Identity.Api.IntegrationTests/
├── UserControllerTests.cs
└── AuthControllerTests.cs
```

### 9.2 测试方法命名

```csharp
// 格式: MethodName_StateUnderTest_ExpectedBehavior
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

[Theory]
[InlineData(null, true)]
[InlineData("", true)]
[InlineData("  ", true)]
[InlineData("hello", false)]
public void IsNullOrWhiteSpace_VariousInputs_ReturnsExpected(string? input, bool expected)
{
    Assert.Equal(expected, input.IsNullOrWhiteSpace());
}
```

### 9.3 测试覆盖率

- SDK 包：目标覆盖率 > 80%
- 核心业务逻辑：目标覆盖率 > 70%
- 工具类/扩展方法：目标覆盖率 > 90%

---

## 10. 文档规范

### 10.1 代码注释

```csharp
/// <summary>
/// 用户服务，处理用户相关的业务逻辑
/// </summary>
public class UserService : IUserService
{
    /// <summary>
    /// 根据用户名获取用户信息
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>用户信息，如不存在返回 null</returns>
    /// <exception cref="ArgumentNullException">当 username 为空时抛出</exception>
    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        // 参数验证
        ArgumentNullException.ThrowIfNull(username);
        
        // 查询用户
        var user = await _repository.GetByUsernameAsync(username);
        
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}
```

### 10.2 README 结构

每个 SDK 包/模块应包含 README.md：

```markdown
# Dawning.Core

核心基础库，提供通用工具类和扩展方法。

## 安装

​```bash
dotnet add package Dawning.Core
​```

## 功能特性

- 统一 API 响应格式
- 全局异常处理中间件
- 扩展方法

## 快速开始

​```csharp
// 示例代码
​```

## API 文档

详见 [API 文档](./docs/API.md)
```

---

## 11. 安全规范

### 11.1 敏感信息

```csharp
// ✅ 使用环境变量或配置
var connectionString = configuration.GetConnectionString("DefaultConnection");
var apiKey = configuration["ApiKey"];

// ❌ 不要硬编码
var password = "123456";
var apiKey = "sk-xxxxx";
```

### 11.2 输入验证

```csharp
// 使用数据注解
public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "用户名只能包含字母、数字和下划线")]
    public string Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}

// 使用 FluentValidation
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Matches(@"^[a-zA-Z0-9_]+$");
    }
}
```

### 11.3 认证授权

```csharp
// 使用 Authorize 特性
[Authorize]
[ApiController]
public class UserController : ControllerBase
{
    // 需要特定角色
    [Authorize(Roles = "admin,super_admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) { }
    
    // 需要特定策略
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto) { }
}
```

---

## 12. 版本发布规范

### 12.1 语义化版本

遵循 [SemVer](https://semver.org/) 规范：`MAJOR.MINOR.PATCH`

- **MAJOR**: 不兼容的 API 变更
- **MINOR**: 向后兼容的功能新增
- **PATCH**: 向后兼容的问题修复

### 12.2 SDK 发布流程

```bash
# 1. 更新版本号 (Directory.Build.props)
# 2. 提交代码
git add .
git commit -m "chore(sdk): bump version to 1.2.0"
git push origin main

# 3. 创建 Tag 触发发布
git tag sdk-v1.2.0
git push origin sdk-v1.2.0
```

### 12.3 变更日志

维护 `CHANGELOG.md`：

```markdown
# Changelog

## [1.2.0] - 2026-01-04

### Added
- 新增 `Dawning.Messaging` 消息队列支持
- 新增 `StringExtensions.ToKebabCase()` 方法

### Changed
- 优化 `CacheService` 性能

### Fixed
- 修复 `JsonExtensions.Deserialize` 空值处理

### Deprecated
- `OldMethod()` 将在 2.0 版本移除，请使用 `NewMethod()`
```

---

## 13. SDK 集成规范

> 本节面向需要集成 Dawning SDK 的外部业务系统开发者。

### 13.1 SDK 包概览

Dawning SDK 已发布至 NuGet.org，无需认证即可使用：

| 包名 | 说明 | 依赖项 |
|------|------|--------|
| `Dawning.Core` | 核心基础库（异常、中间件、统一返回结果） | 无 |
| `Dawning.Extensions` | 扩展方法库（字符串、集合、JSON、日期时间） | 无 |
| `Dawning.Identity` | 身份认证库（JWT 解析、用户上下文） | Dawning.Core |
| `Dawning.Caching` | 缓存服务（内存缓存、Redis） | Dawning.Core |
| `Dawning.Logging` | 日志服务（结构化日志、请求追踪） | Dawning.Core |
| `Dawning.Messaging` | 消息队列（RabbitMQ、Azure Service Bus） | Dawning.Core |
| `Dawning.ORM.Dapper` | ORM 扩展（Dapper 增强） | Dawning.Core |
| `Dawning.Resilience` | 弹性策略（重试、熔断、超时） | Dawning.Core |

### 13.2 安装方式

#### 通过 NuGet CLI

```bash
# 安装核心包
dotnet add package Dawning.Core

# 安装常用包组合
dotnet add package Dawning.Core
dotnet add package Dawning.Extensions
dotnet add package Dawning.Identity
dotnet add package Dawning.Logging
dotnet add package Dawning.Caching
```

#### 通过 PackageReference

```xml
<ItemGroup>
  <PackageReference Include="Dawning.Core" Version="1.0.*" />
  <PackageReference Include="Dawning.Extensions" Version="1.0.*" />
  <PackageReference Include="Dawning.Identity" Version="1.0.*" />
  <PackageReference Include="Dawning.Logging" Version="1.0.*" />
  <PackageReference Include="Dawning.Caching" Version="1.0.*" />
</ItemGroup>
```

### 13.3 环境要求

- **.NET 版本**: .NET 8.0 或更高版本
- **C# 版本**: 12.0 或更高版本
- **可选依赖**:
  - Redis（使用 `Dawning.Caching` 的 Redis 功能时）
  - RabbitMQ（使用 `Dawning.Messaging` 时）
  - MySQL/PostgreSQL/SQL Server（使用 `Dawning.ORM.Dapper` 时）

### 13.4 快速集成

#### 13.4.1 Program.cs 配置

```csharp
using Dawning.Core.Middleware;
using Dawning.Identity.Extensions;
using Dawning.Logging.Extensions;
using Dawning.Caching;

var builder = WebApplication.CreateBuilder(args);

// 1. 添加日志服务
builder.Services.AddDawningLogging(options =>
{
    options.EnableRequestLogging = true;
    options.EnableResponseLogging = true;
});

// 2. 添加身份认证
builder.Services.AddDawningIdentity(options =>
{
    options.JwtSecret = builder.Configuration["Jwt:Secret"]!;
    options.Issuer = builder.Configuration["Jwt:Issuer"]!;
    options.Audience = builder.Configuration["Jwt:Audience"]!;
});

// 3. 添加缓存服务
builder.Services.AddDawningCaching(options =>
{
    options.UseRedis = true;
    options.RedisConnection = builder.Configuration.GetConnectionString("Redis")!;
});

var app = builder.Build();

// 4. 使用全局异常处理
app.UseGlobalExceptionHandler();

// 5. 使用请求日志中间件
app.UseRequestLogging();

// 6. 使用身份认证
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

#### 13.4.2 appsettings.json 配置

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

### 13.5 常用功能示例

#### 13.5.1 统一返回结果

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
            return ApiResult<UserDto>.NotFound("用户不存在");
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

#### 13.5.2 当前用户上下文

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

#### 13.5.3 缓存使用

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
        
        // 尝试从缓存获取
        var cached = await _cache.GetAsync<UserDto>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // 从数据库获取
        var user = await _repository.GetByIdAsync(id);
        if (user != null)
        {
            // 缓存 30 分钟
            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
        }

        return user;
    }
}
```

#### 13.5.4 扩展方法使用

```csharp
using Dawning.Extensions;

// 字符串扩展
string name = "HelloWorld";
name.ToSnakeCase();    // "hello_world"
name.ToKebabCase();    // "hello-world"
name.ToCamelCase();    // "helloWorld"

// 集合扩展
var list = new List<int> { 1, 2, 3 };
list.IsNullOrEmpty();  // false

// JSON 扩展
var obj = new { Name = "test" };
obj.ToJson();          // "{\"name\":\"test\"}"
```

### 13.6 与 Gateway 集成

如果业务系统需要通过 Dawning Gateway 访问：

#### 13.6.1 配置网关路由

在 Gateway 配置中添加业务系统路由：

```yaml
# gateway appsettings.json
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

#### 13.6.2 处理 Gateway 传递的用户信息

```csharp
// Gateway 会在请求头中传递用户信息
// 业务系统通过 Dawning.Identity 自动解析

[Authorize]
public class OrderController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    [HttpPost]
    public async Task<ApiResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        // _currentUser.Id 来自 Gateway 传递的 JWT
        var order = new Order
        {
            UserId = _currentUser.Id,
            // ...
        };
        
        return ApiResult<Order>.Success(order);
    }
}
```

### 13.7 版本兼容性

| SDK 版本 | .NET 版本 | 说明 |
|----------|-----------|------|
| 1.0.x | .NET 8.0+ | 当前稳定版 |
| 1.1.x | .NET 8.0+ | 计划中，新增功能 |
| 2.0.x | .NET 9.0+ | 计划中，可能有破坏性变更 |

### 13.8 常见问题

#### Q: 如何处理 SDK 包的版本冲突？

确保所有 Dawning 包使用相同的主版本号：

```xml
<ItemGroup>
  <PackageReference Include="Dawning.Core" Version="1.0.1" />
  <PackageReference Include="Dawning.Identity" Version="1.0.1" />
  <!-- 所有包保持同一版本 -->
</ItemGroup>
```

#### Q: 如何自定义异常处理？

```csharp
app.UseGlobalExceptionHandler(options =>
{
    options.IncludeStackTrace = app.Environment.IsDevelopment();
    options.OnException = (context, exception) =>
    {
        // 自定义日志记录
        logger.LogError(exception, "Unhandled exception");
    };
});
```

#### Q: 如何禁用某些中间件的日志？

```csharp
builder.Services.AddDawningLogging(options =>
{
    options.ExcludePaths = new[] { "/health", "/metrics" };
    options.EnableResponseLogging = false; // 只记录请求，不记录响应
});
```

---

## 附录

### A. 推荐工具

- **IDE**: Visual Studio 2022 / VS Code / Rider
- **代码格式化**: EditorConfig, Prettier
- **API 测试**: Postman, Thunder Client
- **数据库**: MySQL Workbench, DBeaver
- **容器**: Docker Desktop

### B. 参考资料

- [.NET 编码约定](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Vue 风格指南](https://vuejs.org/style-guide/)
- [RESTful API 设计指南](https://restfulapi.net/)
- [Conventional Commits](https://www.conventionalcommits.org/)
