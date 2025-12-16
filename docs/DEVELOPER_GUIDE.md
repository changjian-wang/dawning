# Dawning Gateway 开发者文档

**版本**: 1.0.0  
**更新日期**: 2025-12-16

---

## 目录

1. [项目概述](#1-项目概述)
2. [技术架构](#2-技术架构)
3. [开发环境搭建](#3-开发环境搭建)
4. [项目结构](#4-项目结构)
5. [后端开发指南](#5-后端开发指南)
6. [前端开发指南](#6-前端开发指南)
7. [数据库设计](#7-数据库设计)
8. [API 规范](#8-api-规范)
9. [测试指南](#9-测试指南)
10. [部署指南](#10-部署指南)

---

## 1. 项目概述

### 1.1 项目简介

Dawning Gateway 是一个企业级 API 网关管理系统，提供身份认证、授权管理、API 路由、流量控制等功能。

### 1.2 核心功能

- **身份认证**: 基于 OpenIddict 的 OAuth 2.0 / OIDC
- **用户管理**: 用户 CRUD、角色权限管理
- **API 网关**: 基于 YARP 的反向代理
- **系统监控**: 请求日志、性能监控、告警

### 1.3 技术选型

| 层次 | 技术 | 版本 |
|------|------|------|
| 后端框架 | .NET | 8.0 |
| 认证框架 | OpenIddict | 5.8.0 |
| 网关 | YARP | 2.1.0 |
| ORM | Dapper | 2.1.66 |
| 数据库 | MySQL | 8.0+ |
| 缓存 | Redis | 6.0+ |
| 前端框架 | Vue | 3.x |
| UI 库 | Arco Design | 2.x |
| 构建工具 | Vite | 5.x |

---

## 2. 技术架构

### 2.1 整体架构

```
┌─────────────────────────────────────────────────────────┐
│                    Load Balancer                        │
└─────────────────────────────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│ Gateway API   │  │ Identity API  │  │ Admin Frontend│
│ (YARP Proxy)  │  │ (Auth Server) │  │ (Vue 3 SPA)   │
└───────────────┘  └───────────────┘  └───────────────┘
        │                  │
        └────────┬─────────┘
                 ▼
        ┌───────────────┐
        │    MySQL      │
        │   Database    │
        └───────────────┘
                 │
        ┌───────────────┐
        │    Redis      │
        │   Cache       │
        └───────────────┘
```

### 2.2 后端分层架构 (DDD)

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│   (Controllers, Middlewares, Filters)                   │
├─────────────────────────────────────────────────────────┤
│                    Application Layer                    │
│   (Services, DTOs, Interfaces, Mappers)                │
├─────────────────────────────────────────────────────────┤
│                      Domain Layer                       │
│   (Entities, Aggregates, Value Objects, Domain Events) │
├─────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                  │
│   (Repositories, Database, External Services)          │
└─────────────────────────────────────────────────────────┘
```

### 2.3 项目依赖关系

```
Dawning.Identity.Api
    ├── Dawning.Identity.Application
    │       ├── Dawning.Identity.Domain
    │       │       └── Dawning.Identity.Domain.Core
    │       └── Dawning.Shared.Dapper.Contrib
    └── Dawning.Identity.Infra.CrossCutting.IoC
            └── Dawning.Identity.Infra.CrossCutting.Mapping
```

---

## 3. 开发环境搭建

### 3.1 必备工具

- **IDE**: Visual Studio 2022 / VS Code / Rider
- **.NET SDK**: 8.0+
- **Node.js**: 18.x+
- **pnpm**: 8.x+
- **MySQL**: 8.0+
- **Redis**: 6.0+ (可选)
- **Git**: 2.x+

### 3.2 克隆项目

```bash
git clone https://github.com/changjian-wang/dawning.git
cd dawning
```

### 3.3 后端配置

1. **配置数据库连接**

编辑 `Dawning.Gateway/src/Dawning.Identity.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DawningIdentity": "Server=localhost;Port=3306;Database=dawning_identity;Uid=root;Pwd=your_password;",
    "Redis": "localhost:6379"
  }
}
```

2. **还原依赖**

```bash
cd Dawning.Gateway
dotnet restore
```

3. **初始化数据库**

```bash
# 创建数据库
mysql -u root -p -e "CREATE DATABASE dawning_identity CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# 执行初始化脚本
mysql -u root -p dawning_identity < docs/sql/dawning_identity.sql

# 执行迁移脚本
mysql -u root -p dawning_identity < docs/sql/migrations/V2_remove_soft_delete.sql
mysql -u root -p dawning_identity < docs/sql/migrations/V3_add_login_lockout.sql
mysql -u root -p dawning_identity < docs/sql/migrations/V4_add_user_sessions.sql
mysql -u root -p dawning_identity < docs/sql/migrations/V5_add_request_logs.sql
mysql -u root -p dawning_identity < docs/sql/migrations/V6_add_backup_records.sql
```

4. **运行后端**

```bash
dotnet run --project src/Dawning.Identity.Api
```

后端服务将在 `https://localhost:5001` 启动。

### 3.4 前端配置

1. **安装依赖**

```bash
cd dawning-admin
pnpm install
```

2. **配置环境变量**

创建 `.env.development`:

```env
VITE_API_BASE_URL=https://localhost:5001
```

3. **运行前端**

```bash
pnpm dev
```

前端将在 `http://localhost:5173` 启动。

### 3.5 Docker 开发环境

使用 Docker Compose 快速启动：

```bash
docker-compose -f docker-compose.dev.yml up -d
```

---

## 4. 项目结构

### 4.1 后端项目结构

```
Dawning.Gateway/
├── docs/                           # 文档
│   └── sql/                        # SQL 脚本
│       └── migrations/             # 迁移脚本
├── src/
│   ├── Dawning.Gateway.Api/        # 网关 API
│   ├── Dawning.Identity.Api/       # 身份认证 API
│   │   ├── Configurations/         # 配置类
│   │   ├── Controllers/            # 控制器
│   │   ├── Data/                   # 数据初始化
│   │   ├── Helpers/                # 辅助类
│   │   ├── Middleware/             # 中间件
│   │   └── Services/               # API 层服务
│   ├── Dawning.Identity.Application/  # 应用层
│   │   ├── DTOs/                   # 数据传输对象
│   │   ├── Interfaces/             # 服务接口
│   │   ├── Mappers/                # 对象映射
│   │   └── Services/               # 服务实现
│   ├── Dawning.Identity.Domain/    # 领域层
│   │   ├── Aggregates/             # 聚合根
│   │   └── Interfaces/             # 仓储接口
│   ├── Dawning.Identity.Domain.Core/  # 领域核心
│   │   ├── Models/                 # 基础模型
│   │   └── Security/               # 安全工具
│   └── Shared/                     # 共享库
│       └── Dawning.Shared.Dapper.Contrib/  # Dapper 扩展
└── tests/                          # 测试项目
```

### 4.2 前端项目结构

```
dawning-admin/
├── public/                         # 静态资源
├── src/
│   ├── api/                        # API 客户端
│   ├── assets/                     # 资源文件
│   ├── components/                 # 通用组件
│   ├── config/                     # 配置
│   ├── hooks/                      # 自定义 Hooks
│   ├── layout/                     # 布局组件
│   ├── locale/                     # 国际化
│   ├── router/                     # 路由配置
│   ├── store/                      # 状态管理
│   ├── types/                      # TypeScript 类型
│   ├── utils/                      # 工具函数
│   └── views/                      # 页面组件
├── config/                         # Vite 配置
└── package.json
```

---

## 5. 后端开发指南

### 5.1 创建新功能模块

以创建"通知管理"模块为例：

#### 1. 定义领域实体

```csharp
// Dawning.Identity.Domain/Aggregates/Notification/Notification.cs
namespace Dawning.Identity.Domain.Aggregates.Notification
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Guid UserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

#### 2. 定义仓储接口

```csharp
// Dawning.Identity.Domain/Interfaces/INotificationRepository.cs
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<Guid> CreateAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(Guid id);
}
```

#### 3. 定义 DTO

```csharp
// Dawning.Identity.Application/DTOs/Notification/NotificationDto.cs
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### 4. 定义服务接口

```csharp
// Dawning.Identity.Application/Interfaces/INotificationService.cs
public interface INotificationService
{
    Task<NotificationDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync();
    Task<Guid> CreateAsync(CreateNotificationDto dto);
    Task MarkAsReadAsync(Guid id);
    Task DeleteAsync(Guid id);
}
```

#### 5. 实现服务

```csharp
// Dawning.Identity.Application/Services/NotificationService.cs
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(
        INotificationRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync()
    {
        var userId = _currentUser.GetUserId();
        var notifications = await _repository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }
    
    // ... 其他方法
}
```

#### 6. 创建控制器

```csharp
// Dawning.Identity.Api/Controllers/NotificationController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> GetMyNotifications()
    {
        var notifications = await _service.GetMyNotificationsAsync();
        return Ok(notifications);
    }
    
    // ... 其他端点
}
```

#### 7. 注册依赖

```csharp
// Program.cs
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
```

### 5.2 中间件开发

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomMiddleware> _logger;

    public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 请求前处理
        _logger.LogInformation("Before request");
        
        await _next(context);
        
        // 请求后处理
        _logger.LogInformation("After request");
    }
}

// 注册中间件
app.UseMiddleware<CustomMiddleware>();
```

### 5.3 仓储模式

使用 Dapper 实现仓储：

```csharp
public class UserRepository : IUserRepository
{
    private readonly IDbConnection _connection;

    public UserRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, username, email, password_hash, created_at, updated_at
            FROM users 
            WHERE id = @Id";
        
        return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id.ToString() });
    }

    public async Task<Guid> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (id, username, email, password_hash, created_at)
            VALUES (@Id, @Username, @Email, @PasswordHash, @CreatedAt)";
        
        await _connection.ExecuteAsync(sql, new
        {
            Id = user.Id.ToString(),
            user.Username,
            user.Email,
            user.PasswordHash,
            user.CreatedAt
        });
        
        return user.Id;
    }
}
```

### 5.4 QueryBuilder 使用

```csharp
// 基本查询
var users = await _connection.QueryBuilder<User>()
    .Where(u => u.IsActive == true)
    .OrderBy(u => u.CreatedAt)
    .Page(1, 20)
    .ToListAsync();

// 条件查询
var query = _connection.QueryBuilder<User>();

if (!string.IsNullOrEmpty(keyword))
{
    query.Where(u => u.Username.Contains(keyword) || u.Email.Contains(keyword));
}

if (status.HasValue)
{
    query.Where(u => u.Status == status.Value);
}

var result = await query.ToPagedListAsync(page, pageSize);
```

---

## 6. 前端开发指南

### 6.1 API 客户端

```typescript
// src/api/notification.ts
import axios from './interceptor';

export interface Notification {
  id: string;
  title: string;
  content: string;
  isRead: boolean;
  createdAt: string;
}

export function getMyNotifications() {
  return axios.get<Notification[]>('/api/notification');
}

export function markAsRead(id: string) {
  return axios.post(`/api/notification/${id}/read`);
}
```

### 6.2 页面组件

```vue
<!-- src/views/notification/index.vue -->
<template>
  <div class="notification-page">
    <a-table :columns="columns" :data="notifications" :loading="loading">
      <template #title="{ record }">
        <span :class="{ 'unread': !record.isRead }">{{ record.title }}</span>
      </template>
      <template #action="{ record }">
        <a-button v-if="!record.isRead" @click="handleMarkAsRead(record.id)">
          标记已读
        </a-button>
      </template>
    </a-table>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getMyNotifications, markAsRead, Notification } from '@/api/notification';
