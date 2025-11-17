using System;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Infra.Data.Context;

namespace Dawning.Identity.Infra.Data.Repository.Administration
{
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
    }
}

