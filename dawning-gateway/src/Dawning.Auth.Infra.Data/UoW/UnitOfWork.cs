using Dawning.Auth.Domain.Interfaces;
using Dawning.Auth.Domain.Interfaces.Administration;
using Dawning.Auth.Infra.Data.Context;
using Dawning.Auth.Infra.Data.Repository.Administration;

namespace Dawning.Auth.Infra.Data.UoW
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
            ClaimType = new ClaimTypeRepository(_context);
            SystemMetadata = new SystemMetadataRepository(_context);
        }

        public IClaimTypeRepository ClaimType { get; }

        public ISystemMetadataRepository SystemMetadata { get; }

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
