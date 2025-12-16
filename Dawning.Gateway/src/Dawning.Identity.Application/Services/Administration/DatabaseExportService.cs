using System.Data;
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
/// 注意：此服务需要直接执行数据库元数据查询命令，这是导出功能的本质需求
/// </summary>
public class DatabaseExportService : IDatabaseExportService
{
    /// <summary>
    /// 导出数据库到 SQL 格式
    /// </summary>
    public async Task<string> ExportToSqlAsync(IDbConnection connection, HashSet<string>? excludeTables = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-- Database Backup");
        sb.AppendLine($"-- Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        excludeTables ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
        {
            connection.Open();
        }

        try
        {
            // 获取所有表名
            var tables = await GetAllTableNamesAsync(connection);

            foreach (var table in tables.Where(t => !excludeTables.Contains(t)))
            {
                // 导出表结构
                await ExportTableStructureAsync(connection, table, sb);

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
    /// 获取所有表名
    /// </summary>
    private async Task<List<string>> GetAllTableNamesAsync(IDbConnection connection)
    {
        // MySQL/MariaDB 元数据查询 - 无法避免直接 SQL
        return (await connection.QueryAsync<string>("SHOW TABLES")).ToList();
    }

    /// <summary>
    /// 导出表结构
    /// </summary>
    private async Task ExportTableStructureAsync(IDbConnection connection, string tableName, StringBuilder sb)
    {
        // MySQL/MariaDB DDL 元数据查询 - 无法避免直接 SQL
        var createTableResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
            $"SHOW CREATE TABLE `{tableName}`"
        );

        if (createTableResult != null)
        {
            sb.AppendLine($"-- Table structure for `{tableName}`");
            sb.AppendLine($"DROP TABLE IF EXISTS `{tableName}`;");

            var dict = (IDictionary<string, object>)createTableResult;
            var createStatement = dict.Values.Skip(1).FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(createStatement))
            {
                sb.AppendLine(createStatement + ";");
            }
            sb.AppendLine();
        }
    }

    /// <summary>
    /// 导出表数据
    /// </summary>
    private async Task ExportTableDataAsync(IDbConnection connection, string tableName, StringBuilder sb)
    {
        // 动态数据导出 - 必须使用动态 SQL
        var rows = await connection.QueryAsync($"SELECT * FROM `{tableName}`");
        var rowList = rows.ToList();

        if (rowList.Any())
        {
            sb.AppendLine($"-- Data for `{tableName}`");

            foreach (var row in rowList)
            {
                var dict = (IDictionary<string, object>)row;
                var columns = string.Join(", ", dict.Keys.Select(k => $"`{k}`"));
                var values = string.Join(", ", dict.Values.Select(FormatSqlValue));
                sb.AppendLine($"INSERT INTO `{tableName}` ({columns}) VALUES ({values});");
            }
            sb.AppendLine();
        }
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
            Guid g => $"'{g}'",
            bool b => b ? "1" : "0",
            byte[] bytes => $"0x{BitConverter.ToString(bytes).Replace("-", "")}",
            _ => value.ToString() ?? "NULL"
        };
    }

    /// <summary>
    /// 转义 SQL 字符串
    /// </summary>
    private static string EscapeSqlString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
