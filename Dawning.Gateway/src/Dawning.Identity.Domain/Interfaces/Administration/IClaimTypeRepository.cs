using System;
using System.Security.Claims;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Domain.Interfaces.Administration
{
    public interface IClaimTypeRepository
    {
        Task<ClaimType> GetAsync(Guid id);

        Task<PagedData<ClaimType>> GetPagedListAsync(
            ClaimTypeModel model,
            int page,
            int itemsPerPage
        );

        Task<IEnumerable<ClaimType>> GetAllAsync();

        ValueTask<int> InsertAsync(ClaimType model);

        ValueTask<bool> UpdateAsync(ClaimType model);

        ValueTask<bool> DeleteAsync(ClaimType model);
    }
}
