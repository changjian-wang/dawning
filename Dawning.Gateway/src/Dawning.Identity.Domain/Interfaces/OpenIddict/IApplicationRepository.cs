using System;
using Dawning.Identity.Domain.Aggregates.OpenIddict;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
	public interface IApplicationRepository
	{
        Task<Application?> GetByIdAsync(Guid id);
        Task<Application?> GetByClientIdAsync(string clientId);
        Task<IEnumerable<Application>> GetAllAsync();
        Task<int> InsertAsync(Application application);
        Task<bool> UpdateAsync(Application application);
        Task<bool> DeleteAsync(Guid id);
        Task<long> CountAsync();
    }
}

