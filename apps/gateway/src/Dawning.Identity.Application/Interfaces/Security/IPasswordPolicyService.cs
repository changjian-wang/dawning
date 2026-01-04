using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Security
{
    /// <summary>
    /// Password policy service interface
    /// </summary>
    public interface IPasswordPolicyService
    {
        /// <summary>
        /// Validates whether the password meets the policy requirements
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Validation result</returns>
        Task<PasswordValidationResult> ValidatePasswordAsync(string password);

        /// <summary>
        /// Gets the current password policy
        /// </summary>
        Task<PasswordPolicy> GetPolicyAsync();
    }

    /// <summary>
    /// Password policy configuration
    /// </summary>
    public class PasswordPolicy
    {
        /// <summary>
        /// Minimum length
        /// </summary>
        public int MinLength { get; set; } = 8;

        /// <summary>
        /// Maximum length
        /// </summary>
        public int MaxLength { get; set; } = 128;

        /// <summary>
        /// Whether uppercase letters are required
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// Whether lowercase letters are required
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// Whether digits are required
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// Whether special characters are required
        /// </summary>
        public bool RequireSpecialChar { get; set; } = false;

        /// <summary>
        /// Special character set
        /// </summary>
        public string SpecialCharacters { get; set; } = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
    }

    /// <summary>
    /// Password validation result
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// Whether validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of error messages
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Creates a success result
        /// </summary>
        public static PasswordValidationResult Success() =>
            new PasswordValidationResult { IsValid = true };

        /// <summary>
        /// Creates a failure result
        /// </summary>
        public static PasswordValidationResult Failure(params string[] errors) =>
            new PasswordValidationResult { IsValid = false, Errors = errors.ToList() };
    }
}
