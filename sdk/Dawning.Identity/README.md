# Dawning.Identity

JWT 认证和用户上下文库。

## 安装

```bash
dotnet add package Dawning.Identity
```

## 功能

- **JWT 认证集成** - 简化 JWT Bearer 认证配置
- **用户上下文** - `CurrentUser` 服务
- **Claims 扩展** - 便捷的 Claims 读取方法

## 使用

### 注册服务

```csharp
builder.Services.AddDawningAuthentication(options =>
{
    options.Authority = "https://your-identity-server";
    options.Audience = "your-api";
});
```

### 注入当前用户

```csharp
public class MyService
{
    private readonly CurrentUser _currentUser;

    public MyService(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void DoSomething()
    {
        var userId = _currentUser.Id;
        var userName = _currentUser.UserName;
        var tenantId = _currentUser.TenantId;
        var roles = _currentUser.Roles;
    }
}
```

### Claims 扩展

```csharp
var userId = User.GetUserId();
var tenantId = User.GetTenantId();
bool isAdmin = User.HasRole("Admin");
```
