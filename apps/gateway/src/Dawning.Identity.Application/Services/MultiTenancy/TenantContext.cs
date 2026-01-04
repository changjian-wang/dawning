using System;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;

namespace Dawning.Identity.Application.Services.MultiTenancy
{
    /// <summary>
    /// Tenant context implementation.
    /// Uses AsyncLocal to store current tenant information, supporting async context propagation.
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private static readonly AsyncLocal<TenantInfo> _currentTenant = new();

        /// <inheritdoc/>
        public Guid? TenantId => _currentTenant.Value?.TenantId;

        /// <inheritdoc/>
        public string? TenantName => _currentTenant.Value?.TenantName;

        /// <inheritdoc/>
        public bool IsHost => _currentTenant.Value?.IsHost ?? false;

        /// <inheritdoc/>
        public void SetTenant(Guid? tenantId, string? tenantName = null, bool isHost = false)
        {
            _currentTenant.Value = new TenantInfo
            {
                TenantId = tenantId,
                TenantName = tenantName,
                IsHost = isHost,
            };
        }

        /// <summary>
        /// Tenant information inner class
        /// </summary>
        private class TenantInfo
        {
            public Guid? TenantId { get; set; }
            public string? TenantName { get; set; }
            public bool IsHost { get; set; }
        }
    }
}
