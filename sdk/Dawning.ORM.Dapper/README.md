# Dawning.ORM.Dapper

A Dapper.Contrib-style CRUD extension library for .NET. Adds attribute-driven
table mapping, type-safe CRUD helpers, and a fluent `QueryBuilder<T>` for
filtering, ordering, projection, paging, and aggregates — with first-class
async support across six database adapters.

[![NuGet](https://img.shields.io/nuget/v/Dawning.ORM.Dapper.svg)](https://www.nuget.org/packages/Dawning.ORM.Dapper/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

- **Target framework**: `net9.0`
- **Dependencies**: [Dapper](https://www.nuget.org/packages/Dapper/) `2.1.66`
- **License**: MIT

---

## Installation

```bash
dotnet add package Dawning.ORM.Dapper
```

```xml
<ItemGroup>
  <PackageReference Include="Dawning.ORM.Dapper" Version="1.3.*" />
</ItemGroup>
```

You also need a concrete ADO.NET provider for your database, e.g.
`Microsoft.Data.Sqlite`, `Npgsql`, `MySqlConnector`, `Microsoft.Data.SqlClient`,
or `FirebirdSql.Data.FirebirdClient`.

---

## Supported databases

The library auto-detects an `ISqlAdapter` based on the runtime type name of
your `IDbConnection`:

| Adapter            | Connection class (lower-cased name) | Notes                                                  |
| ------------------ | ----------------------------------- | ------------------------------------------------------ |
| SQL Server         | `SqlConnection`                     | `Microsoft.Data.SqlClient` or `System.Data.SqlClient`  |
| SQL Server Compact | `SqlCeConnection`                   | Legacy desktop scenarios                               |
| PostgreSQL         | `NpgsqlConnection`                  | `Npgsql`; case-preserving identifier quoting           |
| SQLite             | `SqliteConnection`                  | `Microsoft.Data.Sqlite` (also `System.Data.SQLite`)    |
| MySQL              | `MySqlConnection`                   | `MySqlConnector` or `MySql.Data`                       |
| Firebird           | `FbConnection`                      | `FirebirdSql.Data.FirebirdClient`; use `Pooling=false` |

If your provider exposes a non-standard connection class name, override
detection via [`SqlMapperExtensions.GetDatabaseType`](#customization).

---

## Quick start

```csharp
using System.Data;
using Dawning.ORM.Dapper;
using Microsoft.Data.Sqlite;

[Table("users")]
public class User
{
    [Key]                       // auto-increment identity
    public int Id { get; set; }

    [Column("user_name")]
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

await using var connection = new SqliteConnection("Data Source=app.db");
await connection.OpenAsync();

// CREATE
var newId = await connection.InsertAsync(new User { Name = "Alice", Email = "a@x.com" });

// READ — single by primary key (prefer the QueryBuilder path; see "Reading by id")
var user = await connection
    .Builder<User>()
    .Where(u => u.Id == newId)
    .FirstOrDefaultAsync();

// READ — all rows
var all = await connection.GetAllAsync<User>();

// UPDATE
user!.Email = "alice@x.com";
await connection.UpdateAsync(user);

// DELETE
await connection.DeleteAsync(user);
```

---

## Mapping attributes

Place these on your POCO ("PO" / persistence entity). Domain entities should
not carry them; keep mapping concerns in a dedicated persistence type.

| Attribute                | Target   | Purpose                                                                         |
| ------------------------ | -------- | ------------------------------------------------------------------------------- |
| `[Table("name")]`        | class    | Override the table name (default: type name).                                   |
| `[Key]`                  | property | Marks an **auto-generated** primary key (e.g. `IDENTITY`, `SERIAL`, `ROWID`).   |
| `[ExplicitKey]`          | property | Marks a primary key whose value the **caller** supplies (e.g. `Guid`, `string`).|
| `[Column("col_name")]`   | property | Override the column name (default: property name).                              |
| `[Write(false)]`         | property | Property is read from `SELECT` but never inserted/updated.                      |
| `[Computed]`             | property | Same as `[Write(false)]`; intent is "computed in DB".                           |
| `[IgnoreUpdate]`         | property | Property is inserted but never updated.                                         |
| `[DefaultSortName]`      | property | Marks the column used as a default `ORDER BY` for paged queries.                |

Notes:

- `[Key]` and `[ExplicitKey]` are mutually exclusive on a type; exactly one
  property must carry one of them for `Get` / `Update` / `Delete` to work.
- `[Write(false)]` and `[Computed]` only suppress writes. Values still flow
  back from `SELECT *` into the property.
- Column lookup on read is **case-insensitive** so providers that fold
  identifier case (Firebird → uppercase, PostgreSQL → lowercase for unquoted)
  still resolve correctly.

### Example with all attributes

```csharp
[Table("orders")]
public class Order
{
    [ExplicitKey]
    [Column("id")]
    public string Id { get; set; } = string.Empty;     // app-supplied GUID/ULID

    [Column("customer_name")]
    [DefaultSortName]
    public string CustomerName { get; set; } = string.Empty;

    [IgnoreUpdate]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }            // set on insert, never updated

    [Computed]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }           // computed by DB; read-only here

    [Write(false)]
    public string DisplayName => $"{CustomerName} #{Id}";
}
```

---

## CRUD extension methods

All extensions live on `IDbConnection` and have both sync and async variants.

### `Insert<T>` / `InsertAsync<T>`

```csharp
public static long Insert<T>(this IDbConnection conn, T entityToInsert,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

public static Task<long> InsertAsync<T>(this IDbConnection conn, T entityToInsert,
    IDbTransaction? transaction = null, int? commandTimeout = null,
    ISqlAdapter? sqlAdapter = null) where T : class;
```

- For `[Key]` (auto-generated) PK: returns the new identity (or `0` for batch
  inserts — see below).
- For `[ExplicitKey]` PK: returns `1` on success.
- Accepts a single entity **or** an `IEnumerable<T>` for batch insert.
- For batch inserts of `[Key]`-typed entities, identity values are not
  back-filled (Dapper.Contrib limitation); use a `Builder` query afterwards if
  you need them.

```csharp
var id = await connection.InsertAsync(new User { Name = "Alice" });
var rows = await connection.InsertAsync(new[] { new User { Name = "B" }, new User { Name = "C" } });
```

### `Get<T>` / `GetAsync<T>`

```csharp
public static T? Get<T>(this IDbConnection conn, dynamic id,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new();

public static Task<T?> GetAsync<T>(this IDbConnection conn, dynamic id,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new();
```

Returns one entity by primary key, or `null` when no row matches.

> **Recommended alternative for new code**: prefer
> `connection.Builder<T>().Where(x => x.Id == id).FirstOrDefaultAsync()`. The
> `Get` / `GetAsync` overloads accept `dynamic id`, which forces the call site
> to bind through the C# runtime binder. The `QueryBuilder` path is fully
> static and does not.

### `GetAll<T>` / `GetAllAsync<T>`

```csharp
public static IEnumerable<T> GetAll<T>(this IDbConnection conn,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new();

public static Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection conn,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class, new();
```

Returns every row from the table. Use `Builder<T>()` instead when you need
filtering, ordering, projection, or paging.

### `Update<T>` / `UpdateAsync<T>`

```csharp
public static bool Update<T>(this IDbConnection conn, T entityToUpdate,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

public static Task<bool> UpdateAsync<T>(this IDbConnection conn, T entityToUpdate,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;
```

- Updates by primary key.
- Skips properties marked `[Computed]`, `[Write(false)]`, or `[IgnoreUpdate]`.
- Accepts a single entity or `IEnumerable<T>` for batch update.
- Returns `true` when at least one row was affected.

### `Delete<T>` / `DeleteAsync<T>`

```csharp
public static bool Delete<T>(this IDbConnection conn, T entityToDelete,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

public static Task<bool> DeleteAsync<T>(this IDbConnection conn, T entityToDelete,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;
```

Deletes the row(s) matching the primary key(s). Accepts `IEnumerable<T>` for
batch delete. Returns `true` when at least one row was affected.

### `DeleteAll<T>` / `DeleteAllAsync<T>`

```csharp
public static bool DeleteAll<T>(this IDbConnection conn,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

public static Task<bool> DeleteAllAsync<T>(this IDbConnection conn,
    IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;
```

Removes every row from the table for `T`. **Does not** drop the table.

---

## `QueryBuilder<T>` — fluent queries

Open a builder via:

```csharp
public static QueryBuilder<TEntity> Builder<TEntity>(this IDbConnection conn,
    IDbTransaction? transaction = null, int? commandTimeout = null)
    where TEntity : class, new();
```

The builder produces parameterized SQL through the active `ISqlAdapter` and
materializes rows through the same column-aware mapper used by `Get` / `GetAll`.

### Filtering — `Where`, `WhereIf`

```csharp
QueryBuilder<T> Where(Expression<Func<T, bool>> predicate);
QueryBuilder<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate);
```

- `Where` calls compose with `AND`.
- `WhereIf(false, ...)` is a no-op — useful for optional filters.

```csharp
var query = connection.Builder<User>()
    .Where(u => u.IsActive)
    .WhereIf(!string.IsNullOrEmpty(keyword), u => u.Name.Contains(keyword))
    .Where(u => u.CreatedAt >= since);
```

Supported expression patterns include:

- Comparison: `==`, `!=`, `<`, `<=`, `>`, `>=`
- Boolean composition: `&&`, `||`, `!`
- `string.Contains` / `StartsWith` / `EndsWith` (translated to `LIKE`)
- `Enumerable.Contains` (translated to `IN (...)`)
- `value == null` / `value != null` (translated to `IS NULL` / `IS NOT NULL`)

### Ordering — `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`

```csharp
QueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> key);
QueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> key);
QueryBuilder<T> ThenBy<TKey>(Expression<Func<T, TKey>> key);
QueryBuilder<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> key);
```

`OrderBy` / `OrderByDescending` resets any prior order. Use `ThenBy*` to chain
secondary sort keys.

### Pagination — `Skip`, `Take`, `AsPagedList[Async]`, `AsPagedListByCursorAsync`

```csharp
QueryBuilder<T> Skip(int count);
QueryBuilder<T> Take(int count);

PagedResult<T>            AsPagedList(int page, int itemsPerPage);
Task<PagedResult<T>>      AsPagedListAsync(int page, int itemsPerPage);
Task<PagedResult<T>>      AsPagedListAsync(int page, int itemsPerPage, PagedOptions options);

Task<CursorPagedResult<T>> AsPagedListByCursorAsync(int itemsPerPage, object? lastCursorValue = null);
Task<CursorPagedResult<T>> AsPagedListByCursorAsync(int itemsPerPage, object? lastCursorValue, PagedOptions options);
```

Two strategies:

| Strategy | API                          | Use when                                                     |
| -------- | ---------------------------- | ------------------------------------------------------------ |
| Offset   | `AsPagedList[Async]`         | Page jumping; small or moderate datasets.                    |
| Cursor   | `AsPagedListByCursorAsync`   | Stable, deep, infinite-scroll lists; very large datasets.    |

Cursor pagination requires a stable, monotonic key. Mark it with
`[DefaultSortName]` (or set the order chain explicitly) so the builder knows
which column to keyset on.

```csharp
public class Event
{
    [Key] public long Id { get; set; }
    [DefaultSortName] [Column("ts")] public long Timestamp { get; set; }
}

var first  = await connection.Builder<Event>().AsPagedListByCursorAsync(100);
var second = await connection.Builder<Event>().AsPagedListByCursorAsync(100, first.NextCursor);
```

Use `PagedOptions` to tune defaults and upper bounds. Built-in defaults:

| Property            | Default                        | Purpose                                                      |
| ------------------- | ------------------------------ | ------------------------------------------------------------ |
| `DefaultPageSize`   | `10`                           | Fallback page size when caller passes a non-positive value.  |
| `MaxPageNumber`     | `10000`                        | Hard cap on `page` for offset pagination.                    |
| `MaxCursorPageSize` | `1000`                         | Hard cap on `itemsPerPage` for cursor pagination.            |
| `Strategy`          | `PaginationStrategy.Offset`    | Default strategy when no explicit cursor API is called.      |

A pre-built singleton is available as `PagedOptions.Default`. Customize only
the knobs you care about:

```csharp
var options = new PagedOptions
{
    DefaultPageSize   = 25,    // override default 10
    MaxPageNumber     = 5_000, // override default 10000 (tighter cap)
    MaxCursorPageSize = 500,   // override default 1000  (tighter cap)
    Strategy          = PaginationStrategy.Offset,
};

var page = await connection.Builder<User>()
    .OrderBy(u => u.Id)
    .AsPagedListAsync(page: 3, itemsPerPage: 50, options);
```

`PagedResult<T>` shape:

```csharp
public class PagedResult<T>
{
    public IEnumerable<T> Values { get; set; }
    public int Page { get; set; }
    public int ItemsPerPage { get; set; }
    public long TotalItems { get; set; }
    public int TotalPages { get; set; }
}
```

`CursorPagedResult<T>` shape:

```csharp
public class CursorPagedResult<T>
{
    public IEnumerable<T> Values { get; set; }
    public int ItemsPerPage { get; set; }
    public bool HasMore { get; set; }
    public object? NextCursor { get; set; }
}
```

### Projection — `Select`

```csharp
QueryBuilder<T> Select<TResult>(Expression<Func<T, TResult>> selector);
QueryBuilder<T> Select(params string[] columnNames);
```

Restricts the columns emitted by the generated `SELECT`. The result rows still
materialize as `T`; non-selected properties simply remain at their default.

```csharp
var lite = await connection.Builder<User>()
    .Select(u => new { u.Id, u.Name })
    .ToListAsync();
```

`Select` does not change the runtime row type — it only narrows the wire data.
For an actual projection type, use a separate POCO and call Dapper's
`QueryAsync<TDto>(...)` directly.

### Distinct — `Distinct`

```csharp
QueryBuilder<T> Distinct();
```

Adds `DISTINCT` to the generated `SELECT`. Combine with `Select(...)` to make
distinctness meaningful.

### Materialization

```csharp
TEntity?               FirstOrDefault();          // sync, single row
Task<TEntity?>         FirstOrDefaultAsync();     // async, single row
IEnumerable<TEntity>   AsList();                  // sync, all matched rows
Task<IEnumerable<T>>   AsListAsync();             // async, all matched rows
Task<List<TEntity>>    ToListAsync();             // async, allocates List<T>
```

### Aggregates — `Count`, `Any`, `None`

```csharp
long  Count();        Task<long> CountAsync();
bool  Any();          Task<bool> AnyAsync();
bool  None();         Task<bool> NoneAsync();
```

Aggregates honor the current `WHERE` chain but ignore `Select(...)` /
`Distinct()` / paging — they always run as `SELECT COUNT(*) FROM ... WHERE ...`.

### Transactions and timeouts

Every static extension and `Builder<T>(...)` accepts `IDbTransaction?` and
`int? commandTimeout`. Pass them through when participating in an existing
transaction:

```csharp
using var tx = connection.BeginTransaction();
var id = await connection.InsertAsync(user, tx);
await connection.Builder<User>(tx).Where(u => u.Id == id).FirstOrDefaultAsync();
tx.Commit();
```

---

## Reading by id — recommended pattern

Prefer the `QueryBuilder` predicate path over `Get` / `GetAsync`:

```csharp
// ✅ Recommended — fully static, no dynamic CallSite
var idValue = id.ToString();
var user = await connection
    .Builder<User>()
    .Where(u => u.Id == idValue)
    .FirstOrDefaultAsync();

// ⚠️ Works, but routes the id through `dynamic`
var user2 = await connection.GetAsync<User>(id.ToString());
```

`Get` / `GetAsync` accept `dynamic id` so they can serve any PK type without
overload explosion. That convenience comes at the cost of a runtime call site.
The builder path is type-safe end-to-end and produces equivalent SQL.

---

## Customization

### Custom table name resolution

Set `SqlMapperExtensions.TableNameMapper` to a delegate **once at startup**:

```csharp
SqlMapperExtensions.TableNameMapper = type =>
{
    // e.g. pluralize or apply a tenant prefix
    return $"app_{type.Name.ToLowerInvariant()}s";
};
```

A non-null mapper always wins over `[Table("...")]`. The internal table-name
cache snapshots the current delegate, so swapping the delegate at runtime
correctly invalidates cached entries.

### Custom database-type detection

For ADO.NET providers whose connection class name does not match a built-in
adapter key (e.g. proxied or wrapped connections):

```csharp
SqlMapperExtensions.GetDatabaseType = conn => conn switch
{
    MyWrappedNpgsql      => "npgsqlconnection",
    MyWrappedSqlite      => "sqliteconnection",
    _                    => conn.GetType().Name,
};
```

Return one of the lower-cased keys listed in
[Supported databases](#supported-databases).

---

## Common patterns

### Repository over `IDbConnection`

```csharp
public sealed class UserRepository(IDbConnectionFactory factory)
{
    public async Task<User?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = await factory.OpenAsync(ct);
        return await conn
            .Builder<User>()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<User>> ListActiveAsync(int page, int pageSize, CancellationToken ct)
    {
        await using var conn = await factory.OpenAsync(ct);
        var paged = await conn
            .Builder<User>()
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .AsPagedListAsync(page, pageSize);

        return paged.Values.ToList();
    }

    public async Task<long> CountActiveAsync(CancellationToken ct)
    {
        await using var conn = await factory.OpenAsync(ct);
        return await conn.Builder<User>().Where(u => u.IsActive).CountAsync();
    }
}
```

### Persistence entity (PO) separate from domain entity

```csharp
// Domain — pure, no ORM attributes
public sealed class InboxItem
{
    public Guid Id { get; }
    public string Content { get; private set; }
    public string? Source { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; }
    public DateTimeOffset CreatedAt { get; }

    // ... behavior + Rehydrate factory ...
}

// Persistence — attribute-mapped PO, lives in Infrastructure
[Table("inbox_items")]
internal sealed class InboxItemEntity
{
    [ExplicitKey] [Column("id")]            public string Id { get; set; } = string.Empty;
    [Column("content")]                     public string Content { get; set; } = string.Empty;
    [Column("source")]                      public string? Source { get; set; }
    [Column("captured_at_utc")]             public string CapturedAtUtc { get; set; } = string.Empty; // ISO-8601 round-trip
    [Column("created_at_utc")]              public string CreatedAtUtc { get; set; } = string.Empty;
}
```

This keeps ORM concerns out of the domain layer and lets you evolve the
persistence schema without touching aggregate invariants.

---

## Troubleshooting

**`DataException: Get<T> only supports an entity with a [Key] or an [ExplicitKey] property`**
The PK property is missing or unmapped. Add `[Key]` (auto-generated) or
`[ExplicitKey]` (caller-supplied), and ensure `[Column("...")]` matches the
actual column.

**`Microsoft.CSharp.RuntimeBinder.RuntimeBinderException` from `GetAsync`**
Use the [recommended `Builder.Where(...).FirstOrDefaultAsync()` pattern](#reading-by-id--recommended-pattern)
instead. This is fixed in `1.3.1`+, but the builder path is still the
preferred shape because it is statically typed.

**Identifier-case mismatch (PostgreSQL / Firebird)**
Column lookup is case-insensitive on the materialization side, and the
adapters quote identifiers as needed when emitting `SELECT`. If you still see
a mismatch, double-check that `[Column("...")]` matches the database
spelling exactly.

**Firebird connection pool deadlocks under xUnit**
Append `Pooling=false` to the connection string when each test rebuilds the
schema. Pooled Firebird connections retain server-side state across test
fixtures and can deadlock the embedded server.

---

## Versioning

This package follows [Semantic Versioning](https://semver.org/). See
[`CHANGELOG.md`](CHANGELOG.md) for the full history.

---

## License

MIT. See [`LICENSE`](../../LICENSE).
