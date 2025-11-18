using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    public interface IScopeRepository
    {
        Task<Scope> GetAsync(Guid id);
        Task<Scope?> GetByNameAsync(string name);
        Task<PagedData<Scope>> GetPagedListAsync(ScopeModel model, int page, int itemsPerPage);
        Task<IEnumerable<Scope>> GetAllAsync();
        Task<IEnumerable<Scope>> GetByNamesAsync(IEnumerable<string> names);
        ValueTask<int> InsertAsync(Scope model);
        ValueTask<bool> UpdateAsync(Scope model);
        ValueTask<bool> DeleteAsync(Scope model);
    }
}
