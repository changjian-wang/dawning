using System.Security.Cryptography;
using System.Text;

namespace Dawning.Identity.Domain.Core.Security
{
    /// <summary>
    /// Password hashing utility (using PBKDF2)
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 100000; // OWASP recommended iteration count
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        private const char Delimiter = ';';

        /// <summary>
        /// Hashes a password
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Format: iterations;salt;hash</returns>
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
        /// Verifies a password
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="passwordHash">Stored hash value</param>
        /// <returns>Whether verification succeeded</returns>
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
        /// Checks whether the password needs to be rehashed (e.g., iteration count has been updated)
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
