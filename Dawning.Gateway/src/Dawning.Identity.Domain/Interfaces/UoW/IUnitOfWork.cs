using System;
using Dawning.Identity.Domain.Interfaces.Administration;

namespace Dawning.Identity.Domain.Interfaces.UoW
{
    public interface IUnitOfWork
    {
        IClaimTypeRepository ClaimType { get; }

        ISystemMetadataRepository SystemMetadata { get; }

        // 添加事务管理方法
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}

