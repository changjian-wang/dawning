using System;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Infra.Data.Context;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
	public class ClaimTypeRepository : IClaimTypeRepository
    {
        /// <summary>
        /// Represents the database context used for interacting with the underlying data storage.
        /// This context is essential for performing CRUD (Create, Read, Update, Delete) operations
        /// on entities within the application. It provides a connection to the database and enables
        /// transaction management, ensuring that all changes are committed or rolled back as a unit.
        /// </summary>
        private readonly DbContext _context;

        /// <summary>
        /// ClaimTypeRepository 类实现了 IClaimTypeRepository 接口，提供了一系列用于操作 ClaimType 实体的方法。
        /// 该类通过依赖注入接收一个 DbContext 对象，使用它来执行数据库操作。
        /// </summary>
        public ClaimTypeRepository(DbContext context)
        {
            _context = context;
        }
    }
}

