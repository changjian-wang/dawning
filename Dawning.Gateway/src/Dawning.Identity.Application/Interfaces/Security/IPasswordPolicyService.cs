using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Security
{
    /// <summary>
    /// 密码策略服务接口
    /// </summary>
    public interface IPasswordPolicyService
    {
        /// <summary>
        /// 验证密码是否符合策略
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>验证结果</returns>
        Task<PasswordValidationResult> ValidatePasswordAsync(string password);

        /// <summary>
        /// 获取当前密码策略
        /// </summary>
        Task<PasswordPolicy> GetPolicyAsync();
    }

    /// <summary>
    /// 密码策略配置
    /// </summary>
    public class PasswordPolicy
    {
        /// <summary>
        /// 最小长度
        /// </summary>
        public int MinLength { get; set; } = 8;

        /// <summary>
        /// 最大长度
        /// </summary>
        public int MaxLength { get; set; } = 128;

        /// <summary>
        /// 是否要求大写字母
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// 是否要求小写字母
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// 是否要求数字
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// 是否要求特殊字符
        /// </summary>
        public bool RequireSpecialChar { get; set; } = false;

        /// <summary>
        /// 特殊字符集
        /// </summary>
        public string SpecialCharacters { get; set; } = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
    }

    /// <summary>
    /// 密码验证结果
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 错误消息列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static PasswordValidationResult Success() =>
            new PasswordValidationResult { IsValid = true };

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static PasswordValidationResult Failure(params string[] errors) =>
            new PasswordValidationResult { IsValid = false, Errors = errors.ToList() };
    }
}
