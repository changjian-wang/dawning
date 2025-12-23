using System;

namespace Dawning.Identity.Domain.Interfaces.MultiTenancy
{
    /// <summary>
    /// 租户上下文接口
    /// 用于获取当前请求的租户信息
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>
        /// 当前租户ID
        /// </summary>
        Guid? TenantId { get; }

        /// <summary>
        /// 当前租户名称
        /// </summary>
        string? TenantName { get; }

        /// <summary>
        /// 是否为主机租户（超级管理员，可以跨租户访问）
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// 设置当前租户
        /// </summary>
        void SetTenant(Guid? tenantId, string? tenantName = null, bool isHost = false);
    }
}
