using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Application.Interfaces.OpenIddict
{
    /// <summary>
    /// 身份资源服务接口
    /// </summary>
    public interface IIdentityResourceService
    {
        Task<IdentityResourceDto> GetAsync(Guid id);
        Task<IdentityResourceDto?> GetByNameAsync(string name);
        Task<PagedData<IdentityResourceDto>> GetPagedListAsync(
            IdentityResourceModel model,
            int page,
            int itemsPerPage
        );
        Task<IEnumerable<IdentityResourceDto>?> GetAllAsync();
        Task<IEnumerable<IdentityResourceDto>?> GetByNamesAsync(IEnumerable<string> names);
        ValueTask<int> InsertAsync(IdentityResourceDto dto);
        ValueTask<bool> UpdateAsync(IdentityResourceDto dto);
        ValueTask<bool> DeleteAsync(IdentityResourceDto dto);
    }
}
