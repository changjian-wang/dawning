using System.Security.Cryptography;

var password = "admin";
var storedHash = "100000;UnarlhDVb+eAaojgf5zYpw==;Q4x7xrmCqQVXiJfDPYQ0wpgRtMFjbNe8Mk3iv7QENac=";

var parts = storedHash.Split(';');
var iterations = int.Parse(parts[0]);
var salt = Convert.FromBase64String(parts[1]);
var hash = Convert.FromBase64String(parts[2]);

var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
var isValid = CryptographicOperations.FixedTimeEquals(hash, computedHash);

Console.WriteLine("===================");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Verification: {isValid}");
Console.WriteLine("===================");
