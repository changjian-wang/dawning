using System;
using System.Data;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.Gateway;
using Dawning.Identity.Domain.Interfaces.Monitoring;
using Dawning.Identity.Domain.Interfaces.MultiTenancy;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Repository.Administration;
using Dawning.Identity.Infra.Data.Repository.Gateway;
using Dawning.Identity.Infra.Data.Repository.Monitoring;
using Dawning.Identity.Infra.Data.Repository.MultiTenancy;
using Dawning.Identity.Infra.Data.Repository.OpenIddict;

namespace Dawning.Identity.Infra.Data.Repository.UoW
{
    /// <summary>
    /// Unit of Work implementation
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private bool _disposed = false;

        public UnitOfWork(DbContext context)
        {
            _context = context ?? throw new ArgumentException(nameof(context));

            // All Repositories share one DbContext
            // Administration
            User = new UserRepository(_context);
            ClaimType = new ClaimTypeRepository(_context);
            SystemConfig = new SystemConfigRepository(_context);
            Role = new RoleRepository(_context);
            UserRole = new UserRoleRepository(_context);
            AuditLog = new AuditLogRepository(_context);
            SystemLog = new SystemLogRepository(_context);
            Permission = new PermissionRepository(_context);
            RolePermission = new RolePermissionRepository(_context);
            BackupRecord = new BackupRecordRepository(_context);

            // OpenIddict
            Application = new ApplicationRepository(_context);
            Scope = new ScopeRepository(_context);
            ApiResource = new ApiResourceRepository(_context);
            IdentityResource = new IdentityResourceRepository(_context);
            Authorization = new AuthorizationRepository(_context);
            Token = new TokenRepository(_context);

            // Gateway
            GatewayRoute = new GatewayRouteRepository(_context);
            GatewayCluster = new GatewayClusterRepository(_context);
            RateLimitPolicy = new RateLimitPolicyRepository(_context);
            IpAccessRule = new IpAccessRuleRepository(_context);

            // Monitoring
            AlertRule = new AlertRuleRepository(_context);
            AlertHistory = new AlertHistoryRepository(_context);
            RequestLog = new RequestLogRepository(_context);

            // MultiTenancy
            Tenant = new TenantRepository(_context);
        }

        /// <summary>
        /// Get database connection (for executing SQL directly)
        /// </summary>
        public IDbConnection Connection => _context.Connection;

        // Administration
        public IUserRepository User { get; }
        public IClaimTypeRepository ClaimType { get; }
        public ISystemConfigRepository SystemConfig { get; }
        public IRoleRepository Role { get; }
        public IUserRoleRepository UserRole { get; }
        public IAuditLogRepository AuditLog { get; }
        public ISystemLogRepository SystemLog { get; }
        public IPermissionRepository Permission { get; }
        public IRolePermissionRepository RolePermission { get; }
        public IBackupRecordRepository BackupRecord { get; }

        // OpenIddict
        public IApplicationRepository Application { get; }
        public IScopeRepository Scope { get; }
        public IApiResourceRepository ApiResource { get; }
        public IIdentityResourceRepository IdentityResource { get; }
        public IAuthorizationRepository Authorization { get; }
        public ITokenRepository Token { get; }

        // Gateway
        public IGatewayRouteRepository GatewayRoute { get; }
        public IGatewayClusterRepository GatewayCluster { get; }
        public IRateLimitPolicyRepository RateLimitPolicy { get; }
        public IIpAccessRuleRepository IpAccessRule { get; }

        // Monitoring
        public IAlertRuleRepository AlertRule { get; }
        public IAlertHistoryRepository AlertHistory { get; }
        public IRequestLogRepository RequestLog { get; }

        // MultiTenancy
        public ITenantRepository Tenant { get; }

        public void BeginTransaction()
        {
            _context.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                _context.Commit();
            }
            catch
            {
                _context.Rollback();
            }
        }

        public void Rollback()
        {
            _context.Rollback();
        }

        public Task CommitAsync()
        {
            // Current DbContext only has synchronous methods, wrap as Task simply
            Commit();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _context?.Dispose();
            }

            _disposed = true;
        }
    }
}
