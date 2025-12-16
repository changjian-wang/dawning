using System;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Dawning.Identity.Domain.Interfaces.OpenIddict;

namespace Dawning.Identity.Domain.Interfaces.UoW
{
    public interface IUnitOfWork
    {
        // Administration
        IClaimTypeRepository ClaimType { get; }
        ISystemMetadataRepository SystemMetadata { get; }
        IRoleRepository Role { get; }
        IUserRoleRepository UserRole { get; }
        IAuditLogRepository AuditLog { get; }
        ISystemLogRepository SystemLog { get; }
        IPermissionRepository Permission { get; }
        IRolePermissionRepository RolePermission { get; }

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

        // 添加事务管理方法
        void BeginTransaction();
        void Commit();
        void Rollback();
        Task CommitAsync();
    }
}
