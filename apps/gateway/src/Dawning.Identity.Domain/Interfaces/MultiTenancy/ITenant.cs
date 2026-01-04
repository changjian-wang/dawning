using System;

namespace Dawning.Identity.Domain.Interfaces.MultiTenancy
{
    /// <summary>
    /// Multi-tenant entity interface
    /// Entities implementing this interface will automatically have tenant data isolation
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// Tenant ID
        /// </summary>
        Guid? TenantId { get; set; }
    }
}
