# Dawning.Caching

缓存服务库，支持内存缓存和 Redis 分布式缓存。

## 安装

```bash
dotnet add package Dawning.Caching
```

## 功能特性

- ✅ 统一的缓存接口 `ICacheService`
- ✅ 内存缓存支持
- ✅ Redis 分布式缓存支持
- ✅ 可配置的过期时间
- ✅ 键前缀支持
- ✅ GetOrSet 模式

## 快速开始

### 内存缓存

```csharp
builder.Services.AddDawningMemoryCache(options =>
{
    options.DefaultExpirationMinutes = 30;
    options.KeyPrefix = "myapp";
});
```

### Redis 缓存

```csharp
builder.Services.AddDawningRedisCache("localhost:6379", options =>
{
    options.DefaultExpirationMinutes = 60;
    options.KeyPrefix = "myapp";
    options.Redis.InstanceName = "MyApp:";
});
```

### 使用缓存

```csharp
public class UserService
{
    private readonly ICacheService _cache;

    public UserService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        return await _cache.GetOrSetAsync(
            $"user:{id}",
            async () => await _userRepository.GetByIdAsync(id),
            TimeSpan.FromMinutes(10));
    }

    public async Task InvalidateUserCacheAsync(int id)
    {
        await _cache.RemoveAsync($"user:{id}");
    }
}
```

## 配置选项

### appsettings.json

```json
{
  "Caching": {
    "Provider": "Redis",
    "DefaultExpirationMinutes": 30,
    "KeyPrefix": "myapp",
    "Redis": {
      "ConnectionString": "localhost:6379",
      "InstanceName": "MyApp:",
      "Database": 0
    }
  }
}
```

## API 参考

### ICacheService

| 方法 | 描述 |
|------|------|
| `GetAsync<T>(key)` | 获取缓存值 |
| `SetAsync<T>(key, value, expiration?)` | 设置缓存值 |
| `GetOrSetAsync<T>(key, factory, expiration?)` | 获取或设置缓存值 |
| `RemoveAsync(key)` | 移除缓存 |
| `RemoveByPatternAsync(pattern)` | 按模式移除缓存（仅内存缓存） |
| `ExistsAsync(key)` | 检查缓存是否存在 |
| `RefreshAsync(key, expiration)` | 刷新缓存过期时间 |
