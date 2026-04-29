using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Runs the full <see cref="OrmIntegrationTestBase"/> suite against a fresh
/// in-memory SQLite database. Each test method gets its own connection and DB
/// (the lifetime of an in-memory SQLite database is bound to its connection).
/// </summary>
public class SqliteIntegrationTests : OrmIntegrationTestBase
{
    protected override IDbConnection CreateConnection()
    {
        return new SqliteConnection("Data Source=:memory:");
    }

    protected override void ResetSchema()
    {
        Connection.Execute(
            @"CREATE TABLE widgets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                score_value INTEGER NOT NULL DEFAULT 0,
                Notes TEXT NULL,
                Timestamp INTEGER NOT NULL
            );"
        );
    }

    protected override void EnsureNoOrderSchema()
    {
        Connection.Execute(
            "CREATE TABLE noOrder (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);"
        );
    }

    protected override void EnsureGadgetSchema()
    {
        Connection.Execute(
            @"CREATE TABLE IF NOT EXISTS gadgets (
                Id          TEXT PRIMARY KEY,
                Name        TEXT NOT NULL,
                CreatedBy   TEXT NOT NULL,
                ServerTag   TEXT NOT NULL DEFAULT 'srv-default',
                Display     TEXT NULL
            );"
        );
    }

    protected override void DropAllTables()
    {
        // In-memory SQLite vanishes with the connection; nothing to do.
    }
}
