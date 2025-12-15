using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Api.Security
{
    /// <summary>
    /// 权限验证特性，用于标记需要特定权限的API端点
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 权限代码前缀
        /// </summary>
        public const string PolicyPrefix = "Permission:";

        /// <summary>
        /// 权限代码
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// 创建权限验证特性
        /// </summary>
        /// <param name="permission">权限代码，如 "user.create", "role.delete"</param>
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
            Policy = $"{PolicyPrefix}{permission}";
        }
    }
}
