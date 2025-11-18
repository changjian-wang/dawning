using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using System;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    public interface IApplicationRepository
    {
        Task<Application> GetAsync(Guid id);
        Task<Application?> GetByClientIdAsync(string clientId);
        Task<PagedData<Application>> GetPagedListAsync(ApplicationModel model, int page, int itemsPerPage);
        Task<IEnumerable<Application>> GetAllAsync();
        ValueTask<int> InsertAsync(Application model);
        ValueTask<bool> UpdateAsync(Application model);
        ValueTask<bool> DeleteAsync(Application model);
    }
}

