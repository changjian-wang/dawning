using System;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.Shared.Dapper.Contrib;
using static Dawning.Shared.Dapper.Contrib.SqlMapperExtensions;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
    /// <summary>
    /// Represents a repository for managing system metadata, providing methods to retrieve, insert, update, and delete system metadata records.
    /// </summary>
    public class SystemMetadataRepository : ISystemMetadataRepository
    {
        /// <summary>
        /// Represents the database context used within the SystemMetadataRepository to interact with the underlying data store.
        /// This context provides access to the database connection and facilitates operations such as querying, inserting, updating, and deleting system metadata records.
        /// </summary>
        private readonly DbContext _context;

        /// <summary>
        /// Represents a repository for managing system metadata, providing methods to retrieve, insert, update, and delete system metadata records. Implements the ISystemMetadataRepository interface.
        /// </summary>
        public SystemMetadataRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously retrieves a specific system metadata by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the system metadata to retrieve.</param>
        /// <returns>A SystemMetadata object representing the retrieved system metadata, or null if not found.</returns>
        public async Task<SystemMetadata> GetAsync(Guid id)
        {
            SystemMetadataEntity entity = await _context.Connection.GetAsync<SystemMetadataEntity>(
                id
            );
            return entity.ToModel() ?? new SystemMetadata();
        }

        /// <summary>
        /// Retrieves a paged list of system metadata based on the provided model, page number, and items per page.
        /// </summary>
        /// <param name="model">The filter model to apply for retrieving system metadata.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="itemsPerPage">The number of items per page.</param>
        /// <returns>An enumerable of SystemMetadata representing the paged list of results.</returns>
        public async Task<PagedData<SystemMetadata>> GetPagedListAsync(
            SystemMetadataModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedResult<SystemMetadataEntity> result = await _context
                .Connection.Builder<SystemMetadataEntity>()
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    s => s.Name!.Contains(model.Name ?? "")
                )
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<SystemMetadata> pagedData = new PagedData<SystemMetadata>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };

            return pagedData;
        }

        /// <summary>
        /// Asynchronously inserts a new system metadata record into the database.
        /// </summary>
        /// <param name="model">The SystemMetadata object to be inserted.</param>
        /// <returns>The number of rows affected by the insert operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the model is null.</exception>
        public async ValueTask<int> InsertAsync(SystemMetadata model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            SystemMetadataEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity);
        }

        /// <summary>
        /// Asynchronously updates an existing system metadata with the provided details.
        /// </summary>
        /// <param name="model">The SystemMetadata object containing the updated information.</param>
        /// <returns>A boolean value indicating whether the update operation was successful.</returns>
        public async ValueTask<bool> UpdateAsync(SystemMetadata model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            SystemMetadataEntity entity = model.ToEntity();
            entity.Updated = DateTime.UtcNow;
            return await _context.Connection.UpdateAsync(entity);
        }

        /// <summary>
        /// Asynchronously deletes the specified system metadata.
        /// </summary>
        /// <param name="model">The SystemMetadata object to delete.</param>
        /// <returns>A boolean value indicating whether the deletion was successful.</returns>
        public async ValueTask<bool> DeleteAsync(SystemMetadata model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            SystemMetadataEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity);
        }
    }
}
