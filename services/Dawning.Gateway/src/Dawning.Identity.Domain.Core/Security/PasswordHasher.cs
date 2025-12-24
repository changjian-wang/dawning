using System.Security.Cryptography;
using System.Text;

namespace Dawning.Identity.Domain.Core.Security
{
    /// <summary>
    /// 密码哈希工具（使用 PBKDF2）
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 100000; // OWASP 推荐的迭代次数
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        private const char Delimiter = ';';

        /// <summary>
        /// 哈希密码
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <returns>格式: iterations;salt;hash</returns>
        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

            return string.Join(
                Delimiter,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash)
            );
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">待验证的明文密码</param>
        /// <param name="passwordHash">存储的哈希值</param>
        /// <returns>验证是否成功</returns>
        public static bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrEmpty(passwordHash))
            {
                return false;
            }

            try
            {
                var parts = passwordHash.Split(Delimiter);
                if (parts.Length != 3)
                {
                    return false;
                }

                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var hash = Convert.FromBase64String(parts[2]);

                var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    Algorithm,
                    hash.Length
                );

                return CryptographicOperations.FixedTimeEquals(hash, inputHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查密码是否需要重新哈希（例如迭代次数已更新）
        /// </summary>
        public static bool NeedsRehash(string passwordHash)
        {
            try
            {
                var parts = passwordHash.Split(Delimiter);
                if (parts.Length != 3)
                {
                    return true;
                }

                var iterations = int.Parse(parts[0]);
                return iterations < Iterations;
            }
            catch
            {
                return true;
            }
        }
    }
}
