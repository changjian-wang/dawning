using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Domain.Models;
using Dawning.Auth.Domain.Models.Administration;

namespace Dawning.Auth.Domain.Interfaces.Administration
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
