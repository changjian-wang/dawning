using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    /// <summary>
    /// API resource repository interface
    /// </summary>
    public interface IApiResourceRepository
    {
        Task<ApiResource?> GetAsync(Guid id);
        Task<ApiResource?> GetByNameAsync(string name);
        Task<PagedData<ApiResource>> GetPagedListAsync(
            ApiResourceModel model,
            int page,
            int itemsPerPage
        );
        Task<IEnumerable<ApiResource>> GetAllAsync();
        Task<IEnumerable<ApiResource>> GetByNamesAsync(IEnumerable<string> names);
        ValueTask<int> InsertAsync(ApiResource model);
        ValueTask<bool> UpdateAsync(ApiResource model);
        ValueTask<bool> DeleteAsync(ApiResource model);
    }
}
