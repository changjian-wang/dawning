using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Repository.Administration;
using Dawning.Identity.Infra.Data.Repository.OpenIddict;
using System;

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

            // 所有Repository共享一个DbContext
            // Administration
            ClaimType = new ClaimTypeRepository(_context);
            SystemMetadata = new SystemMetadataRepository(_context);

            // OpenIddict
            Application = new ApplicationRepository(_context);
            Scope = new ScopeRepository(_context);
            Authorization = new AuthorizationRepository(_context);
            Token = new TokenRepository(_context);
        }

        // Administration
        public IClaimTypeRepository ClaimType { get; }
        public ISystemMetadataRepository SystemMetadata { get; }

        // OpenIddict
        public IApplicationRepository Application { get; }
        public IScopeRepository Scope { get; }
        public IAuthorizationRepository Authorization { get; }
        public ITokenRepository Token { get; }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _context?.Dispose();
            }

            _disposed = true;
        }
    }
}

