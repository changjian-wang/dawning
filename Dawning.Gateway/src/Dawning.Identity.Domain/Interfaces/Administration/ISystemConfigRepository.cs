using System;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    public interface ISystemConfigRepository
    {
        Task<SystemConfigAggregate> GetAsync(Guid id);

        Task<PagedData<SystemConfigAggregate>> GetPagedListAsync(
            SystemConfigModel model,
            int page,
            int itemsPerPage
        );

        ValueTask<int> InsertAsync(SystemConfigAggregate model);

        ValueTask<bool> UpdateAsync(SystemConfigAggregate model);

        ValueTask<bool> DeleteAsync(SystemConfigAggregate model);
    }
}