import { Message } from '@arco-design/web-vue';

const notifications = ref<Notification[]>([]);
const loading = ref(false);

const columns = [
  { title: '标题', slotName: 'title' },
  { title: '时间', dataIndex: 'createdAt' },
  { title: '操作', slotName: 'action' },
];

const fetchData = async () => {
  loading.value = true;
  try {
    const { data } = await getMyNotifications();
    notifications.value = data;
  } finally {
    loading.value = false;
  }
};

const handleMarkAsRead = async (id: string) => {
  await markAsRead(id);
  Message.success('已标记为已读');
  await fetchData();
};

onMounted(() => {
  fetchData();
});
</script>
```

### 6.3 路由配置

```typescript
// src/router/routes/modules/notification.ts
export default {
  path: '/notification',
  name: 'notification',
  component: () => import('@/views/notification/index.vue'),
  meta: {
    title: '通知管理',
    requiresAuth: true,
    icon: 'icon-notification',
  },
};
```

### 6.4 状态管理

```typescript
// src/store/modules/notification.ts
import { defineStore } from 'pinia';
import { getMyNotifications, Notification } from '@/api/notification';

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [] as Notification[],
    unreadCount: 0,
  }),
  
  actions: {
    async fetchNotifications() {
      const { data } = await getMyNotifications();
      this.notifications = data;
      this.unreadCount = data.filter(n => !n.isRead).length;
    },
  },
});
```

---

## 7. 数据库设计

### 7.1 命名规范

- 表名：小写，下划线分隔（如 `user_roles`）
- 字段名：小写，下划线分隔（如 `created_at`）
- 主键：`id`（CHAR(36) UUID）
- 外键：`{关联表}_id`（如 `user_id`）
- 时间字段：`created_at`, `updated_at`

### 7.2 核心表结构

详见 `docs/sql/` 目录下的 SQL 脚本。

### 7.3 迁移脚本规范

迁移脚本命名：`V{版本号}_{描述}.sql`

```sql
-- =====================================================
-- V7_add_notifications.sql
-- 通知表创建脚本
-- 创建日期: 2025-12-16
-- 描述: 添加通知功能
-- =====================================================

