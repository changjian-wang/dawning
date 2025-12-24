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
    public interface IApplicationService
    {
        Task<ApplicationDto> GetAsync(Guid id);
        Task<ApplicationDto?> GetByClientIdAsync(string clientId);
        Task<PagedData<ApplicationDto>> GetPagedListAsync(
            ApplicationModel model,
            int page,
            int itemsPerPage
        );
        Task<IEnumerable<ApplicationDto>?> GetAllAsync();
        ValueTask<int> InsertAsync(ApplicationDto dto);
        ValueTask<bool> UpdateAsync(ApplicationDto dto);
        ValueTask<bool> DeleteAsync(ApplicationDto dto);
    }
}
