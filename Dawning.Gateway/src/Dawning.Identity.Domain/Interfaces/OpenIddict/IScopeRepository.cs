using Dawning.Identity.Domain.Aggregates.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    public interface IScopeRepository
    {
        Task<Scope?> GetByIdAsync(Guid id);
        Task<Scope?> GetByNameAsync(string name);
        Task<IEnumerable<Scope>> GetAllAsync();
        Task<IEnumerable<Scope>> GetByNamesAsync(IEnumerable<string> names);
        Task<int> InsertAsync(Scope scope);
        Task<bool> UpdateAsync(Scope scope);
        Task<bool> DeleteAsync(Guid id);
        Task<long> CountAsync();
    }
}
