# Phase 3: 分页策略配置 - 实现总结

## 📋 概述

Phase 3 为Dapper分页系统添加了灵活的配置支持，允许开发者根据不同场景自定义分页行为，同时保持向后兼容性。

---

## ✨ 核心功能

### 1. PagedOptions 配置类

**位置**: `SqlMapperExtensions.cs` (Dapper层)

```csharp
public class PagedOptions
{
    /// <summary>
    /// 最大允许页码（默认：10000）
    /// 防止恶意深度分页请求
    /// </summary>
    public int MaxPageNumber { get; set; } = 10000;

    /// <summary>
    /// 游标分页最大页大小（默认：1000）
    /// </summary>
    public int MaxCursorPageSize { get; set; } = 1000;

    /// <summary>
    /// 默认每页数量（默认：10）
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// 启用并行COUNT查询（默认：false）
    /// 仅支持MARS（Multiple Active Result Sets）的数据库如SQL Server
    /// MySQL不支持MARS，使用顺序执行
    /// </summary>
    public bool EnableParallelCount { get; set; } = false;

    /// <summary>
    /// 启用延迟JOIN优化（默认：false）
    /// 对深度分页使用覆盖索引扫描 + 后续表JOIN
    /// </summary>
    public bool EnableDelayedJoin { get; set; } = false;

    /// <summary>
    /// 分页策略（默认：Offset）
    /// </summary>
    public PaginationStrategy Strategy { get; set; } = PaginationStrategy.Offset;

    /// <summary>
    /// 全局默认配置单例
    /// </summary>
    public static PagedOptions Default { get; } = new PagedOptions();
}
```

### 2. PaginationStrategy 枚举

```csharp
public enum PaginationStrategy
{
    /// <summary>
    /// 传统OFFSET/LIMIT分页
    /// - 优点：支持任意页跳转、有总数统计
    /// - 缺点：深度分页性能差（O(n)）
    /// </summary>
    Offset = 0,

    /// <summary>
    /// 游标（Keyset）分页
    /// - 优点：稳定性能（O(1)）、适合大数据集
    /// - 缺点：不支持任意页跳转、无总数统计
    /// </summary>
    Cursor = 1,

    /// <summary>
    /// 自动选择策略
    /// - 根据页深度自动切换：浅页使用Offset，深页使用Cursor
    /// - 示例：page <= 100 用Offset，page > 100 用Cursor
    /// </summary>
    Auto = 2
}
```

### 3. PaginationOptions (Domain层)

**位置**: `Dawning.Identity.Domain.Models.PageData.cs`

```csharp
/// <summary>
/// 分页配置选项（Domain层定义，避免依赖基础设施层）
/// </summary>
public class PaginationOptions
{
    public int MaxPageNumber { get; set; } = 10000;
    public int MaxCursorPageSize { get; set; } = 1000;
    public int DefaultPageSize { get; set; } = 10;
}
```

---

## 🔧 API更新

### AsPagedListAsync (OFFSET分页)

**新增重载方法**:

```csharp
// 原方法（向后兼容）
public async Task<PagedResult<TEntity>> AsPagedListAsync(int page, int itemsPerPage)
{
    return await AsPagedListAsync(page, itemsPerPage, PagedOptions.Default);
}

// 新方法（配置支持）
public async Task<PagedResult<TEntity>> AsPagedListAsync(int page, int itemsPerPage, PagedOptions options)
{
    if (page < 1) page = 1;
    if (itemsPerPage < 1) itemsPerPage = options.DefaultPageSize;

    var maxPage = options?.MaxPageNumber ?? MaxPageNumber;

    // 页码保护
    if (page > maxPage)
    {
        throw new InvalidOperationException(
            $"Page number {page} exceeds maximum allowed {maxPage}. " +
            "Consider using filters to narrow down results or contact support for large dataset access.");
    }

    // ... 分页查询逻辑
}
```

### AsPagedListByCursorAsync (Cursor分页)

**新增重载方法**:

