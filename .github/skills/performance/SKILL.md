---
description: "Performance analysis for Dawning: N+1 queries, memory allocation, async patterns, database optimization. Trigger: 性能, performance, 优化, optimize, 慢查询, slow, N+1, 内存, memory"
---

# Performance Skill

## 目标

分析和优化 Dawning 项目的性能问题。

## 触发条件

- **关键词**：性能, performance, 优化, optimize, 慢查询, slow query, N+1, 内存, memory, 缓存, cache
- **文件模式**：`*.cs`, `*.sql`
- **用户意图**：性能分析、优化查询、减少内存分配

## 编排

- **前置**：无
- **后续**：`create-tests`（优化后验证功能不破坏）

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} performance — {触发原因}`

---

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

## 验收场景

- **输入**："用户列表接口很慢"
- **预期**：agent 检查 SQL 查询（N+1、缺少索引）、缓存策略、异步模式
- **上次验证**：2026-02-27 ✅
