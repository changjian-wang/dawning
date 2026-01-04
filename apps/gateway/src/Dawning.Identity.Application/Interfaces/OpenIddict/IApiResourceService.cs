using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Application.Interfaces.OpenIddict
{
    /// <summary>
    /// API Resource Service Interface
    /// </summary>
    public interface IApiResourceService
    {
        Task<ApiResourceDto> GetAsync(Guid id);
        Task<ApiResourceDto?> GetByNameAsync(string name);
        Task<PagedData<ApiResourceDto>> GetPagedListAsync(
            ApiResourceModel model,
            int page,
            int itemsPerPage
        );
        Task<IEnumerable<ApiResourceDto>?> GetAllAsync();
        Task<IEnumerable<ApiResourceDto>?> GetByNamesAsync(IEnumerable<string> names);
        ValueTask<int> InsertAsync(ApiResourceDto dto);
        ValueTask<bool> UpdateAsync(ApiResourceDto dto);
        ValueTask<bool> DeleteAsync(ApiResourceDto dto);
    }
}
