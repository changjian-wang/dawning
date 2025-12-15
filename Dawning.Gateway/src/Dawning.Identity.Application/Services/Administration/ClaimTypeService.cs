using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Mapping.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    public class ClaimTypeService : IClaimTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClaimTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimTypeDto?> GetAsync(Guid id)
        {
            var model = await _unitOfWork.ClaimType.GetAsync(id);
            return model?.ToDto();
        }

        public async Task<PagedData<ClaimTypeDto>> GetPagedListAsync(
            ClaimTypeModel model,
            int page,
            int itemsPerPage
        )
        {
            var data = await _unitOfWork.ClaimType.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<ClaimTypeDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<ClaimTypeDto>(),
            };
        }

        public async Task<IEnumerable<ClaimTypeDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.ClaimType.GetAllAsync();
            return list?.ToDtos();
        }

        public async ValueTask<int> InsertAsync(ClaimTypeDto dto)
        {
            var entity = dto.ToEntity();
            return await _unitOfWork.ClaimType.InsertAsync(entity);
        }

        public async ValueTask<bool> UpdateAsync(ClaimTypeDto dto)
        {
            var entity = dto.ToEntity();
            return await _unitOfWork.ClaimType.UpdateAsync(entity);
        }

        public async ValueTask<bool> DeleteAsync(ClaimTypeDto dto)
        {
            var entity = dto.ToEntity();
            return await _unitOfWork.ClaimType.DeleteAsync(entity);
        }
    }
}
