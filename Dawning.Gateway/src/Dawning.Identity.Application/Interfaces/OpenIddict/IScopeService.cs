using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.OpenIddict
{
    public interface IScopeService
    {
        Task<ScopeDto> GetAsync(Guid id);
        Task<ScopeDto?> GetByNameAsync(string name);
        Task<PagedData<ScopeDto>> GetPagedListAsync(ScopeModel model, int page, int itemsPerPage);
        Task<IEnumerable<ScopeDto>?> GetAllAsync();
        Task<IEnumerable<ScopeDto>?> GetByNamesAsync(IEnumerable<string> names);
        ValueTask<int> InsertAsync(ScopeDto dto);
        ValueTask<bool> UpdateAsync(ScopeDto dto);
        ValueTask<bool> DeleteAsync(ScopeDto dto);
    }
}
