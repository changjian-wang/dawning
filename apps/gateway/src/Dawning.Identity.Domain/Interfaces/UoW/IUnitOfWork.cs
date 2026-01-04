using System;
using System.Data;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Dawning.Identity.Domain.Interfaces.Monitoring;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.OpenIddict;

namespace Dawning.Identity.Domain.Interfaces.UoW
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Get database connection (for direct SQL execution)
        /// </summary>
        IDbConnection Connection { get; }

        // Administration
        IUserRepository User { get; }
        IClaimTypeRepository ClaimType { get; }
        ISystemConfigRepository SystemConfig { get; }
        IRoleRepository Role { get; }
        IUserRoleRepository UserRole { get; }
        IAuditLogRepository AuditLog { get; }
        ISystemLogRepository SystemLog { get; }
        IPermissionRepository Permission { get; }
        IRolePermissionRepository RolePermission { get; }
        IBackupRecordRepository BackupRecord { get; }

        // OpenIddict
        IApplicationRepository Application { get; }
        IScopeRepository Scope { get; }
        IApiResourceRepository ApiResource { get; }
        IIdentityResourceRepository IdentityResource { get; }
        IAuthorizationRepository Authorization { get; }
        ITokenRepository Token { get; }

        // Gateway
        IGatewayRouteRepository GatewayRoute { get; }
        IGatewayClusterRepository GatewayCluster { get; }
        IRateLimitPolicyRepository RateLimitPolicy { get; }
        IIpAccessRuleRepository IpAccessRule { get; }

        // Monitoring
        IAlertRuleRepository AlertRule { get; }
        IAlertHistoryRepository AlertHistory { get; }
        IRequestLogRepository RequestLog { get; }

        // MultiTenancy
        ITenantRepository Tenant { get; }

        // Transaction management methods
        void BeginTransaction();
        void Commit();
        void Rollback();
        Task CommitAsync();
    }
}
