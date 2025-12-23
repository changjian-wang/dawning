using System.Security.Cryptography;

// 测试密码验证
var password = "Admin@123456";
var storedHash = "100000;UnarlhDVb+eAaojgf5zYpw==;Q4x7xrmCqQVXiJfDPYQ0wpgRtMFjbNe8Mk3iv7QENac=";

var parts = storedHash.Split(';');
var iterations = int.Parse(parts[0]);
var salt = Convert.FromBase64String(parts[1]);
var hash = Convert.FromBase64String(parts[2]);

var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
var isValid = CryptographicOperations.FixedTimeEquals(hash, computedHash);

Console.WriteLine("=== Verify password ===");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Verification: {isValid}");

// 生成新哈希
Console.WriteLine("\n=== Generate new hash ===");
var newSalt = RandomNumberGenerator.GetBytes(16);
var newHash = Rfc2898DeriveBytes.Pbkdf2("Admin@123456", newSalt, 100000, HashAlgorithmName.SHA256, 32);
var newHashStr = $"100000;{Convert.ToBase64String(newSalt)};{Convert.ToBase64String(newHash)}";
Console.WriteLine($"New hash for Admin@123456:");
Console.WriteLine(newHashStr);
