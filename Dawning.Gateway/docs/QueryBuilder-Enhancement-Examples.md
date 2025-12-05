# QueryBuilder<TEntity> 增强功能说明

## 📋 新增功能概览

本次优化为 `QueryBuilder<TEntity>` 添加了 **6 项增强功能**：

### 1️⃣ Where 方法（无条件版本）
**用途**：简化常规条件添加，无需每次都写 `WhereIf(true, ...)`

```csharp
// ❌ 之前写法
var users = connection.Builder<User>()
    .WhereIf(true, x => x.Age > 18)  // 总是传 true 很冗余
    .AsList();

// ✅ 现在写法
var users = connection.Builder<User>()
    .Where(x => x.Age > 18)  // 更简洁
    .AsList();
```

---

### 2️⃣ ThenBy / ThenByDescending（多列排序）
**用途**：支持二次、三次排序，常用于复杂业务场景

```csharp
// 示例：按部门分组，同部门内按薪资降序，薪资相同按入职时间升序
var employees = connection.Builder<Employee>()
    .Where(x => x.IsActive)
    .OrderBy(x => x.DepartmentId)          // 第一排序：部门
    .ThenByDescending(x => x.Salary)       // 第二排序：薪资（降序）
    .ThenBy(x => x.JoinDate)               // 第三排序：入职时间（升序）
    .AsList();

// 生成 SQL：
// SELECT * FROM Employees 
// WHERE IsActive = @IsActive 
// ORDER BY DepartmentId ASC, Salary DESC, JoinDate ASC
```

---

### 3️⃣ FirstOrDefault（单条查询）
**用途**：获取第一条记录，性能优于 `AsList().FirstOrDefault()`

```csharp
// ❌ 低效写法（查询所有数据再取第一条）
var admin = connection.Builder<User>()
    .Where(x => x.Role == "Admin")
    .AsList()
    .FirstOrDefault();

// ✅ 高效写法（数据库层面直接 LIMIT 1）
var admin = connection.Builder<User>()
    .Where(x => x.Role == "Admin")
    .OrderByDescending(x => x.CreatedAt)
    .FirstOrDefault();  // 自动添加 LIMIT 1 或 TOP 1

// 生成 SQL (MySQL)：
// SELECT * FROM Users 
// WHERE Role = @Role 
// ORDER BY CreatedAt DESC 
// LIMIT 1
```

---

### 4️⃣ Count / Any / None（计数与存在性检查）
**用途**：高效统计和判断，避免加载完整数据

```csharp
// Count：获取总数
long activeUserCount = connection.Builder<User>()
    .Where(x => x.IsActive)
    .Count();  // 返回 1234

// Any：判断是否存在
bool hasAdmins = connection.Builder<User>()
    .Where(x => x.Role == "Admin")
    .Any();  // 返回 true/false

// None：判断是否不存在（语义更清晰）
bool noDeletedUsers = connection.Builder<User>()
    .Where(x => x.IsDeleted)
    .None();  // 等价于 !Any()

// 生成 SQL：
// SELECT COUNT(*) FROM Users WHERE IsDeleted = @IsDeleted
```

---

### 5️⃣ Take / Skip（限制与跳过）
**用途**：灵活控制结果集大小，适用于自定义分页

```csharp
// Take：限制前 N 条
var top10Users = connection.Builder<User>()
    .Where(x => x.IsActive)
    .OrderByDescending(x => x.Score)
    .Take(10)  // 只取前 10 条
    .AsList();

// Skip + Take：跳过前 N 条，然后取 M 条
var page2Users = connection.Builder<User>()
    .OrderBy(x => x.Id)
    .Skip(20)   // 跳过前 20 条
    .Take(10)   // 取接下来的 10 条
    .AsList();

// 组合使用（手动分页）
int pageIndex = 2;  // 第 3 页（从 0 开始）
int pageSize = 20;
var page3 = connection.Builder<User>()
    .Where(x => !x.IsDeleted)
    .OrderBy(x => x.CreatedAt)
    .Skip(pageIndex * pageSize)  // 跳过 40 条
    .Take(pageSize)              // 取 20 条
    .AsList();

// 生成 SQL (MySQL)：
// SELECT * FROM Users 
// WHERE IsDeleted = @IsDeleted 
// ORDER BY CreatedAt ASC 
// LIMIT 20 OFFSET 40
```

---

### 6️⃣ 多数据库语法适配
**自动处理不同数据库的 LIMIT/OFFSET 语法差异**：

| 数据库       | 生成 SQL 语法                          |
|------------|---------------------------------------|
| MySQL      | `LIMIT 10 OFFSET 20`                 |
| PostgreSQL | `LIMIT 10 OFFSET 20`                 |
| SQLite     | `LIMIT 10 OFFSET 20`                 |
| SQL Server | `OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY` |
| Firebird   | `SELECT FIRST 10 SKIP 20 *`          |

---

## 🔥 实战示例

### 场景 1：复杂查询 + 多列排序
```csharp
// 查询活跃用户，按创建时间降序，相同时间按ID升序
var users = connection.Builder<User>()
    .Where(x => x.IsActive)
    .Where(x => x.Age >= 18)  // 可以多次调用 Where
    .OrderByDescending(x => x.CreatedAt)
    .ThenBy(x => x.Id)
    .Take(50)
    .AsList();
```

