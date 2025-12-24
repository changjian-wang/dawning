# Dawning 认证接入指南

本文档说明如何在新业务系统中接入 Dawning 统一认证中心。

## 目录

- [快速开始](#快速开始)
- [配置说明](#配置说明)
- [使用方式](#使用方式)
- [常见场景](#常见场景)
- [网关路由配置](#网关路由配置)
- [故障排查](#故障排查)

---

## 快速开始

### 1. 添加项目引用

```xml
<!-- 在你的 .csproj 中添加 -->
<ItemGroup>
  <ProjectReference Include="..\Shared\Dawning.Shared.Authentication\Dawning.Shared.Authentication.csproj" />
</ItemGroup>
```

或者发布为 NuGet 包后:

```xml
<PackageReference Include="Dawning.Shared.Authentication" Version="1.0.0" />
```

### 2. 配置 appsettings.json

```json
{
  "DawningAuth": {
    "Authority": "http://localhost:5202",
    "Issuer": "http://localhost:5202",
    "RequireHttpsMetadata": false,
    "ClockSkewMinutes": 5
  }
}
```

**生产环境配置:**

```json
{
  "DawningAuth": {
    "Authority": "https://identity.yourdomain.com",
    "Issuer": "https://identity.yourdomain.com",
    "RequireHttpsMetadata": true,
    "ClockSkewMinutes": 5
  }
}
```

### 3. 注册服务 (Program.cs)

```csharp
using Dawning.Shared.Authentication.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加 Dawning 认证
builder.Services.AddDawningAuthentication(builder.Configuration);

// 添加 Dawning 授权策略 (可选)
builder.Services.AddDawningAuthorization();

var app = builder.Build();

// 启用认证和授权中间件
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

就这样！你的服务现在已经接入统一认证了。

---

## 配置说明

| 配置项 | 说明 | 默认值 | 必填 |
|--------|------|--------|:----:|
| `Authority` | Identity 服务器地址 | - | ✅ |
| `Issuer` | Token 签发者 | - | ✅ |
| `Audience` | 受众 (留空不验证) | null | ❌ |
| `RequireHttpsMetadata` | 是否要求 HTTPS | true | ❌ |
| `ClockSkewMinutes` | Token 过期容差(分钟) | 5 | ❌ |
| `ValidateRoles` | 是否验证角色 | true | ❌ |
| `ValidatePermissions` | 是否验证权限 | true | ❌ |
| `ValidateTenant` | 是否验证租户 | false | ❌ |
| `GatewayUrl` | API 网关地址 | null | ❌ |

---

## 使用方式

### 方式一: 使用 [Authorize] 特性

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    // 需要认证
    [Authorize]
    [HttpGet]
    public IActionResult GetOrders()
    {
        return Ok();
    }

    // 需要特定角色
    [Authorize(Roles = "admin,super_admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        return Ok();
    }
}
```

### 方式二: 使用 Dawning 授权特性

```csharp
using Dawning.Shared.Authentication.Attributes;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    // 仅管理员
    [AdminOnly]
    [HttpPost]
    public IActionResult CreateProduct()
    {
        return Ok();
    }

    // 仅超级管理员
    [SuperAdminOnly]
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        return Ok();
    }

    // 需要特定权限
    [RequirePermission("product.update")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id)
    {
        return Ok();
    }
}
```

### 方式三: 注入 ICurrentUser 获取当前用户

```csharp
using Dawning.Shared.Authentication;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public ProfileController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Email = _currentUser.Email,
            Roles = _currentUser.Roles,
            TenantId = _currentUser.TenantId,
            IsAdmin = _currentUser.IsAdmin
        });
    }

    [Authorize]
    [HttpGet("permissions")]
    public IActionResult GetPermissions()
    {
        // 检查权限
        if (!_currentUser.HasPermission("profile.read"))
        {
            return Forbid();
        }

        return Ok(_currentUser.Permissions);
    }
}
```

### 方式四: 使用 ClaimsPrincipal 扩展方法

```csharp
using Dawning.Shared.Authentication.Extensions;

[Authorize]
[HttpGet("info")]
public IActionResult GetUserInfo()
{
    var user = User; // ControllerBase.User

    return Ok(new
    {
        UserId = user.GetUserId(),
        UserName = user.GetUserName(),
        Roles = user.GetRoles(),
        Permissions = user.GetPermissions(),
        TenantId = user.GetTenantId(),
        IsSuperAdmin = user.IsSuperAdmin(),
        IsHost = user.IsHost()
    });
}
```

---

## 常见场景

### 场景一: 多租户数据隔离

```csharp
public class OrderService
{
    private readonly ICurrentUser _currentUser;
    private readonly IDbConnection _db;

    public async Task<List<Order>> GetOrdersAsync()
    {
        var tenantId = _currentUser.TenantId;
        
        // 宿主管理员可看所有数据
        if (_currentUser.IsHost)
        {
            return await _db.QueryAsync<Order>("SELECT * FROM orders");
        }

        // 普通用户只能看自己租户的数据
        return await _db.QueryAsync<Order>(
            "SELECT * FROM orders WHERE tenant_id = @TenantId",
            new { TenantId = tenantId });
    }
}
```

### 场景二: 审计日志

```csharp
public class AuditService
{
    private readonly ICurrentUser _currentUser;

    public void LogAction(string action, string entity)
    {
        var log = new AuditLog
        {
            UserId = _currentUser.UserIdAsGuid ?? Guid.Empty,
            UserName = _currentUser.UserName ?? "Anonymous",
            TenantId = _currentUser.TenantIdAsGuid,
            Action = action,
            Entity = entity,
            Timestamp = DateTime.UtcNow,
            IpAddress = GetClientIp()
        };

        // 保存日志...
    }
}
```

### 场景三: 动态权限检查

```csharp
public class DocumentService
{
    private readonly ICurrentUser _currentUser;

    public async Task<Document> GetDocumentAsync(Guid id)
    {
        var document = await _repository.GetByIdAsync(id);

        // 检查是否有权限访问
        if (document.IsConfidential && 
            !_currentUser.HasPermission("document.confidential"))
        {
            throw new UnauthorizedAccessException("无权访问机密文档");
        }

        // 检查租户
        if (!_currentUser.IsHost && 
            document.TenantId != _currentUser.TenantIdAsGuid)
        {
            throw new UnauthorizedAccessException("无权访问其他租户的文档");
        }

        return document;
    }
}
```

---

## 网关路由配置

新服务需要在网关中配置路由才能被外部访问。

### 方式一: 管理界面配置

1. 登录 Dawning Admin (http://localhost:5173)
2. 进入 **网关管理** → **集群管理**
3. 添加新集群，填入你的服务地址
4. 进入 **路由管理**
5. 添加新路由，关联集群

### 方式二: 数据库配置

```sql
-- 1. 添加集群
INSERT INTO gateway_clusters (id, cluster_id, destinations, created_at, updated_at)
VALUES (
    UUID(),
    'my-service',
    '[{"Address": "http://localhost:5300"}]',
    NOW(),
    NOW()
);

-- 2. 添加路由
INSERT INTO gateway_routes (id, route_id, cluster_id, match_path, transforms, created_at, updated_at)
VALUES (
    UUID(),
    'my-service-route',
    'my-service',
    '/api/my-service/{**catch-all}',
    '{"RequestHeader": "X-Forwarded-Prefix"}',
    NOW(),
    NOW()
);
```

### 方式三: appsettings.json 配置

在 `Dawning.Gateway.Api/appsettings.json` 中:

```json
{
  "ReverseProxy": {
    "Clusters": {
      "my-service": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5300"
          }
        }
      }
    },
    "Routes": {
      "my-service-route": {
        "ClusterId": "my-service",
        "Match": {
          "Path": "/api/my-service/{**catch-all}"
        }
      }
    }
  }
}
```

---

## 故障排查

### 问题一: 401 Unauthorized

**检查步骤:**

1. 确认请求头包含 `Authorization: Bearer <token>`
2. 确认 Token 未过期
3. 确认 `Authority` 和 `Issuer` 配置正确
4. 检查 Identity 服务是否可访问

**调试代码:**

```csharp
// 在 Controller 中
[AllowAnonymous]
[HttpGet("debug-auth")]
public IActionResult DebugAuth()
{
    var authHeader = Request.Headers["Authorization"].ToString();
    var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
    var claims = User.Claims.Select(c => new { c.Type, c.Value });

    return Ok(new { authHeader, isAuthenticated, claims });
}
```

### 问题二: 403 Forbidden

**检查步骤:**

1. 确认用户有所需角色
2. 确认用户有所需权限
3. 检查 Token 中的 Claims

**查看 Token 内容:**

```bash
# 在 https://jwt.io 解析 Token 查看 Claims
```

### 问题三: Token 验证失败

**常见原因:**

| 错误 | 原因 | 解决方案 |
|------|------|----------|
| `IDX10214` | Audience 验证失败 | 移除 Audience 验证或配置正确的 Audience |
| `IDX10223` | Token 已过期 | 刷新 Token 或增加 ClockSkew |
| `IDX10500` | 签名验证失败 | 确认 Authority 可访问 |

### 问题四: 开发环境 HTTPS 问题

```json
{
  "DawningAuth": {
    "RequireHttpsMetadata": false  // 开发环境设为 false
  }
}
```

---

## 附录: 完整示例

### 新服务 Program.cs 完整示例

```csharp
using Dawning.Shared.Authentication.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== 基础服务 =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===== Swagger 配置 (带 Token 支持) =====
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Service API", Version = "v1" });

    // 添加 Bearer Token 认证
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== Dawning 认证 =====
builder.Services.AddDawningAuthentication(builder.Configuration);
builder.Services.AddDawningAuthorization();

var app = builder.Build();

// ===== 中间件 =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### 新服务 appsettings.json 完整示例

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DawningAuth": {
    "Authority": "http://localhost:5202",
    "Issuer": "http://localhost:5202",
    "RequireHttpsMetadata": false,
    "ClockSkewMinutes": 5
  }
}
```

---

## 联系与支持

如有问题，请联系:

- 查看 API 文档: http://localhost:5202/swagger
- 查看管理后台: http://localhost:5173
- 查看项目文档: `/docs` 目录