```csharp
// 原方法（向后兼容）
public async Task<CursorPagedResult<TEntity>> AsPagedListByCursorAsync(
    int itemsPerPage, 
    object? lastCursorValue = null, 
    bool ascending = false)
{
    return await AsPagedListByCursorAsync(itemsPerPage, lastCursorValue, ascending, PagedOptions.Default);
}

// 新方法（配置支持）
public async Task<CursorPagedResult<TEntity>> AsPagedListByCursorAsync(
    int itemsPerPage, 
    object? lastCursorValue, 
    bool ascending, 
    PagedOptions options)
{
    if (itemsPerPage < 1) itemsPerPage = options.DefaultPageSize;

    var maxPageSize = options?.MaxCursorPageSize ?? 1000;
    if (itemsPerPage > maxPageSize)
    {
        throw new InvalidOperationException(
            $"Page size {itemsPerPage} exceeds maximum allowed {maxPageSize} for cursor pagination.");
    }

    // ... 游标分页查询逻辑
}
```

---

## 📚 使用示例

### 示例 1: 调整最大页数限制

```csharp
// 场景：内部管理系统允许更深的分页
var options = new PaginationOptions 
{ 
    MaxPageNumber = 50000  // 允许访问5万页
};

var result = await userRepository.GetPagedListWithOptionsAsync(page, pageSize, options);
```

### 示例 2: 增大游标分页大小

```csharp
// 场景：批量导出需要更大的页大小
var options = new PaginationOptions 
{ 
    MaxCursorPageSize = 5000  // 允许每页5000条
};

var result = await repository.GetPagedListByCursorAsync(5000, lastCursor, false, options);
```

### 示例 3: 自定义默认页大小

```csharp
// 场景：移动端API使用较小的默认页大小
var options = new PaginationOptions 
{ 
    DefaultPageSize = 5  // 默认每页5条
};

var result = await repository.GetPagedListWithOptionsAsync(page, 0, options);
// itemsPerPage=0 时使用 DefaultPageSize
```

### 示例 4: 全局配置

```csharp
// 应用启动时配置全局默认值
PagedOptions.Default.MaxPageNumber = 20000;
PagedOptions.Default.DefaultPageSize = 20;

// 所有未指定配置的分页调用都会使用这些默认值
```

---

## 🏗️ 架构实现

### 分层设计

```
┌─────────────────────────────────────────────┐
│ Presentation Layer (API Controllers)       │
│  - UserController                           │
│  - Uses PaginationOptions (Domain)         │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Application Layer (Services)               │
│  - UserService                              │
│  - Passes PaginationOptions through        │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Domain Layer (Interfaces + Models)         │
│  - IUserRepository                          │
│  - PaginationOptions (避免Dapper依赖)      │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Infrastructure Layer (Repositories)        │
│  - UserRepository                           │
│  - Maps PaginationOptions → PagedOptions   │
│  - Calls Dapper with PagedOptions          │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Dapper Layer (SqlMapperExtensions)         │
│  - PagedOptions (完整配置类)               │
│  - PaginationStrategy enum                  │
│  - AsPagedListAsync/ByCursorAsync          │
└─────────────────────────────────────────────┘
```

### 关键设计决策

1. **Domain层独立配置类**
   - `PaginationOptions` 在Domain层定义
   - 避免Domain/Application层依赖Dapper
   - 遵循DDD（领域驱动设计）原则

2. **Infrastructure层转换**
   - `UserRepository` 负责 `PaginationOptions` → `PagedOptions` 映射
   - 隔离技术细节，保持Domain纯净

3. **向后兼容**
   - 原有方法保持不变
   - 新方法提供可选配置参数
   - 默认行为与Phase 1/2一致

---

## 🧪 测试端点

### GET /api/user/custom-config

**功能**: 测试自定义分页配置

**请求参数**:
- `page` (int): 页码
- `pageSize` (int): 每页数量
- `maxPageNumber` (int, optional): 自定义最大页数
- `maxCursorPageSize` (int, optional): 自定义游标分页限制

**响应示例**:
```json
{
  "code": 0,
  "message": "Success (custom config applied)",
  "config": {
    "maxPageNumber": 5000,
    "maxCursorPageSize": 500,
    "defaultPageSize": 20
  },
  "data": {
    "list": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "username": "admin",
        "email": "admin@example.com"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 2,
      "total": 3
    }
  }
}
```

