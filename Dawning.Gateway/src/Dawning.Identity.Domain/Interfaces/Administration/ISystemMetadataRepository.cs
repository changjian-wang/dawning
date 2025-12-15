using System;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    public interface ISystemMetadataRepository
    {
        Task<SystemMetadata> GetAsync(Guid id);

        Task<PagedData<SystemMetadata>> GetPagedListAsync(
            SystemMetadataModel model,
            int page,
            int itemsPerPage
        );

        ValueTask<int> InsertAsync(SystemMetadata model);

        ValueTask<bool> UpdateAsync(SystemMetadata model);

        ValueTask<bool> DeleteAsync(SystemMetadata model);
    }
}
