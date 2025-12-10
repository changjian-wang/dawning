using MySql.Data.MySqlClient;

var connectionString = "Server=localhost;Port=3306;Database=dawning_identity;User=aluneth;Password=123456";
await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();

// Query permissions by resource
var query = "SELECT resource, COUNT(*) as count FROM permissions GROUP BY resource";
await using var cmd = new MySqlCommand(query, connection);
await using var reader = await cmd.ExecuteReaderAsync();

Console.WriteLine("
Permissions by Resource:");
while (await reader.ReadAsync())
{
    Console.WriteLine($"{reader["resource"]}: {reader["count"]}");
}
reader.Close();

// Query role assignments
var roleQuery = "SELECT r.name, COUNT(rp.permission_id) as perm_count FROM roles r LEFT JOIN role_permissions rp ON r.id = rp.role_id GROUP BY r.id, r.name";
await using var roleCmd = new MySqlCommand(roleQuery, connection);
await using var roleReader = await roleCmd.ExecuteReaderAsync();

Console.WriteLine("
Role Permission Counts:");
while (await roleReader.ReadAsync())
{
    Console.WriteLine($"{roleReader["name"]}: {roleReader["perm_count"]}");
}
