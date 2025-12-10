using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

class Verify
{
    static async Task Main(string[] args)
    {
        var connectionString = "Server=localhost;Port=3306;Database=dawning_identity;User=aluneth;Password=123456";
        
        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("=== Database Verification ===\n");
            
            // Query permissions by resource
            var query = "SELECT resource, COUNT(*) as count FROM permissions GROUP BY resource ORDER BY resource";
            await using var cmd = new MySqlCommand(query, connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            Console.WriteLine("Permissions by Resource:");
            while (await reader.ReadAsync())
            {
                Console.WriteLine($"  {reader["resource"]}: {reader["count"]}");
            }
            await reader.CloseAsync();
            
            // Query permissions by category
            var categoryQuery = "SELECT category, COUNT(*) as count FROM permissions GROUP BY category ORDER BY category";
            await using var catCmd = new MySqlCommand(categoryQuery, connection);
            await using var catReader = await catCmd.ExecuteReaderAsync();
            
            Console.WriteLine("\nPermissions by Category:");
            while (await catReader.ReadAsync())
            {
                Console.WriteLine($"  {catReader["category"]}: {catReader["count"]}");
            }
            await catReader.CloseAsync();
            
            // Query role assignments
            var roleQuery = @"
                SELECT r.name, COUNT(rp.permission_id) as perm_count 
                FROM roles r 
                LEFT JOIN role_permissions rp ON r.id = rp.role_id 
                GROUP BY r.id, r.name
                ORDER BY r.name";
            await using var roleCmd = new MySqlCommand(roleQuery, connection);
            await using var roleReader = await roleCmd.ExecuteReaderAsync();
            
            Console.WriteLine("\nRole Permission Counts:");
            while (await roleReader.ReadAsync())
            {
                Console.WriteLine($"  {roleReader["name"]}: {roleReader["perm_count"]}");
            }
            await roleReader.CloseAsync();
            
            // Show sample permissions
            var sampleQuery = "SELECT code, name, resource FROM permissions WHERE resource = 'user' ORDER BY display_order LIMIT 5";
            await using var sampleCmd = new MySqlCommand(sampleQuery, connection);
            await using var sampleReader = await sampleCmd.ExecuteReaderAsync();
            
            Console.WriteLine("\nSample User Permissions:");
            while (await sampleReader.ReadAsync())
            {
                Console.WriteLine($"  {sampleReader["code"],-25} - {sampleReader["name"]}");
            }
            
            Console.WriteLine("\n=== Verification Complete ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
