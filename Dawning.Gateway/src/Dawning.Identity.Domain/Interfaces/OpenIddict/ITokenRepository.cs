using Dawning.Identity.Domain.Aggregates.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Interfaces.OpenIddict
{
    public interface ITokenRepository
    {
        Task<Token?> GetByIdAsync(Guid id);
        Task<Token?> GetByReferenceIdAsync(string referenceId);
        Task<IEnumerable<Token>> GetBySubjectAsync(string subject);
        Task<IEnumerable<Token>> GetByApplicationIdAsync(Guid applicationId);
        Task<IEnumerable<Token>> GetByAuthorizationIdAsync(Guid authorizationId);
        Task<IEnumerable<Token>> GetAllAsync();
        Task<int> InsertAsync(Token token);
        Task<bool> UpdateAsync(Token token);
        Task<bool> DeleteAsync(Guid id);
        Task<long> CountAsync();
        Task<int> PruneExpiredTokensAsync();
    }
}
