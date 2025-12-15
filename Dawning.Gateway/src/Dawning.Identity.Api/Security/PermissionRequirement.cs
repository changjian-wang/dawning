using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Api.Security
{
    /// <summary>
    /// 权限需求，用于定义需要验证的权限
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 需要的权限代码
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// 创建权限需求
        /// </summary>
        /// <param name="permission">权限代码</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
