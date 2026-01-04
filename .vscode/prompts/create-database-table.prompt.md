---
description: 创建数据库表和实体类
---

# 创建数据库表和实体类

创建符合项目规范的数据库表设计和对应的 C# 实体类。

## 重要规范

### GUID 主键 + Timestamp（必须）

当使用 GUID 作为主键时，**必须**添加 `timestamp` 字段：

```sql
-- ✅ 正确
CREATE TABLE {table_name} (
    id              CHAR(36) PRIMARY KEY,
    timestamp       BIGINT NOT NULL,               -- 必须！
    -- 业务字段 --
    name            VARCHAR(100) NOT NULL,
    description     TEXT,
    is_active       TINYINT(1) DEFAULT 1,          -- 布尔用 is_ 前缀
    -- 审计字段 --
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME ON UPDATE CURRENT_TIMESTAMP,
    deleted_at      DATETIME,                      -- 软删除
    created_by      CHAR(36),
    updated_by      CHAR(36),
    
    INDEX idx_timestamp (timestamp),               -- 必须！
    INDEX idx_name (name)
);

-- ❌ 错误：缺少 timestamp
CREATE TABLE {table_name} (
    id CHAR(36) PRIMARY KEY,
    name VARCHAR(100) NOT NULL
    -- 缺少 timestamp 字段和索引！
);
```

## 命名规范

| 类型 | 规范 | 示例 |
|------|------|------|
| 表名 | snake_case，复数 | `users`, `user_roles` |
| 字段名 | snake_case | `user_name`, `created_at` |
| 主键 | `id` | `id CHAR(36)` |
| 时间戳 | `timestamp` | `timestamp BIGINT` |
| 外键 | `{table}_id` | `user_id`, `tenant_id` |
| 布尔 | `is_` / `has_` 前缀 | `is_active`, `has_permission` |
| 日期时间 | `{action}_at` | `created_at`, `deleted_at` |
| 普通索引 | `idx_{column}` | `idx_username` |
| 唯一索引 | `uk_{column}` | `uk_email` |

## 创建流程

### 1. 设计表结构

根据业务需求设计字段，确保包含：
- `id` - GUID 主键
- `timestamp` - BIGINT 时间戳
- 业务字段
- 审计字段（created_at, updated_at, deleted_at）

### 2. 创建 SQL 迁移脚本

```sql
-- migrations/V{version}__{description}.sql

CREATE TABLE {table_name} (
    id              CHAR(36) PRIMARY KEY COMMENT '唯一标识',
    timestamp       BIGINT NOT NULL COMMENT '乐观锁时间戳',
    -- 业务字段
    code            VARCHAR(50) NOT NULL COMMENT '编码',
    name            VARCHAR(100) NOT NULL COMMENT '名称',
    -- 状态和配置
    is_enabled      TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    sort_order      INT DEFAULT 0 COMMENT '排序号',
    -- 审计字段
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at      DATETIME ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    deleted_at      DATETIME COMMENT '删除时间（软删除）',
    
    -- 索引
    UNIQUE INDEX uk_code (code),
    INDEX idx_timestamp (timestamp),
    INDEX idx_name (name),
    INDEX idx_is_enabled (is_enabled)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='{TableComment}';
```

### 3. 创建实体类

```csharp
namespace Dawning.Gateway.Domain.Entities;

/// <summary>
/// {EntityName} 实体
/// </summary>
public class {EntityName} : BaseEntity
{
    /// <summary>编码</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>排序号</summary>
    public int SortOrder { get; set; }
}
```

### 4. 创建 Repository

```csharp
public interface I{EntityName}Repository
{
    Task<{EntityName}?> GetByIdAsync(Guid id);
    Task<{EntityName}?> GetByCodeAsync(string code);
    Task<IEnumerable<{EntityName}>> GetAllAsync();
    Task<Guid> CreateAsync({EntityName} entity);
    Task UpdateAsync({EntityName} entity);
    Task DeleteAsync(Guid id);
}
```

## 特殊字段处理

### 软删除
```sql
deleted_at      DATETIME COMMENT '删除时间',

-- 查询时排除已删除记录
WHERE deleted_at IS NULL
```

### 多租户
```sql
tenant_id       CHAR(36) NOT NULL COMMENT '租户ID',
INDEX idx_tenant_id (tenant_id)
```

### JSON 字段
```sql
metadata        JSON COMMENT '扩展元数据',
config          JSON COMMENT '配置信息'
```

## 输出要求

1. SQL 建表语句（带注释）
2. C# 实体类（带 XML 文档注释）
3. Repository 接口
4. 必要的索引设计说明
