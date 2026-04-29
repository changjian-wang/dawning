# Changelog — Dawning.ORM.Dapper

All notable changes to this package are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

This release is a deep modernization of `Dawning.ORM.Dapper`. The package is now
verified end-to-end against six adapters with real databases (SQL Server,
SqlCe-mode, MySQL, PostgreSQL, SQLite, Firebird), and several latent bugs have
been fixed along the way.

### Added

- **`QueryBuilder<TEntity>` async aggregates** — `CountAsync()`, `AnyAsync()`,
  `NoneAsync()` now mirror the existing sync `Count() / Any() / None()`
  semantics over `await`. They honor the current `WHERE` chain and ignore
  `_distinct` / `_selectColumns`, exactly as the sync versions do.
- **Projection + cursor regression suite** — three new integration cases per
  adapter exercise `Skip/Take + Select` (sync and async), and cursor
  pagination over a `[Column]`-renamed property. These lock in the
  projection / cursor fixes below.
- **Multi-adapter integration test matrix** — full ORM API coverage on:
  - SQLite (`Microsoft.Data.Sqlite 8.0.10`, in-memory)
  - MySQL (`MySqlConnector 2.4.0`, `mysql:8` container)
  - PostgreSQL (`Npgsql 8.0.5`, `postgres:16` container)
  - SQL Server + SqlCe-compat path (`Microsoft.Data.SqlClient 5.2.2`,
    `mssql/server:2022` container)
  - Firebird (`FirebirdSql.Data.FirebirdClient 10.3.1`,
    `jacobalberty/firebird:v4` container; connection strings must include
    `Pooling=false` to avoid xUnit re-instantiation lock contention).
  Total ORM test count: **252 passing** (237 prior + 15 new regression cases).

### Changed

- **`ISqlAdapter.Insert` / `ISqlAdapter.InsertAsync`** now accept
  `IDbTransaction?` instead of `IDbTransaction`. This is a non-breaking
  widening for callers, but custom adapter implementations should update
  their signatures to match.