CREATE TABLE IF NOT EXISTS notifications (
    id CHAR(36) NOT NULL PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    content TEXT NOT NULL,
    user_id CHAR(36) NOT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL,
    
    INDEX idx_notifications_user_id (user_id),
    INDEX idx_notifications_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 回滚脚本（注释状态）
-- DROP TABLE IF EXISTS notifications;
```

---

## 8. API 规范

### 8.1 URL 规范

- 使用小写字母和连字符
- RESTful 风格
- 版本号可选（当前无版本号）

```
GET    /api/users           # 获取用户列表
GET    /api/users/{id}      # 获取单个用户
POST   /api/users           # 创建用户
PUT    /api/users/{id}      # 更新用户
DELETE /api/users/{id}      # 删除用户
```

### 8.2 响应格式

**成功响应**：

```json
{
  "id": "xxx",
  "username": "admin",
  "email": "admin@example.com"
}
```

**分页响应**：

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

**错误响应**：

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "用户名已存在",
  "traceId": "xxx"
}
```

### 8.3 HTTP 状态码

| 状态码 | 说明 |
|--------|------|
| 200 | 成功 |
| 201 | 创建成功 |
| 204 | 删除成功（无返回内容） |
| 400 | 请求参数错误 |
| 401 | 未认证 |
| 403 | 无权限 |
| 404 | 资源不存在 |
| 500 | 服务器错误 |

---

## 9. 测试指南

### 9.1 单元测试

```csharp
// Dawning.Identity.Application.Tests/UserServiceTests.cs
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new UserService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "test" };
        _mockRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDto>(user)).Returns(new UserDto { Id = userId });

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }
}
```

### 9.2 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定项目测试
dotnet test src/Dawning.Identity.Application.Tests

# 运行带覆盖率
dotnet test --collect:"XPlat Code Coverage"
```

### 9.3 前端测试

```bash
# 运行测试
pnpm test

# 运行 E2E 测试
pnpm test:e2e
```

---

## 10. 部署指南

详见 [部署文档](./DEPLOYMENT.md)

### 10.1 快速部署（Docker）

```bash
# 构建镜像
docker-compose build

# 启动服务
docker-compose up -d

# 查看日志
docker-compose logs -f
```

### 10.2 生产环境配置

详见 [部署文档](./DEPLOYMENT.md) 中的生产环境配置章节。

---

## 附录

### A. 常用命令

```bash
# 后端
dotnet build                    # 构建项目
dotnet run                      # 运行项目
dotnet test                     # 运行测试
dotnet ef migrations add xxx    # 添加迁移（如使用 EF）

# 前端
pnpm install                    # 安装依赖
pnpm dev                        # 开发模式
pnpm build                      # 生产构建
pnpm lint                       # 代码检查
```

### B. 参考资源

- [.NET 文档](https://docs.microsoft.com/dotnet/)
- [OpenIddict 文档](https://documentation.openiddict.com/)
- [YARP 文档](https://microsoft.github.io/reverse-proxy/)
- [Vue 3 文档](https://vuejs.org/)
- [Arco Design Vue](https://arco.design/vue)

---

**文档维护**: Dawning 开发团队  
**最后更新**: 2025-12-16
