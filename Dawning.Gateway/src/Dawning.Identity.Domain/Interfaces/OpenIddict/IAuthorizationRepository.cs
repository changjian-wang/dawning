using Dawning.Identity.Domain.Aggregates.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    public interface IAuthorizationRepository
    {
        Task<Authorization?> GetByIdAsync(Guid id);
        Task<IEnumerable<Authorization>> GetBySubjectAsync(string subject);
        Task<IEnumerable<Authorization>> GetByApplicationIdAsync(Guid applicationId);
        Task<IEnumerable<Authorization>> GetAllAsync();
        Task<int> InsertAsync(Authorization authorization);
        Task<bool> UpdateAsync(Authorization authorization);
        Task<bool> DeleteAsync(Guid id);
        Task<long> CountAsync();
    }
}
