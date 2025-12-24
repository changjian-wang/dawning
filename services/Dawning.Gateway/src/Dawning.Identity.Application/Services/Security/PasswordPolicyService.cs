using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Security;
using Microsoft.Extensions.Configuration;

namespace Dawning.Identity.Application.Services.Security
{
    /// <summary>
    /// 密码策略服务实现
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
        /// 验证密码是否符合策略
        /// </summary>
        public async Task<PasswordValidationResult> ValidatePasswordAsync(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return PasswordValidationResult.Failure("密码不能为空");
            }

            var policy = await GetPolicyAsync();
            var errors = new List<string>();

            // 检查长度
            if (password.Length < policy.MinLength)
            {
                errors.Add($"密码长度不能少于 {policy.MinLength} 个字符");
            }

            if (password.Length > policy.MaxLength)
            {
                errors.Add($"密码长度不能超过 {policy.MaxLength} 个字符");
            }

            // 检查大写字母
            if (policy.RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            {
                errors.Add("密码必须包含至少一个大写字母");
            }

            // 检查小写字母
            if (policy.RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            {
                errors.Add("密码必须包含至少一个小写字母");
            }

            // 检查数字
            if (policy.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            {
                errors.Add("密码必须包含至少一个数字");
            }

            // 检查特殊字符
            if (policy.RequireSpecialChar)
            {
                var escapedChars = Regex.Escape(policy.SpecialCharacters);
                if (!Regex.IsMatch(password, $"[{escapedChars}]"))
                {
                    errors.Add($"密码必须包含至少一个特殊字符 ({policy.SpecialCharacters})");
                }
            }

            // 检查常见弱密码模式
            if (IsCommonWeakPassword(password))
            {
                errors.Add("密码过于简单，请使用更复杂的密码");
            }

            return errors.Count == 0
                ? PasswordValidationResult.Success()
                : new PasswordValidationResult { IsValid = false, Errors = errors };
        }

        /// <summary>
        /// 获取当前密码策略
        /// </summary>
        public Task<PasswordPolicy> GetPolicyAsync()
        {
            // 使用简单缓存避免频繁读取配置
            if (_cachedPolicy != null && DateTime.UtcNow - _policyCacheTime < PolicyCacheDuration)
            {
                return Task.FromResult(_cachedPolicy);
            }

            var policy = new PasswordPolicy();

            // 从 appsettings.json 读取配置
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
        /// 检查是否为常见弱密码
        /// </summary>
        private static bool IsCommonWeakPassword(string password)
        {
            var lowerPassword = password.ToLowerInvariant();

            // 常见弱密码列表
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

            // 检查重复字符模式 (如 "aaaa", "1111")
            if (Regex.IsMatch(password, @"^(.)\1{3,}$"))
            {
                return true;
            }

            // 检查简单序列 (如 "abcd", "1234")
            if (IsSequentialPattern(password))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否为连续字符序列
        /// </summary>
        private static bool IsSequentialPattern(string password)
        {
            if (password.Length < 4)
                return false;

            var isSequential = true;
            for (int i = 1; i < password.Length && isSequential; i++)
            {
                // 检查是否为连续递增或递减
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
