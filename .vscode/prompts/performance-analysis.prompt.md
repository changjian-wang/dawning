---
mode: agent
description: 性能分析与优化建议
tools: ["read_file", "grep_search", "semantic_search"]
---

# 性能分析与优化

对代码进行性能分析，识别潜在的性能问题并提供优化建议。

## 常见性能问题

### 1. 数据库相关

#### N+1 查询问题
```csharp
// ❌ N+1 问题
var users = await _userRepo.GetAllAsync();
foreach (var user in users)
{
    user.Roles = await _roleRepo.GetByUserIdAsync(user.Id);  // 每个用户执行一次查询
}

// ✅ 使用 JOIN 或批量查询
var users = await _userRepo.GetAllWithRolesAsync();
// 或
var userIds = users.Select(u => u.Id).ToList();
var allRoles = await _roleRepo.GetByUserIdsAsync(userIds);  // 一次批量查询
```

#### 缺少索引
```sql
-- 检查慢查询是否有合适的索引
EXPLAIN SELECT * FROM users WHERE username = 'test';

-- 添加索引
CREATE INDEX idx_username ON users(username);
```

#### 查询返回过多数据
```csharp
// ❌ 返回所有字段
SELECT * FROM large_table;

// ✅ 只查需要的字段
SELECT id, name, status FROM large_table WHERE status = 1;
```

### 2. 内存相关

#### 大集合加载到内存
```csharp
// ❌ 一次性加载全部
var allLogs = await _context.RequestLogs.ToListAsync();
var filtered = allLogs.Where(x => x.StatusCode >= 500);

// ✅ 在数据库端过滤
var errorLogs = await _context.RequestLogs
    .Where(x => x.StatusCode >= 500)
    .Take(1000)
    .ToListAsync();
```

#### 字符串拼接
```csharp
// ❌ 循环中字符串拼接
string result = "";
foreach (var item in items)
{
    result += item.Name + ",";
}

// ✅ 使用 StringBuilder 或 string.Join
var result = string.Join(",", items.Select(x => x.Name));
```

### 3. 异步相关

#### 阻塞异步调用
```csharp
// ❌ 阻塞等待
var result = GetDataAsync().Result;
var data = GetDataAsync().GetAwaiter().GetResult();

// ✅ 正确使用 async/await
var result = await GetDataAsync();
```

#### 不必要的异步
```csharp
// ❌ 没有 I/O 操作却标记为 async
public async Task<int> Calculate(int a, int b)
{
    return await Task.FromResult(a + b);
}

// ✅ 同步即可
public int Calculate(int a, int b)
{
    return a + b;
}
```

### 4. 缓存相关

#### 缺少缓存
```csharp
// ❌ 每次都查询数据库
public async Task<List<Category>> GetAllCategoriesAsync()
{
    return await _repo.GetAllAsync();
}

// ✅ 使用缓存
public async Task<List<Category>> GetAllCategoriesAsync()
{
    return await _cache.GetOrSetAsync(
        "categories:all",
        () => _repo.GetAllAsync(),
        TimeSpan.FromMinutes(10));
}
```

#### 缓存穿透/击穿
```csharp
// ✅ 防止缓存穿透（缓存空值）
public async Task<UserDto?> GetByIdAsync(Guid id)
{
    var cacheKey = $"user:{id}";
    var cached = await _cache.GetAsync<CacheWrapper<UserDto>>(cacheKey);
    
    if (cached != null)
        return cached.Value;  // 可能是 null，但已缓存
    
    var user = await _repo.GetByIdAsync(id);
    await _cache.SetAsync(cacheKey, new CacheWrapper<UserDto>(user), TimeSpan.FromMinutes(5));
    return user;
}
```

### 5. HTTP 相关

#### 未复用 HttpClient
```csharp
// ❌ 每次创建新实例
using var client = new HttpClient();
var result = await client.GetAsync(url);

// ✅ 使用 IHttpClientFactory
public class MyService(IHttpClientFactory factory)
{
    public async Task<string> GetDataAsync()
    {
        var client = factory.CreateClient();
        return await client.GetStringAsync(url);
    }
}
```

## 分析输出格式

```markdown
## 性能分析报告

### 🔴 严重问题
- **位置**: 文件:行号
- **问题类型**: N+1 查询 / 内存泄漏 / 阻塞调用
- **影响**: 描述性能影响
- **修复建议**: 具体代码示例

### 🟡 潜在问题
- **位置**: 文件:行号
- **问题类型**: 缺少缓存 / 未优化查询
- **建议**: 优化方案

### 🟢 优化建议
- 可选的性能提升建议

### 📊 性能评估
- 预估响应时间影响
- 资源占用评估
- 优先级建议
```

## 性能检查清单

- [ ] 数据库查询有适当的索引
- [ ] 避免 N+1 查询问题
- [ ] 正确使用 async/await
- [ ] 频繁访问的数据使用缓存
- [ ] 大集合使用分页
- [ ] HttpClient 正确复用
- [ ] 避免字符串循环拼接
- [ ] 避免大对象频繁创建
