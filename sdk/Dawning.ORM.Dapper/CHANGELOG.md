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
- **Multi-adapter integration test matrix** — full ORM API coverage on:
  - SQLite (`Microsoft.Data.Sqlite 8.0.10`, in-memory)
  - MySQL (`MySqlConnector 2.4.0`, `mysql:8` container)
  - PostgreSQL (`Npgsql 8.0.5`, `postgres:16` container)
  - SQL Server + SqlCe-compat path (`Microsoft.Data.SqlClient 5.2.2`,
    `mssql/server:2022` container)
  - Firebird (`FirebirdSql.Data.FirebirdClient 10.3.1`,
    `jacobalberty/firebird:v4` container; connection strings must include
    `Pooling=false` to avoid xUnit re-instantiation lock contention).
  Total ORM test count: **237 passing**.

### Changed

- **`ISqlAdapter.Insert` / `ISqlAdapter.InsertAsync`** now accept
  `IDbTransaction?` instead of `IDbTransaction`. This is a non-breaking
  widening for callers, but custom adapter implementations should update
  their signatures to match.
- **Internal helpers** `GetMemberName(Expression?)` and
  `GetValueFromExpression(Expression?)` now accept and validate nullable
  expressions (`ArgumentNullException` for the former, returns `null` for the
  latter) so callers no longer need to pre-guard `Expression?` references.
- Codebase compiles cleanly with full nullable analysis enabled —
  the previous `<NoWarn>$(NoWarn);CS8600;CS8602;CS8603;CS8604;CS8625</NoWarn>`
  suppression block has been removed from `Dawning.ORM.Dapper.csproj`.

### Fixed

Nine latent correctness bugs surfaced by the new integration matrix and
fixed in this release:

#### SQLite
- `AsListAsync` no longer diverges from the sync `AsList` path when the row
  set is empty (parity restored).
- `MapRow` now passes `Write(false)` to Dapper's `MapRow`, preventing spurious
  property writes on read-only mappings.
- `ApplySkipTake` now emits `OFFSET` after `LIMIT` (SQLite requires both
  clauses; the previous shape silently dropped pagination on Skip-only
  queries).

#### PostgreSQL
- Quoted identifier handling in `INSERT` / `UPDATE` now matches PG's
  case-folding rules (no more lookup misses on mixed-case column names).
- `RETURNING` clauses now flow through the auto-key-fetch path on
  `InsertAsync`, restoring identity round-tripping for `bigint` primary keys.
- `ApplySkipTake` for PG now emits `LIMIT … OFFSET …` instead of the
  SqlServer-style `OFFSET … FETCH NEXT …` that PG rejects.
- Boolean parameter binding in `WHERE` no longer coerces to `int` on the
  Npgsql path.

#### Cross-adapter
- `FirstOrDefaultAsync` no longer hard-codes `LIMIT 1`; it now defers to the
  adapter's `ApplySkipTake(0, 1)`, which produces the correct dialect on
  every backend (SQL Server / Firebird in particular previously failed at
  parse time).
- `MapRow` is now case-insensitive for column-to-property matching, fixing
  PG's lower-cased return columns and SQL Server's mixed-case unquoted
  identifiers.

### Notes for Maintainers

- The Firebird container path requires `Pooling=false` in the connection
  string. With pooling enabled, xUnit re-instantiation across test classes
  triggers DDL lock contention inside Firebird and the suite hangs.
- The async aggregate parity tests (`QueryBuilder_CountAsync_AnyAsync_NoneAsync`)
  now run inside every adapter's integration test suite via
  `OrmIntegrationTestBase`.

---

## [1.2.0] — Previous baseline

Prior history was tracked outside this file. This CHANGELOG starts here.
