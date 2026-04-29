using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Runs the full <see cref="OrmIntegrationTestBase"/> suite against a real
/// SQL Server 2022 database. Defaults to the local docker container
/// provisioned for ORM testing; override via the <c>DAWNING_ORM_SQLSERVER</c>
/// env var.
///
/// Provisioning (one-time):
/// <code>
/// docker run -d --name dawning-orm-mssql \
///   -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=Dawning_Test_2024! \
///   -p 1434:1433 mcr.microsoft.com/mssql/server:2022-latest
/// docker exec dawning-orm-mssql /opt/mssql-tools18/bin/sqlcmd \
///   -S localhost -U sa -P Dawning_Test_2024! -No -C \
///   -Q "CREATE DATABASE OrmTest;"
/// </code>
///
/// Notes for SQL Server:
/// - SqlServerAdapter quotes identifiers with brackets ([Id], [Name], …).
/// - Default OrmTest collation is case-insensitive, so identifier case is
///   not strictly required to match — but the schema below uses the same
///   casing as the ORM emits.
/// - Pagination relies on <c>OFFSET … FETCH NEXT …</c> and therefore demands
///   an explicit ORDER BY (the QueryBuilder enforces this for us).
/// </summary>
public class SqlServerIntegrationTests : OrmIntegrationTestBase
{
    private const string DefaultConnectionString =
        "Server=127.0.0.1,1434;Database=OrmTest;User Id=sa;Password=Dawning_Test_2024!;"
        + "TrustServerCertificate=True;Encrypt=False;";

    protected override IDbConnection CreateConnection()
    {
        var cs =
            Environment.GetEnvironmentVariable("DAWNING_ORM_SQLSERVER") ?? DefaultConnectionString;
        return new SqlConnection(cs);
    }

    protected override void ResetSchema()
    {
        Connection.Execute("IF OBJECT_ID('dbo.widgets', 'U') IS NOT NULL DROP TABLE dbo.widgets;");
        Connection.Execute("IF OBJECT_ID('dbo.gadgets', 'U') IS NOT NULL DROP TABLE dbo.gadgets;");
        Connection.Execute("IF OBJECT_ID('dbo.noOrder', 'U') IS NOT NULL DROP TABLE dbo.noOrder;");
        Connection.Execute(
            @"CREATE TABLE dbo.widgets (
                [Id]          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Name]        NVARCHAR(255) NOT NULL,
                [score_value] INT NOT NULL DEFAULT 0,
                [Notes]       NVARCHAR(255) NULL,
                [ts_ms]       BIGINT NOT NULL
            );"
        );
    }

    protected override void EnsureNoOrderSchema()
    {
        Connection.Execute("IF OBJECT_ID('dbo.noOrder', 'U') IS NOT NULL DROP TABLE dbo.noOrder;");
        Connection.Execute(
            @"CREATE TABLE dbo.noOrder (
                [Id]   BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Name] NVARCHAR(255) NOT NULL
            );"
        );
    }

    protected override void EnsureGadgetSchema()
    {
        Connection.Execute("IF OBJECT_ID('dbo.gadgets', 'U') IS NOT NULL DROP TABLE dbo.gadgets;");
        Connection.Execute(
            @"CREATE TABLE dbo.gadgets (
                [Id]        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [Name]      NVARCHAR(255) NOT NULL,
                [CreatedBy] NVARCHAR(255) NOT NULL,
                [ServerTag] NVARCHAR(255) NOT NULL DEFAULT 'srv-default',
                [Display]   NVARCHAR(255) NULL
            );"
        );
    }

    protected override void DropAllTables()
    {
        Connection.Execute("IF OBJECT_ID('dbo.widgets', 'U') IS NOT NULL DROP TABLE dbo.widgets;");
        Connection.Execute("IF OBJECT_ID('dbo.gadgets', 'U') IS NOT NULL DROP TABLE dbo.gadgets;");
        Connection.Execute("IF OBJECT_ID('dbo.noOrder', 'U') IS NOT NULL DROP TABLE dbo.noOrder;");
    }
}
