using AutoMapper;
using Dawning.Auth.Application.Dtos.Administration;
using Dawning.Auth.Application.Interfaces.Administration;
using Dawning.Auth.Application.Mapping.Administration;
using Dawning.Auth.Domain.Aggregates.Administration;
using Dawning.Auth.Domain.Interfaces;
using Dawning.Auth.Domain.Models;
using Dawning.Auth.Domain.Models.Administration;

namespace Dawning.Auth.Application.Services.Administration
{
    public class SystemMetadataService : ISystemMetadataService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        /// <summary>
        /// Service for managing claim types, implementing the ISystemMetadataService interface.
        /// Provides methods to interact with claim type data, including operations like
        /// retrieving, inserting, updating, and deleting claim types.
        /// </summary>
        public SystemMetadataService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async ValueTask<bool> DeleteAsync(SystemMetadataDto dto)
        {
            SystemMetadata model = dto?.ToModel() ?? new SystemMetadata();
            return await _unitOfWork.SystemMetadata.DeleteAsync(model);
        }

        public async Task<SystemMetadataDto> GetAsync(Guid id)
        {
            SystemMetadata model = await _unitOfWork.SystemMetadata.GetAsync(id);
            return model?.ToDto() ?? new SystemMetadataDto();
        }

        public async Task<PagedData<SystemMetadataDto>> GetPagedListAsync(
            SystemMetadataModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedData<SystemMetadata> data = await _unitOfWork.SystemMetadata.GetPagedListAsync(
                model,
                page,
                itemsPerPage
            );
            PagedData<SystemMetadataDto> pageResult = new PagedData<SystemMetadataDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<SystemMetadataDto>(),
            };

            return pageResult;
        }

        public async ValueTask<int> InsertAsync(SystemMetadataDto dto)
        {
            SystemMetadata model = _mapper.Map<SystemMetadata>(dto);
            return await _unitOfWork.SystemMetadata.InsertAsync(model);
        }

        public async ValueTask<bool> UpdateAsync(SystemMetadataDto dto)
        {
            SystemMetadata model = _mapper.Map<SystemMetadata>(dto);
            return await _unitOfWork.SystemMetadata.UpdateAsync(model);
        }
    }
}
