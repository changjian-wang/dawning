---
description: "Use when: Analyzing N+1 queries, memory allocation issues, async anti-patterns, database optimization, or caching strategy\nDon't use when: Fixing bugs (use code-patterns), auditing code (use code-review)\nInputs: Performance concern or slow operation to analyze\nOutputs: Root cause analysis with specific optimization recommendations\nSuccess criteria: Performance issue identified, optimization applied, measurable improvement"
---

# Performance Skill

## 常见性能问题

### 1. 数据库相关

#### N+1 查询

```csharp
// ❌ N+1 问题
var users = await _userRepo.GetAllAsync();
foreach (var user in users)
    user.Roles = await _roleRepo.GetByUserIdAsync(user.Id);

// ✅ 批量查询
var userIds = users.Select(u => u.Id).ToList();
var allRoles = await _roleRepo.GetByUserIdsAsync(userIds);
```

#### 缺少索引

```sql
EXPLAIN SELECT * FROM users WHERE username = 'test';
CREATE INDEX idx_username ON users(username);
```

#### 查询返回过多数据

```csharp
// ❌ 返回所有
SELECT * FROM large_table;

// ✅ 只查需要的
SELECT id, name, status FROM large_table WHERE status = 1 LIMIT 1000;
```

### 2. 内存相关

```csharp
// ❌ 循环中字符串拼接
string result = "";
foreach (var item in items)
    result += item.Name + ",";

// ✅ string.Join
var result = string.Join(",", items.Select(x => x.Name));
```

### 3. 异步相关

```csharp
// ❌ 阻塞异步
var result = GetDataAsync().Result;

// ✅ 正确使用
var result = await GetDataAsync();

// ❌ 不必要的异步
public async Task<int> Calculate(int a, int b) => await Task.FromResult(a + b);

// ✅ 同步即可
public int Calculate(int a, int b) => a + b;
```

### 4. 缓存策略

```csharp
// 合理使用缓存减少数据库查询
await _cache.SetAsync(CacheKeyConstants.User.ById(id), user, TimeSpan.FromMinutes(30));
```

