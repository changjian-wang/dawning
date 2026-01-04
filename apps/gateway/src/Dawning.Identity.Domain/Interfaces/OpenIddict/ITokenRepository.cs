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
    public interface ITokenRepository
    {
        Task<Token> GetAsync(Guid id);
        Task<Token?> GetByReferenceIdAsync(string referenceId);
        Task<IEnumerable<Token>> GetBySubjectAsync(string subject);
        Task<IEnumerable<Token>> GetByApplicationIdAsync(Guid applicationId);
        Task<IEnumerable<Token>> GetByAuthorizationIdAsync(Guid authorizationId);
        Task<PagedData<Token>> GetPagedListAsync(TokenModel model, int page, int itemsPerPage);
        Task<IEnumerable<Token>> GetAllAsync();
        ValueTask<int> InsertAsync(Token model);
        ValueTask<bool> UpdateAsync(Token model);
        ValueTask<bool> DeleteAsync(Token model);
        Task<int> PruneExpiredTokensAsync();

        /// <summary>
        /// Get valid tokens for a user
        /// </summary>
        Task<IEnumerable<Token>> GetValidTokensBySubjectAsync(string subject);

        /// <summary>
        /// Revoke all valid tokens for a user
        /// </summary>
        Task<int> RevokeAllBySubjectAsync(string subject);

        /// <summary>
        /// Revoke a specific token
        /// </summary>
        Task<bool> RevokeByIdAsync(Guid tokenId);
    }
}
