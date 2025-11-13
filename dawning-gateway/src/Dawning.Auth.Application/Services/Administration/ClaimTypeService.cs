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
    public class ClaimTypeService : IClaimTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        /// <summary>
        /// Service for managing claim types, implementing the IClaimTypeService interface.
        /// Provides methods to interact with claim type data, including operations like
        /// retrieving, inserting, updating, and deleting claim types.
        /// </summary>
        public ClaimTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async ValueTask<bool> DeleteAsync(ClaimTypeDto dto)
        {
            ClaimType model = dto?.ToModel() ?? new ClaimType();
            return await _unitOfWork.ClaimType.DeleteAsync(model);
        }

        public async Task<ClaimTypeDto> GetAsync(Guid id)
        {
            ClaimType model = await _unitOfWork.ClaimType.GetAsync(id);
            return model?.ToDto() ?? new ClaimTypeDto();
        }

        public async Task<PagedData<ClaimTypeDto>> GetPagedListAsync(
            ClaimTypeModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedData<ClaimType> data = await _unitOfWork.ClaimType.GetPagedListAsync(
                model,
                page,
                itemsPerPage
            );
            PagedData<ClaimTypeDto> pageResult = new PagedData<ClaimTypeDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<ClaimTypeDto>(),
            };

            return pageResult;
        }

        public async Task<IEnumerable<ClaimTypeDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.ClaimType.GetAllAsync();
            return list?.ToDtos() ?? new List<ClaimTypeDto>();
        }

        public async ValueTask<int> InsertAsync(ClaimTypeDto dto)
        {
            ClaimType model = _mapper.Map<ClaimType>(dto);
            return await _unitOfWork.ClaimType.InsertAsync(model);
        }

        public async ValueTask<bool> UpdateAsync(ClaimTypeDto dto)
        {
            ClaimType model = _mapper.Map<ClaimType>(dto);
            return await _unitOfWork.ClaimType.UpdateAsync(model);
        }
    }
}
