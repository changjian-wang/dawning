using System;

namespace Dawning.Identity.Domain.Interfaces.MultiTenancy
{
    /// <summary>
    /// 多租户实体接口
    /// 实现此接口的实体将自动进行租户数据隔离
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        Guid? TenantId { get; set; }
    }
}
