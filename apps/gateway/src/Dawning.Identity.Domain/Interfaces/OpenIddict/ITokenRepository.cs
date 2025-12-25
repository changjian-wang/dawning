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
        /// 获取用户的有效令牌列表
        /// </summary>
        Task<IEnumerable<Token>> GetValidTokensBySubjectAsync(string subject);

        /// <summary>
        /// 撤销用户的所有有效令牌
        /// </summary>
        Task<int> RevokeAllBySubjectAsync(string subject);

        /// <summary>
        /// 撤销指定令牌
        /// </summary>
        Task<bool> RevokeByIdAsync(Guid tokenId);
    }
}
