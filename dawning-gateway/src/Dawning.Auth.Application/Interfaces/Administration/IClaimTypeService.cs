using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Domain.Models;
using Dawning.Auth.Domain.Models.Administration;

namespace Dawning.Auth.Application.Interfaces.Administration
{
	public interface IClaimTypeService
	{
        Task<ClaimTypeDto> GetAsync(Guid id);

        Task<PagedData<ClaimTypeDto>> GetPagedListAsync(ClaimTypeModel model, int page, int itemsPerPage);

        Task<IEnumerable<ClaimTypeDto>?> GetAllAsync();

        ValueTask<int> InsertAsync(ClaimTypeDto dto);

        ValueTask<bool> UpdateAsync(ClaimTypeDto dto);

        ValueTask<bool> DeleteAsync(ClaimTypeDto dto);
    }
}

