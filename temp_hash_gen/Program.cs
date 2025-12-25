using System;
using System.Security.Cryptography;

const int SaltSize = 16;
const int KeySize = 32;
const int Iterations = 100000;
const char Delimiter = ';';

var password = "Admin@123";
var salt = RandomNumberGenerator.GetBytes(SaltSize);
var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
var result = string.Join(Delimiter, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
Console.WriteLine(result);
