using System;

namespace Dawning.Identity.Application.Interfaces
{
    /// <summary>
    /// 当前用户服务接口
    /// 用于在 Application 层获取当前登录用户信息
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// 获取当前用户名
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// 是否已认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        bool IsInRole(string role);
    }
}
