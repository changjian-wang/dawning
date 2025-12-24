using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;

namespace Dawning.Identity.Application.Services.Administration;

/// <summary>
/// 数据库导出服务接口
/// </summary>
public interface IDatabaseExportService
{
    /// <summary>
    /// 导出数据库到 SQL 格式
    /// </summary>
    /// <param name="connection">数据库连接</param>
    /// <param name="excludeTables">要排除的表名集合</param>
    /// <returns>SQL 格式的数据库备份内容</returns>
    Task<string> ExportToSqlAsync(IDbConnection connection, HashSet<string>? excludeTables = null);
}

/// <summary>
/// 数据库导出服务实现
/// 使用 ADO.NET GetSchema() 实现跨数据库兼容
/// </summary>
public class DatabaseExportService : IDatabaseExportService
{
    /// <summary>
    /// 导出数据库到 SQL 格式
    /// </summary>
    public async Task<string> ExportToSqlAsync(
        IDbConnection connection,
        HashSet<string>? excludeTables = null
    )
    {
        var sb = new StringBuilder();
        sb.AppendLine("-- Database Backup");
        sb.AppendLine($"-- Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"-- Database Type: {connection.GetType().Name}");
        sb.AppendLine();

        excludeTables ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
        {
            connection.Open();
        }

        try
        {
            // 使用 ADO.NET GetSchema 获取表列表（跨数据库兼容）
            var tables = GetTableNames(connection, excludeTables);

            foreach (var table in tables)
            {
                // 导出表数据
                await ExportTableDataAsync(connection, table, sb);
            }
        }
        finally
        {
            if (!wasOpen)
            {
                connection.Close();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取所有表名（使用 ADO.NET GetSchema，跨数据库兼容）
    /// </summary>
    private List<string> GetTableNames(IDbConnection connection, HashSet<string> excludeTables)
    {
        var tables = new List<string>();

        if (connection is DbConnection dbConnection)
        {
            // 使用 ADO.NET 标准方式获取表信息
            var schema = dbConnection.GetSchema("Tables");

            foreach (DataRow row in schema.Rows)
            {
                // 不同数据库的列名可能不同，尝试常见的列名
                var tableName = GetTableNameFromRow(row);

                if (
                    !string.IsNullOrEmpty(tableName)
                    && !excludeTables.Contains(tableName)
                    && !IsSystemTable(tableName)
                )
                {
                    tables.Add(tableName);
                }
            }
        }

        return tables.OrderBy(t => t).ToList();
    }

    /// <summary>
    /// 从 DataRow 获取表名
    /// </summary>
    private string? GetTableNameFromRow(DataRow row)
    {
        // 不同数据库使用不同的列名
        string[] possibleColumns = { "TABLE_NAME", "table_name", "TableName", "Name" };

        foreach (var col in possibleColumns)
        {
            if (row.Table.Columns.Contains(col) && row[col] != DBNull.Value)
            {
                return row[col]?.ToString();
            }
        }

        return null;
    }

    /// <summary>
    /// 判断是否为系统表
    /// </summary>
    private bool IsSystemTable(string tableName)
    {
        // 排除常见的系统表前缀
        var systemPrefixes = new[] { "sys", "pg_", "sqlite_", "mysql.", "information_schema." };
        var lowerName = tableName.ToLowerInvariant();

        return systemPrefixes.Any(prefix => lowerName.StartsWith(prefix));
    }

    /// <summary>
    /// 导出表数据（使用 Dapper，跨数据库兼容）
    /// </summary>
    private async Task ExportTableDataAsync(
        IDbConnection connection,
        string tableName,
        StringBuilder sb
    )
    {
        // 使用参数化的表名（注意：表名无法参数化，但这里的表名来自 GetSchema，是安全的）
        var quotedTable = QuoteIdentifier(tableName, connection);
        var rows = await connection.QueryAsync($"SELECT * FROM {quotedTable}");
        var rowList = rows.ToList();

        if (rowList.Any())
        {
            sb.AppendLine($"-- Data for `{tableName}`");

            foreach (var row in rowList)
            {
                var dict = (IDictionary<string, object>)row;
                var columns = string.Join(
                    ", ",
                    dict.Keys.Select(k => QuoteIdentifier(k, connection))
                );
                var values = string.Join(", ", dict.Values.Select(FormatSqlValue));
                sb.AppendLine($"INSERT INTO {quotedTable} ({columns}) VALUES ({values});");
            }
            sb.AppendLine();
        }
    }

    /// <summary>
    /// 引用标识符（根据数据库类型使用不同的引号）
    /// </summary>
    private string QuoteIdentifier(string identifier, IDbConnection connection)
    {
        var connType = connection.GetType().Name.ToLowerInvariant();

        return connType switch
        {
            var t when t.Contains("mysql") => $"`{identifier}`",
            var t when t.Contains("sqlserver") || t.Contains("sqlconnection") => $"[{identifier}]",
            var t when t.Contains("npgsql") || t.Contains("postgres") => $"\"{identifier}\"",
            _ => $"`{identifier}`" // 默认使用反引号
        };
    }

    /// <summary>
    /// 格式化 SQL 值
    /// </summary>
    private static string FormatSqlValue(object? value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return value switch
        {
            string s => $"'{EscapeSqlString(s)}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            DateTimeOffset dto => $"'{dto:yyyy-MM-dd HH:mm:ss}'",
            Guid g => $"'{g}'",
            bool b => b ? "1" : "0",
            byte[] bytes => $"0x{BitConverter.ToString(bytes).Replace("-", "")}",
            _ => value.ToString() ?? "NULL",
        };
    }

    /// <summary>
    /// 转义 SQL 字符串
    /// </summary>
    private static string EscapeSqlString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("'", "''"); // SQL 标准转义
    }
}
