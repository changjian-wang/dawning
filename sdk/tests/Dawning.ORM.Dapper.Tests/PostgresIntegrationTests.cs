using System.Data;
using Dapper;
using Npgsql;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Runs the full <see cref="OrmIntegrationTestBase"/> suite against a real
/// PostgreSQL 16 database. Defaults to the local docker container provisioned
/// for ORM testing; override via the <c>DAWNING_ORM_POSTGRES</c> env var.
///
/// Provisioning (one-time):
/// <code>
/// docker run -d --name dawning-orm-pg \
///   -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres \
///   -e POSTGRES_DB=ormtest -p 5433:5432 postgres:16-alpine
/// </code>
///
/// PostgreSQL folds unquoted identifiers to lowercase; the ORM emits column
/// names quoted, so the schema below uses double-quoted identifiers wherever
/// the property name preserves case (Id, Name, Notes). The <c>score_value</c>
/// and <c>ts_ms</c> columns come from <c>[Column]</c> attributes and are
/// already lowercase.
/// </summary>
public class PostgresIntegrationTests : OrmIntegrationTestBase
{
    private const string DefaultConnectionString =
        "Host=127.0.0.1;Port=5433;Database=ormtest;Username=postgres;Password=postgres;";

    protected override IDbConnection CreateConnection()
    {
        var cs =
            Environment.GetEnvironmentVariable("DAWNING_ORM_POSTGRES") ?? DefaultConnectionString;
        return new NpgsqlConnection(cs);
    }

    protected override void ResetSchema()
    {
        Connection.Execute("DROP TABLE IF EXISTS widgets;");
        Connection.Execute("DROP TABLE IF EXISTS gadgets;");
        Connection.Execute("DROP TABLE IF EXISTS \"noOrder\";");
        Connection.Execute(
            @"CREATE TABLE widgets (
                ""Id""      BIGSERIAL PRIMARY KEY,
                ""Name""    TEXT NOT NULL,
                score_value INT NOT NULL DEFAULT 0,
                ""Notes""   TEXT NULL,
                ts_ms       BIGINT NOT NULL
            );"
        );
    }

    protected override void EnsureNoOrderSchema()
    {
        // Table name `noOrder` carries case; the ORM emits it unquoted in some
        // paths (FROM clause), and PostgreSQL folds unquoted identifiers to
        // lowercase. Create the table with the lowercase form so both quoted
        // and unquoted references resolve.
        Connection.Execute("DROP TABLE IF EXISTS \"noOrder\";");
        Connection.Execute("DROP TABLE IF EXISTS noorder;");
        Connection.Execute(
            @"CREATE TABLE noorder (
                ""Id""   BIGSERIAL PRIMARY KEY,
                ""Name"" TEXT NOT NULL
            );"
        );
    }

    protected override void EnsureGadgetSchema()
    {
        Connection.Execute("DROP TABLE IF EXISTS gadgets;");
        Connection.Execute(
            @"CREATE TABLE gadgets (
                ""Id""        UUID PRIMARY KEY,
                ""Name""      TEXT NOT NULL,
                ""CreatedBy"" TEXT NOT NULL,
                ""ServerTag"" TEXT NOT NULL DEFAULT 'srv-default',
                ""Display""   TEXT NULL
            );"
        );
    }

    protected override void DropAllTables()
    {
        Connection.Execute("DROP TABLE IF EXISTS widgets;");
        Connection.Execute("DROP TABLE IF EXISTS gadgets;");
        Connection.Execute("DROP TABLE IF EXISTS \"noOrder\";");
        Connection.Execute("DROP TABLE IF EXISTS noorder;");
    }

    // Schema columns are case-preserved via double quotes, so raw SQL must
    // also quote them or PostgreSQL folds Display -> display and rejects.
    protected override string SetGadgetDisplaySql => "UPDATE gadgets SET \"Display\" = 'computed!'";
}
