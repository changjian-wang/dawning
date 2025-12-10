using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    /// <summary>
    /// 身份资源仓储接口
    /// </summary>
    public interface IIdentityResourceRepository
    {
        Task<IdentityResource?> GetAsync(Guid id);
        Task<IdentityResource?> GetByNameAsync(string name);
        Task<PagedData<IdentityResource>> GetPagedListAsync(IdentityResourceModel model, int page, int itemsPerPage);
        Task<IEnumerable<IdentityResource>> GetAllAsync();
        Task<IEnumerable<IdentityResource>> GetByNamesAsync(IEnumerable<string> names);
        ValueTask<int> InsertAsync(IdentityResource model);
        ValueTask<bool> UpdateAsync(IdentityResource model);
        ValueTask<bool> DeleteAsync(IdentityResource model);
    }
}
