using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Security;
using Microsoft.Extensions.Configuration;

namespace Dawning.Identity.Application.Services.Security
{
    /// <summary>
    /// Password policy service implementation
    /// </summary>
    public class PasswordPolicyService : IPasswordPolicyService
    {
        private readonly IConfiguration _configuration;
        private PasswordPolicy? _cachedPolicy;
        private DateTime _policyCacheTime;
        private static readonly TimeSpan PolicyCacheDuration = TimeSpan.FromMinutes(5);

        public PasswordPolicyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Validate whether password complies with policy
        /// </summary>
        public async Task<PasswordValidationResult> ValidatePasswordAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return PasswordValidationResult.Failure("Password cannot be empty");
            }

            var policy = await GetPolicyAsync();
            var errors = new List<string>();

            // Check length
            if (password.Length < policy.MinLength)
            {
                errors.Add($"Password must be at least {policy.MinLength} characters");
            }

            if (password.Length > policy.MaxLength)
            {
                errors.Add($"Password cannot exceed {policy.MaxLength} characters");
            }

            // Check uppercase letters
            if (policy.RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            {
                errors.Add("Password must contain at least one uppercase letter");
            }

            // Check lowercase letters
            if (policy.RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            {
                errors.Add("Password must contain at least one lowercase letter");
            }

            // Check digits
            if (policy.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            {
                errors.Add("Password must contain at least one digit");
            }

            // Check special characters
            if (policy.RequireSpecialChar)
            {
                var escapedChars = Regex.Escape(policy.SpecialCharacters);
                if (!Regex.IsMatch(password, $"[{escapedChars}]"))
                {
                    errors.Add($"Password must contain at least one special character ({policy.SpecialCharacters})");
                }
            }

            // Check common weak password patterns
            if (IsCommonWeakPassword(password))
            {
                errors.Add("Password is too simple, please use a more complex password");
            }

            return errors.Count == 0
                ? PasswordValidationResult.Success()
                : new PasswordValidationResult { IsValid = false, Errors = errors };
        }

        /// <summary>
        /// Get current password policy
        /// </summary>
        public Task<PasswordPolicy> GetPolicyAsync()
        {
            // Use simple cache to avoid frequent config reads
            if (_cachedPolicy != null && DateTime.UtcNow - _policyCacheTime < PolicyCacheDuration)
            {
                return Task.FromResult(_cachedPolicy);
            }

            var policy = new PasswordPolicy();

            // Read configuration from appsettings.json
            var passwordSection = _configuration.GetSection("Security:Password");
            if (passwordSection.Exists())
            {
                policy.MinLength = passwordSection.GetValue("MinLength", 8);
                policy.MaxLength = passwordSection.GetValue("MaxLength", 128);
                policy.RequireUppercase = passwordSection.GetValue("RequireUppercase", true);
                policy.RequireLowercase = passwordSection.GetValue("RequireLowercase", true);
                policy.RequireDigit = passwordSection.GetValue("RequireDigit", true);
                policy.RequireSpecialChar = passwordSection.GetValue("RequireSpecialChar", false);
                policy.SpecialCharacters =
                    passwordSection.GetValue("SpecialCharacters", "!@#$%^&*()_+-=[]{}|;:,.<>?")
                    ?? "!@#$%^&*()_+-=[]{}|;:,.<>?";
            }

            _cachedPolicy = policy;
            _policyCacheTime = DateTime.UtcNow;

            return Task.FromResult(policy);
        }

        /// <summary>
        /// Check if password is a common weak password
        /// </summary>
        private static bool IsCommonWeakPassword(string password)
        {
            var lowerPassword = password.ToLowerInvariant();

            // Common weak passwords list
            var weakPasswords = new HashSet<string>
            {
                "password",
                "password1",
                "password123",
                "123456",
                "12345678",
                "123456789",
                "qwerty",
                "qwerty123",
                "abc123",
                "abcd1234",
                "admin",
                "admin123",
                "administrator",
                "letmein",
                "welcome",
                "monkey",
                "dragon",
                "master",
                "111111",
                "000000",
                "123123",
                "654321",
            };

            if (weakPasswords.Contains(lowerPassword))
            {
                return true;
            }

            // Check repeated character patterns (e.g., "aaaa", "1111")
            if (Regex.IsMatch(password, @"^(.)\1{3,}$"))
            {
                return true;
            }

            // Check simple sequences (e.g., "abcd", "1234")
            if (IsSequentialPattern(password))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if password is a sequential character pattern
        /// </summary>
        private static bool IsSequentialPattern(string password)
        {
            if (password.Length < 4)
                return false;

            var isSequential = true;
            for (int i = 1; i < password.Length && isSequential; i++)
            {
                // Check if consecutive ascending or descending
                var diff = password[i] - password[i - 1];
                if (i == 1)
                {
                    isSequential = diff == 1 || diff == -1;
                }
                else
                {
                    var prevDiff = password[i - 1] - password[i - 2];
                    isSequential = diff == prevDiff && (diff == 1 || diff == -1);
                }
            }

            return isSequential;
        }
    }
}
