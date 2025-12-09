using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    public interface ISystemMetadataService
    {
        Task<SystemMetadataDto?> GetAsync(Guid id);

        Task<PagedData<SystemMetadataDto>> GetPagedListAsync(SystemMetadataModel model, int page, int itemsPerPage);

        Task<IEnumerable<SystemMetadataDto>?> GetAllAsync();

        ValueTask<int> InsertAsync(SystemMetadataDto dto);

        ValueTask<bool> UpdateAsync(SystemMetadataDto dto);

        ValueTask<bool> DeleteAsync(SystemMetadataDto dto);
    }
}
