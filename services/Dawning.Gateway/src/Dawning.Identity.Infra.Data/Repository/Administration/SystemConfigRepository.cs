using System;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.ORM.Dapper;
using static Dawning.ORM.Dapper.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// Represents a repository for managing system config, providing methods to retrieve, insert, update, and delete system config records.
    /// </summary>
    public class SystemConfigRepository : ISystemConfigRepository
    {
        /// <summary>
        /// Represents the database context used within the SystemConfigRepository to interact with the underlying data store.
        /// This context provides access to the database connection and facilitates operations such as querying, inserting, updating, and deleting system config records.
        /// </summary>
        private readonly DbContext _context;

        /// <summary>
        /// Represents a repository for managing system config, providing methods to retrieve, insert, update, and delete system config records. Implements the ISystemConfigRepository interface.
        /// </summary>
        public SystemConfigRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously retrieves a specific system config by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the system config to retrieve.</param>
        /// <returns>A SystemConfigAggregate object representing the retrieved system config, or null if not found.</returns>
        public async Task<SystemConfigAggregate> GetAsync(Guid id)
        {
            SystemConfigEntity entity = await _context.Connection.GetAsync<SystemConfigEntity>(
                id
            );
            return entity.ToModel() ?? new SystemConfigAggregate();
        }

        /// <summary>
        /// Retrieves a paged list of system config based on the provided model, page number, and items per page.
        /// </summary>
        /// <param name="model">The filter model to apply for retrieving system config.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="itemsPerPage">The number of items per page.</param>
        /// <returns>An enumerable of SystemConfigAggregate representing the paged list of results.</returns>
        public async Task<PagedData<SystemConfigAggregate>> GetPagedListAsync(
            SystemConfigModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedResult<SystemConfigEntity> result = await _context
                .Connection.Builder<SystemConfigEntity>()
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    s => s.Name!.Contains(model.Name ?? "")
                )
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<SystemConfigAggregate> pagedData = new PagedData<SystemConfigAggregate>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };

            return pagedData;
        }

        /// <summary>
        /// Asynchronously inserts a new system config record into the database.
        /// </summary>
        /// <param name="model">The SystemConfigAggregate object to be inserted.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the model is null.</exception>
        public async ValueTask<int> InsertAsync(SystemConfigAggregate model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            SystemConfigEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity);
        }

        /// <summary>
        /// Asynchronously updates an existing system config with the provided details.
        /// </summary>
        /// <param name="model">The SystemConfigAggregate object containing the updated information.</param>
        /// <returns>A boolean value indicating whether the update operation was successful.</returns>
        public async ValueTask<bool> UpdateAsync(SystemConfigAggregate model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            SystemConfigEntity entity = model.ToEntity();
            entity.Updated = DateTime.UtcNow;
            return await _context.Connection.UpdateAsync(entity);
        }

        /// <summary>
        /// Asynchronously deletes the specified system config.
        /// </summary>
        /// <param name="model">The SystemConfigAggregate object to delete.</param>
        /// <returns>A boolean value indicating whether the deletion was successful.</returns>
        public async ValueTask<bool> DeleteAsync(SystemConfigAggregate model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            SystemConfigEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity);
        }
    }
}
