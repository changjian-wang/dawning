using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using System;

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

        // OpenIddict
        IApplicationRepository Application { get; }
        IScopeRepository Scope { get; }
        IAuthorizationRepository Authorization { get; }
        ITokenRepository Token { get; }

        // 添加事务管理方法
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}

