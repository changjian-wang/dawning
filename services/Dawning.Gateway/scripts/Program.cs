using System;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString =
            "Server=localhost;Port=3306;Database=dawning_identity;User=aluneth;Password=123456";
        var sqlFilePath =
            @"C:\github\dawning\Dawning.Gateway\docs\sql\008_create_permissions_table.sql";

        try
        {
            var sql = await File.ReadAllTextAsync(sqlFilePath);

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            Console.WriteLine("Connected to MySQL successfully.");

            // Use MySqlScript to handle multi-statement SQL with proper parsing
            var script = new MySql.Data.MySqlClient.MySqlScript(connection, sql);
            script.StatementExecuted += (sender, args) =>
            {
                Console.WriteLine(
                    $"Executed: {args.StatementText.Substring(0, Math.Min(80, args.StatementText.Length)).Replace("\n", " ")}..."
                );
            };

            var result = script.Execute();

            Console.WriteLine($"SQL script executed successfully! ({result} statements)");

            // 验证表是否创建成功
            var checkSql = "SELECT COUNT(*) FROM permissions WHERE is_system = 1";
            await using var checkCommand = new MySqlCommand(checkSql, connection);
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            Console.WriteLine($"System permissions created: {count}");

            // 验证角色权限分配
            var rolePermSql = "SELECT COUNT(*) FROM role_permissions";
            await using var rolePermCommand = new MySqlCommand(rolePermSql, connection);
            var rolePermCount = Convert.ToInt32(await rolePermCommand.ExecuteScalarAsync());

            Console.WriteLine($"Role permissions assigned: {rolePermCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}
