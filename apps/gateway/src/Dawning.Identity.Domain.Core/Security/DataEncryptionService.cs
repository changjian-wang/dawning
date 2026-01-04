using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Domain.Core.Security
{
    /// <summary>
    /// Data encryption service interface
    /// </summary>
    public interface IDataEncryptionService
    {
        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="plainText">Plain text</param>
        /// <returns>Encrypted Base64 string</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypt a string
        /// </summary>
        /// <param name="cipherText">Encrypted Base64 string</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// Try to decrypt (returns original text if decryption fails)
        /// </summary>
        string TryDecrypt(string text);

        /// <summary>
        /// Check if a string is encrypted
        /// </summary>
        bool IsEncrypted(string text);
    }

    /// <summary>
    /// AES-256 based data encryption service implementation
    /// </summary>
    public class AesDataEncryptionService : IDataEncryptionService
    {
        private readonly byte[] _key;
        private readonly ILogger<AesDataEncryptionService>? _logger;
        private const string EncryptionPrefix = "ENC:";

        public AesDataEncryptionService(
            IConfiguration configuration,
            ILogger<AesDataEncryptionService>? logger = null
        )
        {
            _logger = logger;

            // Read encryption key from configuration
            var keyBase64 = configuration["Security:EncryptionKey"];

            if (string.IsNullOrEmpty(keyBase64))
            {
                // If not configured, log a warning and use a default key (for development only)
                _logger?.LogWarning(
                    "Security:EncryptionKey not configured. Using a default key. THIS IS NOT SECURE FOR PRODUCTION!"
                );
                _key = GenerateDefaultKey();
            }
            else
            {
                try
                {
                    _key = Convert.FromBase64String(keyBase64);
                    if (_key.Length != 32)
                    {
                        throw new ArgumentException("Encryption key must be 256 bits (32 bytes)");
                    }
                }
                catch (FormatException)
                {
                    throw new ArgumentException(
                        "Invalid encryption key format. Must be a valid Base64 string."
                    );
                }
            }
        }

        /// <summary>
        /// Encrypt a string
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            // If already encrypted, return as-is
            if (IsEncrypted(plainText))
                return plainText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();

                // Write IV first
                msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                using (
                    var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)
                )
                using (var swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
                {
                    swEncrypt.Write(plainText);
                }

                return EncryptionPrefix + Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to encrypt data");
                throw new CryptographicException("Encryption failed", ex);
            }
        }

        /// <summary>
        /// Decrypt a string
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            // If no encryption prefix, return as-is
            if (!IsEncrypted(cipherText))
                return cipherText;

            try
            {
                // Remove prefix
                var actualCipherText = cipherText.Substring(EncryptionPrefix.Length);
                var cipherBytes = Convert.FromBase64String(actualCipherText);

                using var aes = Aes.Create();
                aes.Key = _key;

                // Extract IV from ciphertext (first 16 bytes)
                var iv = new byte[16];
                Array.Copy(cipherBytes, 0, iv, 0, 16);
                aes.IV = iv;

                // Actual ciphertext starts from byte 17
                var actualCipherBytes = new byte[cipherBytes.Length - 16];
                Array.Copy(cipherBytes, 16, actualCipherBytes, 0, actualCipherBytes.Length);

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new MemoryStream(actualCipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to decrypt data");
                throw new CryptographicException("Decryption failed", ex);
            }
        }

        /// <summary>
        /// Try to decrypt (returns original text if decryption fails)
        /// </summary>
        public string TryDecrypt(string text)
        {
            if (string.IsNullOrEmpty(text) || !IsEncrypted(text))
                return text;

            try
            {
                return Decrypt(text);
            }
            catch
            {
                return text;
            }
        }

        /// <summary>
        /// Check if a string is encrypted
        /// </summary>
        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(EncryptionPrefix);
        }

        /// <summary>
        /// Generate a new random encryption key
        /// </summary>
        public static string GenerateNewKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        /// <summary>
        /// Generate default key (for development environment only)
        /// </summary>
        private static byte[] GenerateDefaultKey()
        {
            // Use fixed seed to generate repeatable key (for development only)
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes("DawningDevKey2025!@#"));
        }
    }
}
