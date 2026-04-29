using System.Data;
using Dapper;
using MySqlConnector;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Runs the full <see cref="OrmIntegrationTestBase"/> suite against a real
/// MySQL 8 database. Defaults to the local docker container provisioned
/// by the project; override via the <c>DAWNING_ORM_MYSQL</c> env var.
///
/// Provisioning (one-time):
/// <code>
/// docker exec dawning-mysql mysql -uroot -p... -e \
///   "CREATE DATABASE dawning_orm_test CHARACTER SET utf8mb4;"
/// </code>
/// </summary>
public class MySqlIntegrationTests : OrmIntegrationTestBase
{
    private const string DefaultConnectionString =
        "Server=127.0.0.1;Port=3306;Database=dawning_orm_test;"
        + "User=dawning;Password=dawning_password_2024;"
        + "AllowPublicKeyRetrieval=True;SslMode=None;ConnectionReset=False;";

    protected override IDbConnection CreateConnection()
    {
        var cs = Environment.GetEnvironmentVariable("DAWNING_ORM_MYSQL") ?? DefaultConnectionString;
        return new MySqlConnection(cs);
    }

    protected override void ResetSchema()
    {
        Connection.Execute("DROP TABLE IF EXISTS widgets;");
        Connection.Execute("DROP TABLE IF EXISTS gadgets;");
        Connection.Execute("DROP TABLE IF EXISTS noOrder;");
        Connection.Execute(
            @"CREATE TABLE widgets (
                `Id`          BIGINT NOT NULL AUTO_INCREMENT,
                `Name`        VARCHAR(255) NOT NULL,
                `score_value` INT NOT NULL DEFAULT 0,
                `Notes`       VARCHAR(255) NULL,
                `Timestamp`   BIGINT NOT NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
        );
    }

    protected override void EnsureNoOrderSchema()
    {
        Connection.Execute("DROP TABLE IF EXISTS noOrder;");
        Connection.Execute(
            @"CREATE TABLE noOrder (
                `Id`   BIGINT NOT NULL AUTO_INCREMENT,
                `Name` VARCHAR(255) NOT NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
        );
    }

    protected override void EnsureGadgetSchema()
    {
        Connection.Execute("DROP TABLE IF EXISTS gadgets;");
        Connection.Execute(
            @"CREATE TABLE gadgets (
                `Id`        CHAR(36) NOT NULL,
                `Name`      VARCHAR(255) NOT NULL,
                `CreatedBy` VARCHAR(255) NOT NULL,
                `ServerTag` VARCHAR(255) NOT NULL DEFAULT 'srv-default',
                `Display`   VARCHAR(255) NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
        );
    }

    protected override void DropAllTables()
    {
        Connection.Execute("DROP TABLE IF EXISTS widgets;");
        Connection.Execute("DROP TABLE IF EXISTS gadgets;");
        Connection.Execute("DROP TABLE IF EXISTS noOrder;");
    }
}