---

## ✅ 测试验证

运行测试脚本:
```powershell
# 1. 启动API
cd C:\github\dawning\Dawning.Gateway\src\Dawning.Identity.Api
dotnet run

# 2. 在新终端运行测试
cd C:\github\dawning\Dawning.Gateway
.\test-phase3-config.ps1
```

**预期结果**:
- ✅ 自定义MaxPageNumber生效（返回配置信息）
- ✅ 自定义MaxCursorPageSize生效
- ✅ 标准端点继续使用默认配置
- ✅ 向后兼容性验证通过

---

## 📊 对比总结

### Phase 1: OFFSET分页优化
- ✅ 简化双查询 → 单查询
- ✅ MaxPageNumber=10000 硬编码保护
- ✅ MySQL顺序执行COUNT + Data

### Phase 2: Cursor分页实现
- ✅ AsPagedListByCursorAsync方法
- ✅ O(1)性能，适合大数据集
- ✅ MaxCursorPageSize=1000 硬编码限制

### Phase 3: 配置层支持 (本阶段)
- ✅ PagedOptions配置类
- ✅ PaginationStrategy枚举
- ✅ Domain层PaginationOptions（DDD合规）
- ✅ 可配置MaxPageNumber/MaxCursorPageSize/DefaultPageSize
- ✅ 向后兼容（原方法保持不变）
- ✅ 支持全局配置（PagedOptions.Default）
- ✅ Repository/Service/Controller全栈支持

---

## 🎯 优势总结

1. **灵活性**: 不同场景可使用不同配置
2. **安全性**: 可按应用需求调整保护限制
3. **性能**: 支持并行COUNT、延迟JOIN（预留）
4. **兼容性**: 现有代码无需修改
5. **可维护性**: 集中配置管理
6. **可扩展性**: 预留Auto策略、延迟JOIN等高级特性

---

## 🚀 后续优化方向

### 1. EnableParallelCount实现
```csharp
// 对支持MARS的数据库（SQL Server）使用并行查询
if (options.EnableParallelCount && sqlAdapter.SupportsMars())
{
    var countTask = connection.ExecuteScalarAsync(countSql, parameters);
    var dataTask = sqlAdapter.RetrieveCurrentPaginatedDataAsync(...);
    await Task.WhenAll(countTask, dataTask);
}
```

### 2. EnableDelayedJoin实现
```csharp
// 对深度分页使用覆盖索引优化
if (options.EnableDelayedJoin && page > 100)
{
    // 步骤1: 使用覆盖索引获取ID列表
    SELECT id FROM users WHERE ... ORDER BY ... LIMIT 10 OFFSET 10000
    
    // 步骤2: IN子查询JOIN回完整数据
    SELECT * FROM users WHERE id IN (...)
}
```

### 3. Auto策略实现
```csharp
if (options.Strategy == PaginationStrategy.Auto)
{
    // 浅页使用OFFSET（支持跳转、有总数）
    if (page <= 100) return await AsPagedListAsync(...);
    
    // 深页使用Cursor（稳定性能）
    else return await AsPagedListByCursorAsync(...);
}
```

---

## 📝 Commit信息

```
feat: Add Phase 3 pagination configuration support

✨ Features:
- PagedOptions class for flexible pagination configuration
- PaginationStrategy enum (Offset/Cursor/Auto)
- PaginationOptions in Domain layer (DDD compliance)
- Configurable MaxPageNumber, MaxCursorPageSize, DefaultPageSize
- EnableParallelCount, EnableDelayedJoin flags (reserved)

🔧 Implementation:
- AsPagedListAsync/ByCursorAsync overloads with PagedOptions
- Repository/Service/Controller full-stack support
- GET /api/user/custom-config test endpoint

🎯 Benefits:
- Flexible per-use-case configuration
- Application-specific security limits
- Performance tuning for different scenarios
- Backward compatible
- DDD architecture compliance
```

---

## 🎉 Phase 3 完成！

**时间**: 2025-12-05  
**提交**: 5c4a384  
**状态**: ✅ 已完成  
**测试**: ✅ 编译通过，待运行时测试

**下一步**: 运行测试脚本验证所有功能
