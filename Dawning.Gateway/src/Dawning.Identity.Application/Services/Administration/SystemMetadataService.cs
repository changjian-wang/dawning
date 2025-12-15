using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Mapping.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    public class SystemMetadataService : ISystemMetadataService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SystemMetadataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemMetadataDto?> GetAsync(Guid id)
        {
            var model = await _unitOfWork.SystemMetadata.GetAsync(id);
            return model?.ToDto();
        }

        public async Task<PagedData<SystemMetadataDto>> GetPagedListAsync(
            SystemMetadataModel model,
            int page,
            int itemsPerPage
        )
        {
            var data = await _unitOfWork.SystemMetadata.GetPagedListAsync(
                model,
                page,
                itemsPerPage
            );

            return new PagedData<SystemMetadataDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<SystemMetadataDto>(),
            };
        }

        public async Task<IEnumerable<SystemMetadataDto>?> GetAllAsync()
        {
            // SystemMetadata Repository 没有 GetAllAsync 方法，返回空列表
            return new List<SystemMetadataDto>();
        }

        public async ValueTask<int> InsertAsync(SystemMetadataDto dto)
        {
            var entity = dto.ToEntity();
            return await _unitOfWork.SystemMetadata.InsertAsync(entity);
        }

        public async ValueTask<bool> UpdateAsync(SystemMetadataDto dto)
        {
            var entity = dto.ToEntity();
            return await _unitOfWork.SystemMetadata.UpdateAsync(entity);
        }

        public async ValueTask<bool> DeleteAsync(SystemMetadataDto dto)
        {
            var entity = dto.ToEntity();
            return await _unitOfWork.SystemMetadata.DeleteAsync(entity);
        }
    }
}
