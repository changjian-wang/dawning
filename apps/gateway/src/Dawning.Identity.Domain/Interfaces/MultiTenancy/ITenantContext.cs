using System;

namespace Dawning.Identity.Domain.Interfaces.MultiTenancy
{
    /// <summary>
    /// Tenant context interface
    /// Used to get tenant information for the current request
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>
        /// Current tenant ID
        /// </summary>
        Guid? TenantId { get; }

        /// <summary>
        /// Current tenant name
        /// </summary>
        string? TenantName { get; }

        /// <summary>
        /// Whether this is a host tenant (super admin, can access across tenants)
        /// </summary>
        bool IsHost { get; }

        /// <summary>
        /// Set current tenant
        /// </summary>
        void SetTenant(Guid? tenantId, string? tenantName = null, bool isHost = false);
    }
}
