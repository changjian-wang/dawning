using System;
using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Domain.Models;
using Dawning.Auth.Domain.Models.Administration;

namespace Dawning.Auth.Application.Interfaces.Administration
{
    public interface ISystemMetadataService
    {
        Task<SystemMetadataDto> GetAsync(Guid id);

        Task<PagedData<SystemMetadataDto>> GetPagedListAsync(
            SystemMetadataModel model,
            int page,
            int itemsPerPage
        );

        ValueTask<int> InsertAsync(SystemMetadataDto dto);

        ValueTask<bool> UpdateAsync(SystemMetadataDto dto);

        ValueTask<bool> DeleteAsync(SystemMetadataDto dto);
    }
}
