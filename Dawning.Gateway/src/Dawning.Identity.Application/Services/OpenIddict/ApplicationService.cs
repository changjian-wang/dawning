using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Application.Mapping.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Services.OpenIddict
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Service for managing OpenIddict applications
        /// </summary>
        public ApplicationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApplicationDto> GetAsync(Guid id)
        {
            Domain.Aggregates.OpenIddict.Application model = await _unitOfWork.Application.GetAsync(id);
            return model?.ToDto() ?? new ApplicationDto();
        }

        public async Task<ApplicationDto?> GetByClientIdAsync(string clientId)
        {
            Domain.Aggregates.OpenIddict.Application? model = await _unitOfWork.Application.GetByClientIdAsync(clientId);
            return model?.ToDto();
        }

        public async Task<PagedData<ApplicationDto>> GetPagedListAsync(
            ApplicationModel model,
            int page,
            int itemsPerPage)
        {
            PagedData<Domain.Aggregates.OpenIddict.Application> data = await _unitOfWork.Application.GetPagedListAsync(
                model,
                page,
                itemsPerPage);

            PagedData<ApplicationDto> pageResult = new PagedData<ApplicationDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<ApplicationDto>()
            };

            return pageResult;
        }

        public async Task<IEnumerable<ApplicationDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.Application.GetAllAsync();
            return list?.ToDtos() ?? new List<ApplicationDto>();
        }

        public async ValueTask<int> InsertAsync(ApplicationDto dto)
        {
            Domain.Aggregates.OpenIddict.Application model = _mapper.Map<Domain.Aggregates.OpenIddict.Application>(dto);
            return await _unitOfWork.Application.InsertAsync(model);
        }

        public async ValueTask<bool> UpdateAsync(ApplicationDto dto)
        {
            Domain.Aggregates.OpenIddict.Application model = _mapper.Map<Domain.Aggregates.OpenIddict.Application>(dto);
            return await _unitOfWork.Application.UpdateAsync(model);
        }

        public async ValueTask<bool> DeleteAsync(ApplicationDto dto)
        {
            Domain.Aggregates.OpenIddict.Application model = dto?.ToModel() ?? new Domain.Aggregates.OpenIddict.Application();
            return await _unitOfWork.Application.DeleteAsync(model);
        }
    }
}