### 场景 2：高效检查数据存在性
```csharp
// 检查用户名是否已存在
bool usernameExists = connection.Builder<User>()
    .Where(x => x.Username == "admin")
    .Any();

if (usernameExists)
{
    throw new InvalidOperationException("用户名已存在");
}
```

### 场景 3：获取最新记录
```csharp
// 获取最新登录的用户
var latestUser = connection.Builder<User>()
    .Where(x => x.LastLoginTime != null)
    .OrderByDescending(x => x.LastLoginTime)
    .FirstOrDefault();  // 性能优于 AsList().FirstOrDefault()
```

### 场景 4：自定义分页（不用 AsPagedList）
```csharp
// 适用于不需要总数的轻量级分页
public class UserService
{
    public List<User> GetUsersByPage(int pageIndex, int pageSize)
    {
        return _connection.Builder<User>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .AsList()
            .ToList();
    }
}
```

---

## ⚖️ AsPagedList vs Skip/Take 对比

| 功能            | AsPagedList               | Skip + Take              |
|----------------|---------------------------|--------------------------|
| 返回总数        | ✅ 自动计算 TotalItems     | ❌ 需手动 Count()         |
| 性能开销        | 较高（额外 COUNT 查询）     | 较低（只查数据）           |
| 分页信息        | ✅ 完整（Page, ItemsPerPage, TotalItems） | ❌ 需手动计算 |
| 适用场景        | 需要显示总页数的 UI 分页    | 简单列表、瀑布流、移动端加载更多 |

**建议**：
- 📄 **传统分页（带总数）**：使用 `AsPagedList`
- 📱 **无限滚动/加载更多**：使用 `Skip + Take`

---

## 🚨 注意事项

### 1. SQL Server 的 OFFSET 必须有 ORDER BY
```csharp
// ❌ SQL Server 会报错（没有 ORDER BY）
var users = connection.Builder<User>()
    .Skip(10)
    .Take(20)
    .AsList();  // Exception: OFFSET/FETCH requires ORDER BY

// ✅ 正确写法
var users = connection.Builder<User>()
    .OrderBy(x => x.Id)  // 必须指定排序
    .Skip(10)
    .Take(20)
    .AsList();
```

### 2. ThenBy 必须在 OrderBy/OrderByDescending 之后
```csharp
// ❌ 错误写法（没有主排序）
var users = connection.Builder<User>()
    .ThenBy(x => x.Name)  // 会导致排序丢失
    .AsList();

// ✅ 正确写法
var users = connection.Builder<User>()
    .OrderBy(x => x.Id)         // 主排序
    .ThenBy(x => x.Name)        // 次排序
    .AsList();
```

### 3. FirstOrDefault 返回 null 需处理
```csharp
var user = connection.Builder<User>()
    .Where(x => x.Id == 999)
    .FirstOrDefault();

if (user == null)
{
    // 处理未找到的情况
    throw new NotFoundException("用户不存在");
}
```

---

## 📊 性能对比

### Count vs AsList().Count()
```csharp
// ❌ 低效（加载所有数据到内存再统计）
int count1 = connection.Builder<User>()
    .Where(x => x.IsActive)
    .AsList()
    .Count();  // 可能加载数万条记录

// ✅ 高效（数据库层面直接 COUNT）
long count2 = connection.Builder<User>()
    .Where(x => x.IsActive)
    .Count();  // 只返回一个数字
```

### FirstOrDefault vs AsList().FirstOrDefault()
```csharp
// ❌ 低效（查询所有数据）
var user1 = connection.Builder<User>()
    .Where(x => x.Role == "Admin")
    .AsList()           // 可能加载 1000 条管理员
    .FirstOrDefault();  // 然后丢弃 999 条

// ✅ 高效（数据库层面 LIMIT 1）
var user2 = connection.Builder<User>()
    .Where(x => x.Role == "Admin")
    .FirstOrDefault();  // 数据库只返回 1 条
```

---

## ✅ 总结

| 新增方法                  | 用途                        | 性能优势         |
|--------------------------|----------------------------|-----------------|
| `Where`                  | 简化条件添加                 | 无              |
| `ThenBy/ThenByDescending` | 多列排序                    | 无              |
| `FirstOrDefault`         | 单条查询                    | ⚡ 避免全表扫描   |
| `Count`                  | 高效计数                    | ⚡ 只返回数字     |
| `Any/None`               | 存在性检查                   | ⚡ 只返回布尔值   |
| `Take/Skip`              | 灵活分页                    | ⚡ 减少数据传输   |

**优化完成率**：✅ 100%（核心查询功能完整，满足绝大多数业务场景）

---

## 🔮 未来可选增强（按需实现）

如果后续有需求，可以考虑：

1. **聚合函数**：`Sum()`, `Max()`, `Min()`, `Average()`
2. **分组查询**：`GroupBy()` + `Having()`
3. **JOIN 支持**：`LeftJoin()`, `InnerJoin()`（较复杂）
4. **批量操作**：`DeleteBatch()`, `UpdateBatch()`
5. **异步版本**：`CountAsync()`, `FirstOrDefaultAsync()` 等

**目前状态**：核心功能已完整，建议先在业务中验证，按需扩展。
