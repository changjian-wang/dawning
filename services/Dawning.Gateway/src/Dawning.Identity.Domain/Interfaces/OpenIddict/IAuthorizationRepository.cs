using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    public interface IAuthorizationRepository
    {
        Task<Authorization> GetAsync(Guid id);
        Task<IEnumerable<Authorization>> GetBySubjectAsync(string subject);
        Task<IEnumerable<Authorization>> GetByApplicationIdAsync(Guid applicationId);
        Task<PagedData<Authorization>> GetPagedListAsync(
            AuthorizationModel model,
            int page,
            int itemsPerPage
        );
        Task<IEnumerable<Authorization>> GetAllAsync();
        ValueTask<int> InsertAsync(Authorization model);
        ValueTask<bool> UpdateAsync(Authorization model);
        ValueTask<bool> DeleteAsync(Authorization model);
    }
}
