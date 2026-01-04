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
    /// ClaimTypeRepository class implements the IClaimTypeRepository interface, providing a set of methods for operating on ClaimType entities.
    /// This class receives a DbContext object through dependency injection and uses it to execute database operations.
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
        /// ClaimTypeRepository class implements the IClaimTypeRepository interface, providing a set of methods for operating on ClaimType entities.
        /// This class receives a DbContext object through dependency injection and uses it to execute database operations.
        /// </summary>
        public ClaimTypeRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Asynchronously get a ClaimType entity by the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the ClaimType entity to retrieve.</param>
        /// <returns>Returns the ClaimType entity matching the given ID, or a new ClaimType instance if not found.</returns>
        public async Task<ClaimType> GetAsync(Guid id)
        {
            ClaimTypeEntity entity = await _context.Connection.GetAsync<ClaimTypeEntity>(
                id,
                _context.Transaction
            );
            return entity.ToModel() ?? new ClaimType();
        }

        /// <summary>
        /// Asynchronously get a paged list of ClaimType entities based on the specified model, page number, and items per page.
        /// </summary>
        /// <param name="model">The ClaimTypeModel used to filter query results.</param>
        /// <param name="page">The page number of results to retrieve.</param>
        /// <param name="itemsPerPage">The number of items to display per page.</param>
        /// <returns>Returns a paged list of ClaimType entities matching the criteria, or null if no matches are found.</returns>
        public async Task<PagedData<ClaimType>> GetPagedListAsync(
            ClaimTypeModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedResult<ClaimTypeEntity> result = await _context
                .Connection.Builder<ClaimTypeEntity>(_context.Transaction)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(model.Name),
                    s => s.Name!.Contains(model.Name ?? "")
                )
                .AsPagedListAsync(page, itemsPerPage);

            PagedData<ClaimType> pagedData = new PagedData<ClaimType>
            {
                PageIndex = page,
                PageSize = itemsPerPage,
                TotalCount = result.TotalItems,
                Items = result.Values.ToModels(),
            };

            return pagedData;
        }

        /// <summary>
        /// Asynchronously get all ClaimType entities.
        /// </summary>
        /// <returns>Returns a collection containing all ClaimType entities, or null if no entities are found.</returns>
        public async Task<IEnumerable<ClaimType>> GetAllAsync()
        {
            var list = await _context.Connection.GetAllAsync<ClaimTypeEntity>();
            return list?.ToModels() ?? new List<ClaimType>();
        }

        /// <summary>
        /// Asynchronously insert a new ClaimType entity.
        /// </summary>
        /// <param name="model">The ClaimType entity to insert.</param>
        /// <returns>Returns an integer representing the number of affected rows. Typically 1 if insertion succeeds; may return 0 or throw an exception if insertion fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided model is null.</exception>
        public async ValueTask<int> InsertAsync(ClaimType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ClaimTypeEntity entity = model.ToEntity();
            return await _context.Connection.InsertAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Asynchronously update the specified ClaimType entity.
        /// </summary>
        /// <param name="model">The ClaimType entity to update.</param>
        /// <returns>Returns true if update succeeds; otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided model is null.</exception>
        public async ValueTask<bool> UpdateAsync(ClaimType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ClaimTypeEntity entity = model.ToEntity();
            entity.Updated = DateTime.UtcNow;
            return await _context.Connection.UpdateAsync(entity, _context.Transaction);
        }

        /// <summary>
        /// Asynchronously delete the specified ClaimType record based on the provided ClaimType entity.
        /// </summary>
        /// <param name="model">The ClaimType entity to delete.</param>
        /// <returns>Returns true if deletion succeeds; otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided ClaimType entity is null.</exception>
        public async ValueTask<bool> DeleteAsync(ClaimType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ClaimTypeEntity entity = model.ToEntity();
            return await _context.Connection.DeleteAsync(entity, _context.Transaction);
        }
    }
}
