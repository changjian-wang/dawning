using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Application.Interfaces.OpenIddict
{
    public interface ITokenService
    {
        Task<TokenDto> GetAsync(Guid id);
        Task<TokenDto?> GetByReferenceIdAsync(string referenceId);
        Task<IEnumerable<TokenDto>?> GetBySubjectAsync(string subject);
        Task<IEnumerable<TokenDto>?> GetByApplicationIdAsync(Guid applicationId);
        Task<IEnumerable<TokenDto>?> GetByAuthorizationIdAsync(Guid authorizationId);
        Task<PagedData<TokenDto>> GetPagedListAsync(TokenModel model, int page, int itemsPerPage);
        Task<IEnumerable<TokenDto>?> GetAllAsync();
        ValueTask<int> InsertAsync(TokenDto dto);
        ValueTask<bool> UpdateAsync(TokenDto dto);
        ValueTask<bool> DeleteAsync(TokenDto dto);
        Task<int> PruneExpiredTokensAsync();
    }
}
