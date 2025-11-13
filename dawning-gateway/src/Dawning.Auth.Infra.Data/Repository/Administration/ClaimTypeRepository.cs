using Dawning.Auth.Dapper.Contrib;
using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Domain.Interfaces.Administration;
using Dawning.Auth.Domain.Models;
using Dawning.Auth.Domain.Models.Administration;
using Dawning.Auth.Infra.Data.Context;
using Dawning.Auth.Infra.Data.Mapping.Administration;
using Dawning.Auth.Infra.Data.PersistentObjects.Administration;
using static Dawning.Auth.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Auth.Infra.Data.Repository.Administration
{
    /// <summary>
    /// ClaimTypeRepository 类实现了 IClaimTypeRepository 接口，提供了一系列用于操作 ClaimType 实体的方法。
    /// 该类通过依赖注入接收一个 DbContext 对象，使用它来执行数据库操作。
    /// </summary>
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

        /// <summary>
        /// 异步获取指定ID的ClaimType实体。
        /// </summary>
        /// <param name="id">要检索的ClaimType实体的唯一标识符。</param>
        /// <returns>返回与给定ID匹配的ClaimType实体，如果没有找到则返回一个新的ClaimType实例。</returns>
        public async Task<ClaimType> GetAsync(Guid id)
        {
            ClaimTypeEntity entity = await _context.Connection.GetAsync<ClaimTypeEntity>(id);
            return entity.ToModel() ?? new ClaimType();
        }

        /// <summary>
        /// 异步获取指定模型、页码和每页项数的ClaimType实体分页列表。
        /// </summary>
        /// <param name="model">用于过滤查询结果的ClaimTypeModel。</param>
        /// <param name="page">要检索的结果页码。</param>
        /// <param name="itemsPerPage">每页显示的项数。</param>
        /// <returns>返回符合条件的ClaimType实体分页列表，如果没有找到匹配项则返回null。</returns>
        public async Task<PagedData<ClaimType>> GetPagedListAsync(ClaimTypeModel model, int page, int itemsPerPage)
        {
            PagedResult<ClaimTypeEntity> result = await _context.Connection.Builder<ClaimTypeEntity>(model)
                .WhereIf(!string.IsNullOrWhiteSpace(model.Name), s => s.Name!.Contains(model.Name ?? ""))
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<ClaimType> pagedData = new PagedData<ClaimType>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels()
            };
            
            return pagedData;
        }

        /// <summary>
        /// 异步获取所有ClaimType实体。
        /// </summary>
        /// <returns>返回包含所有ClaimType实体的集合，如果未找到任何实体，则返回null。</returns>
        public async Task<IEnumerable<ClaimType>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<ClaimTypeEntity>();
            return list?.ToModels() ?? new List<ClaimType>();
        }

        /// <summary>
        /// 异步插入一个新的ClaimType实体。
        /// </summary>
        /// <param name="model">要插入的ClaimType实体。</param>
        /// <returns>返回一个整数，表示受影响的行数。如果插入成功，则通常为1；如果插入失败，则可能返回0或抛出异常。</returns>
        /// <exception cref="ArgumentNullException">当提供的model为null时抛出。</exception>
        public async ValueTask<int> InsertAsync(ClaimType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ClaimTypeEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity);
        }

        /// <summary>
        /// 异步更新指定的ClaimType实体。
        /// </summary>
        /// <param name="model">要更新的ClaimType实体。</param>
        /// <returns>如果更新成功，则返回true；否则返回false。</returns>
        /// <exception cref="ArgumentNullException">当提供的model为null时抛出。</exception>
        public async ValueTask<bool> UpdateAsync(ClaimType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            ClaimTypeEntity entity = model.ToEntity();
            entity.Updated = DateTime.UtcNow;
            return await _context.Connection.UpdateAsync(entity);
        }

        /// <summary>
        /// 根据提供的ClaimType实体异步删除指定的ClaimType记录。
        /// </summary>
        /// <param name="model">要删除的ClaimType实体。</param>
        /// <returns>如果删除成功，则返回true；否则返回false。</returns>
        /// <exception cref="ArgumentNullException">当提供的ClaimType实体为null时抛出。</exception>
        public async ValueTask<bool> DeleteAsync(ClaimType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            ClaimTypeEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity);
        }
    }
}