- **`FbAdapter.Insert` / `InsertAsync`** now use Firebird's
  `INSERT … RETURNING` clause to round-trip auto-generated keys. The
  previous implementation issued a separate
  `SELECT FIRST 1 {key} FROM {table} ORDER BY {key} DESC` — race-prone under
  concurrency (a different transaction's row could be returned) and broken
  for entities whose key carries a `[Column("…")]` rename. The new path is
  race-safe and honors `[Column]` for the key column.
- **Internal helpers** `GetMemberName(Expression?)` and
  `GetValueFromExpression(Expression?)` now accept and validate nullable
  expressions (`ArgumentNullException` for the former, returns `null` for the
  latter) so callers no longer need to pre-guard `Expression?` references.
- **`QueryBuilder._sqlAdapter`** is now `private readonly` and follows the
  project's `_camelCase` field convention. Constructor uses a direct
  assignment (the old `??=` was redundant on a freshly-constructed
  reference type field).
- Codebase compiles cleanly with full nullable analysis enabled —
  the previous `<NoWarn>$(NoWarn);CS8600;CS8602;CS8603;CS8604;CS8625</NoWarn>`
  suppression block has been removed from `Dawning.ORM.Dapper.csproj`.

### Fixed

Latent correctness bugs surfaced by the new integration matrix and the
post-Phase-5 deep audit, fixed in this release:

#### Cross-adapter

- **`QueryBuilder.Where(... !x.Member ...)` and `Where(... !(expr) ...)`
  no longer silently return the entire table.** `Visit`'s
  `ExpressionType.Not` branch only matched `!list.Contains(member)`. Any
  other shape (a bool member negation, a binary negation, a Convert
  wrap) fell through with no SQL emitted, leaving the `WHERE` clause
  empty (`1=1`). `!member` (bool / bool?) now emits `{col} = 0`,
  arbitrary `!(expr)` emits `NOT (...)`, and unhandled shapes throw
  `NotSupportedException` instead of silently widening the result set.
- **`MapRow.ConvertScalar` now short-circuits when the provider already
  returned the target CLR type.** The previous fallback was
  `Convert.ChangeType`, which requires `IConvertible` — `byte[]`,
  `DateTimeOffset`, `TimeSpan`, and any custom non-IConvertible type
  threw at materialization time. Also added explicit branches for
  `DateTimeOffset` and `TimeSpan` (parse-from-string fallback) and
  string-based enum parsing.
- **`SqlMapperExtensions.TableNameMapper` swap now invalidates the
  table-name cache.** `GetTableName` cached the resolved name keyed
  only by `RuntimeTypeHandle`, so reassigning the global mapper at
  runtime (multi-tenant prefixing, test isolation) silently kept
  returning stale names. Cache entries now record which mapper
  produced them and are invalidated when the mapper reference
  changes.
- **`QueryBuilder.FirstOrDefault[Async]` no longer ignores per-adapter
  row-limit injection when a `Select(...)` projection is used.** The previous
  implementation called `sql.Replace("SELECT *", "SELECT TOP 1 *")` (SQL
  Server / SqlCe) and `sql.Replace("SELECT *", "SELECT FIRST 1 *")`
  (Firebird), but once the leading `*` was replaced by a projected column
  list the `Replace` was a no-op — so the entire matching row set was
  streamed back to the client and `.FirstOrDefault()` was applied
  client-side. The new helper `InsertSelectModifier` injects the modifier
  immediately after the leading `SELECT [DISTINCT]` regardless of the
  projection.
- **`QueryBuilder.Skip(...).Take(...)` on Firebird now applies the row
  limit when combined with `Select(...)` projection.** Same root cause as
  above: the Firebird branch of `ApplySkipTake` rewrote the leading `*` to
  `FIRST n SKIP m *`. Fixed to use `InsertSelectModifier`. Other adapters
  (which append `LIMIT/OFFSET` to the SQL tail) were unaffected.
- **`QueryBuilder.AsPagedListByCursorAsync` now extracts `NextCursor`
  correctly when the order-by property carries `[Column("…")]`.** The
  cursor-property lookup used `type.GetProperty(cursorColumn)` where
  `cursorColumn` is the resolved SQL column name. For renamed properties
  (e.g. `[Column("score_value")] public int Score`) the lookup returned
  `null`, leaving `NextCursor` at `null` and silently restarting the next
  page from the beginning. Now matches by either CLR property name or
  `[Column]` attribute value.
- **Sync `Insert<T>` no longer leaks the connection on exception.** The
  `if (wasClosed) connection.Close()` reset is now in a `finally` block,
  matching the existing `InsertAsync` shape.
- **`PostgresAdapter.Insert / InsertAsync` no longer NREs when
  `RETURNING` produces zero rows.** Previously `results[0]` was indexed
  unconditionally; now the empty case returns `0` (matches the other
  adapters' "no key yielded" semantics). This is defensive — under normal
  schemas `RETURNING` always yields one row per inserted row, but a
  user-defined `INSTEAD OF` trigger can suppress it.
- **`SqlServerAdapter.Insert[Async]` and `SQLiteAdapter.Insert[Async]` no
  longer leak the underlying `DataReader` returned by Dapper's
  `QueryMultiple[Async]`.** Both adapters use `QueryMultiple` to combine
  `INSERT … ; SELECT SCOPE_IDENTITY()` (SQL Server) or
  `INSERT … ; SELECT last_insert_rowid()` (SQLite) into a single round-trip,
  but the resulting `SqlMapper.GridReader` (which owns a live `IDataReader`)
  was assigned to a plain local and never disposed. Under sustained insert
  load this leaked one reader per insert until the pool's command/cursor
  budget was exhausted. All four call sites now wrap the reader in
  `using var multi = …` (`GridReader` is `IDisposable`, not
  `IAsyncDisposable`, so sync `using` is the correct shape on both
  the sync and the async paths).
- **Async `Insert<T>` opens the connection asynchronously when possible.**
  The async entry path called `connection.Open()` (sync) when the caller
  passed a closed `IDbConnection`, blocking the calling thread on TCP /
  TLS handshake. It now prefers `DbConnection.OpenAsync()` when the
  connection derives from `System.Data.Common.DbConnection` (true for
  every driver shipped in this matrix) and only falls back to sync
  `Open()` for pure `IDbConnection` shims that don't expose `OpenAsync`.

Plus the nine bugs documented earlier in this Unreleased entry from the
initial multi-adapter rollout (SQLite parity, PG quoting, etc.).

### Notes for Maintainers

- The Firebird container path requires `Pooling=false` in the connection
  string. With pooling enabled, xUnit re-instantiation across test classes
  triggers DDL lock contention inside Firebird and the suite hangs.
- The async aggregate parity tests (`QueryBuilder_CountAsync_AnyAsync_NoneAsync`)
  now run inside every adapter's integration test suite via
  `OrmIntegrationTestBase`.
- Table-name identifier quoting remains intentionally adapter-agnostic
  (raw `[Table("…")]` value passes through unquoted to the SQL). Adding
  per-adapter quoting would change behavior for case-folding databases
  (PostgreSQL, Firebird) where unquoted identifiers fold to lower / upper
  case respectively. Users who want case-preserved table names should
  embed the quotes themselves via `[Table("\"MyTable\"")]` or register a
  custom `TableNameMapper`.

---

## [1.2.0] — Previous baseline

Prior history was tracked outside this file. This CHANGELOG starts here.
