# Dawning.ORM.Dapper

Dapper CRUD 扩展库。

## 安装

```bash
dotnet add package Dawning.ORM.Dapper
```

## 功能

- **CRUD 扩展** - Insert, Get, Update, Delete
- **异步支持** - 所有方法都有异步版本
- **特性映射** - `[Table]`, `[Key]`, `[Column]`

## 使用

### 定义实体

```csharp
using Dawning.ORM.Dapper;

[Table("users")]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Column("user_name")]
    public string Name { get; set; }
    
    public string Email { get; set; }
}
```

### CRUD 操作

```csharp
using var connection = new NpgsqlConnection(connectionString);

// 插入
var id = await connection.InsertAsync(new User 
{ 
    Name = "张三", 
    Email = "test@example.com" 
});

// 查询单个
var user = await connection.GetAsync<User>(id);

// 查询全部
var allUsers = await connection.GetAllAsync<User>();

// 更新
user.Name = "李四";
await connection.UpdateAsync(user);

// 删除
await connection.DeleteAsync(user);
// 或
await connection.DeleteAsync<User>(id);
```

## 支持的数据库

- PostgreSQL
- MySQL
- SQL Server
- SQLite
