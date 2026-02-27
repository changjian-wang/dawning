---
description: "Create MySQL database table with GUID primary key, timestamp field, entity class, and repository. Trigger: 创建表, create table, 建表, database, 数据库, 实体, entity, 表结构"
---

# Create Database Skill

## 目标

创建符合项目规范的数据库表结构和对应的 C# 实体类、Repository。

## 触发条件

- **关键词**：创建表, create table, 建表, database, 数据库, 实体, entity, 表结构, migration, 迁移
- **文件模式**：`*.sql`, `*Entity.cs`, `*Repository.cs`
- **用户意图**：设计数据库表、创建实体类、创建 Repository

## 编排

- **前置**：无
- **后续**：`create-api`（表结构就绪后创建 API）

---

## 重要规范

### GUID 主键 + Timestamp（必须）

```sql
CREATE TABLE {table_name} (
    id              CHAR(36) PRIMARY KEY COMMENT '唯一标识',
    timestamp       BIGINT NOT NULL COMMENT '乐观锁时间戳',
    -- 业务字段 --
    name            VARCHAR(100) NOT NULL COMMENT '名称',
    is_active       TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    -- 审计字段 --
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at      DATETIME ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    deleted_at      DATETIME COMMENT '删除时间（软删除）',
    created_by      CHAR(36) COMMENT '创建者',
    updated_by      CHAR(36) COMMENT '更新者',

    INDEX idx_timestamp (timestamp),
    INDEX idx_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='{表注释}';
```

### 命名规范

| 类型 | 规范 | 示例 |
|------|------|------|
| 表名 | snake_case，复数 | `users`, `user_roles` |
| 字段名 | snake_case | `user_name`, `created_at` |
| 主键 | `id` | `id CHAR(36)` |
| 外键 | `{table}_id` | `user_id`, `tenant_id` |
| 布尔 | `is_` / `has_` 前缀 | `is_active`, `has_permission` |
| 日期时间 | `{action}_at` | `created_at`, `deleted_at` |
| 普通索引 | `idx_{column}` | `idx_username` |
| 唯一索引 | `uk_{column}` | `uk_email` |

## 创建流程

### 1. SQL 迁移脚本

### 2. 创建实体类

```csharp
namespace Dawning.Gateway.Domain.Entities;

public class {EntityName} : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
```

### 3. 创建 Repository

```csharp
public interface I{EntityName}Repository
{
    Task<{EntityName}?> GetByIdAsync(Guid id);
    Task<IEnumerable<{EntityName}>> GetAllAsync();
    Task<Guid> CreateAsync({EntityName} entity);
    Task UpdateAsync({EntityName} entity);
    Task DeleteAsync(Guid id);
}
```

## 特殊字段处理

| 字段类型 | SQL | 说明 |
|---------|-----|------|
| 软删除 | `deleted_at DATETIME` | 查询时 `WHERE deleted_at IS NULL` |
| 多租户 | `tenant_id CHAR(36) NOT NULL` | 加索引 |
| JSON | `metadata JSON` | 扩展元数据 |
| 排序 | `sort_order INT DEFAULT 0` | 列表排序 |

## 验收场景

- **输入**："创建一个 products 表"
- **预期**：agent 生成带 GUID+timestamp 的 SQL、实体类、Repository 接口
- **上次验证**：2026-02-27 ✅
