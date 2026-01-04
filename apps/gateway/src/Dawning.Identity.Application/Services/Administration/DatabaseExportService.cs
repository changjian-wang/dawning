using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;

namespace Dawning.Identity.Application.Services.Administration;

/// <summary>
/// Database export service interface
/// </summary>
public interface IDatabaseExportService
{
    /// <summary>
    /// Export database to SQL format
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="excludeTables">Set of table names to exclude</param>
    /// <returns>Database backup content in SQL format</returns>
    Task<string> ExportToSqlAsync(IDbConnection connection, HashSet<string>? excludeTables = null);
}

/// <summary>
/// Database export service implementation
/// Uses ADO.NET GetSchema() for cross-database compatibility
/// </summary>
public class DatabaseExportService : IDatabaseExportService
{
    /// <summary>
    /// Export database to SQL format
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
            // Use ADO.NET GetSchema to get table list (cross-database compatible)
            var tables = GetTableNames(connection, excludeTables);

            foreach (var table in tables)
            {
                // Export table data
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
    /// Get all table names (using ADO.NET GetSchema, cross-database compatible)
    /// </summary>
    private List<string> GetTableNames(IDbConnection connection, HashSet<string> excludeTables)
    {
        var tables = new List<string>();

        if (connection is DbConnection dbConnection)
        {
            // Use ADO.NET standard way to get table information
            var schema = dbConnection.GetSchema("Tables");

            foreach (DataRow row in schema.Rows)
            {
                // Different databases may have different column names, try common column names
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
    /// Get table name from DataRow
    /// </summary>
    private string? GetTableNameFromRow(DataRow row)
    {
        // Different databases use different column names
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
    /// Check if it is a system table
    /// </summary>
    private bool IsSystemTable(string tableName)
    {
        // Exclude common system table prefixes
        var systemPrefixes = new[] { "sys", "pg_", "sqlite_", "mysql.", "information_schema." };
        var lowerName = tableName.ToLowerInvariant();

        return systemPrefixes.Any(prefix => lowerName.StartsWith(prefix));
    }

    /// <summary>
    /// Export table data (using Dapper, cross-database compatible)
    /// </summary>
    private async Task ExportTableDataAsync(
        IDbConnection connection,
        string tableName,
        StringBuilder sb
    )
    {
        // Use parameterized table name (Note: table names cannot be parameterized, but here the table name comes from GetSchema, which is safe)
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
    /// Quote identifier (use different quotes based on database type)
    /// </summary>
    private string QuoteIdentifier(string identifier, IDbConnection connection)
    {
        var connType = connection.GetType().Name.ToLowerInvariant();

        return connType switch
        {
            var t when t.Contains("mysql") => $"`{identifier}`",
            var t when t.Contains("sqlserver") || t.Contains("sqlconnection") => $"[{identifier}]",
            var t when t.Contains("npgsql") || t.Contains("postgres") => $"\"{identifier}\"",
            _ => $"`{identifier}`" // Default to backticks
        };
    }

    /// <summary>
    /// Format SQL value
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
    /// Escape SQL string
    /// </summary>
    private static string EscapeSqlString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("'", "''"); // SQL standard escape
    }
}
