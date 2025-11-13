using Dawning.Auth.Domain.Models;
using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Domain.Models.Administration;

namespace Dawning.Auth.Domain.Interfaces.Administration
{
	public interface IClaimTypeRepository
	{
        Task<ClaimType> GetAsync(Guid id);

        Task<PagedData<ClaimType>> GetPagedListAsync(ClaimTypeModel model, int page, int itemsPerPage);

        Task<IEnumerable<ClaimType>> GetAllAsync();

        ValueTask<int> InsertAsync(ClaimType model);

        ValueTask<bool> UpdateAsync(ClaimType model);

        ValueTask<bool> DeleteAsync(ClaimType model);
    }
}

