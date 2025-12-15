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
    public interface IAuthorizationService
    {
        Task<AuthorizationDto> GetAsync(Guid id);
        Task<IEnumerable<AuthorizationDto>?> GetBySubjectAsync(string subject);
        Task<IEnumerable<AuthorizationDto>?> GetByApplicationIdAsync(Guid applicationId);
        Task<PagedData<AuthorizationDto>> GetPagedListAsync(
            AuthorizationModel model,
            int page,
            int itemsPerPage
        );
        Task<IEnumerable<AuthorizationDto>?> GetAllAsync();
        ValueTask<int> InsertAsync(AuthorizationDto dto);
        ValueTask<bool> UpdateAsync(AuthorizationDto dto);
        ValueTask<bool> DeleteAsync(AuthorizationDto dto);
    }
}
