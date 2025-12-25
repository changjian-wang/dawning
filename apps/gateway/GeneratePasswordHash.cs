using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public class PasswordHasher
{
    public static string Hash(string password)
    {
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        string hashed = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            )
        );

        return $"{Convert.ToBase64String(salt)}:{hashed}";
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Password hash for 'admin':");
        Console.WriteLine(PasswordHasher.Hash("admin"));
    }
}
