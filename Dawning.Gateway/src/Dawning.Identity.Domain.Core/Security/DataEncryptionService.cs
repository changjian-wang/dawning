using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Domain.Core.Security
{
    /// <summary>
    /// 数据加密服务接口
    /// </summary>
    public interface IDataEncryptionService
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>加密后的 Base64 字符串</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="cipherText">加密的 Base64 字符串</param>
        /// <returns>解密后的明文</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// 尝试解密（如果失败返回原文）
        /// </summary>
        string TryDecrypt(string text);

        /// <summary>
        /// 检查字符串是否已加密
        /// </summary>
        bool IsEncrypted(string text);
    }

    /// <summary>
    /// 基于 AES-256 的数据加密服务实现
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

            // 从配置读取加密密钥
            var keyBase64 = configuration["Security:EncryptionKey"];

            if (string.IsNullOrEmpty(keyBase64))
            {
                // 如果未配置，生成一个警告并使用默认密钥（仅用于开发）
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
        /// 加密字符串
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            // 如果已经加密，直接返回
            if (IsEncrypted(plainText))
                return plainText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var msEncrypt = new MemoryStream();

                // 先写入 IV
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
        /// 解密字符串
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            // 如果没有加密前缀，直接返回
            if (!IsEncrypted(cipherText))
                return cipherText;

            try
            {
                // 移除前缀
                var actualCipherText = cipherText.Substring(EncryptionPrefix.Length);
                var cipherBytes = Convert.FromBase64String(actualCipherText);

                using var aes = Aes.Create();
                aes.Key = _key;

                // 从密文中提取 IV（前16字节）
                var iv = new byte[16];
                Array.Copy(cipherBytes, 0, iv, 0, 16);
                aes.IV = iv;

                // 实际密文从第17字节开始
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
        /// 尝试解密（如果失败返回原文）
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
        /// 检查字符串是否已加密
        /// </summary>
        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(EncryptionPrefix);
        }

        /// <summary>
        /// 生成一个新的随机加密密钥
        /// </summary>
        public static string GenerateNewKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        /// <summary>
        /// 生成默认密钥（仅用于开发环境）
        /// </summary>
        private static byte[] GenerateDefaultKey()
        {
            // 使用固定种子生成可重复的密钥（仅用于开发）
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes("DawningDevKey2025!@#"));
        }
    }
}
