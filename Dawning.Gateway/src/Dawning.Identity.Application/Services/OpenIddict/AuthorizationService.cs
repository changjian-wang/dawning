using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Application.Mapping.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Application.Services.OpenIddict
{
    /// <summary>
    /// Service for managing OpenIddict authorizations
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthorizationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AuthorizationDto> GetAsync(Guid id)
        {
            Authorization model = await _unitOfWork.Authorization.GetAsync(id);
            return model?.ToDto() ?? new AuthorizationDto();
        }

        public async Task<IEnumerable<AuthorizationDto>?> GetBySubjectAsync(string subject)
        {
            var list = await _unitOfWork.Authorization.GetBySubjectAsync(subject);
            return list?.ToDtos() ?? new List<AuthorizationDto>();
        }

        public async Task<IEnumerable<AuthorizationDto>?> GetByApplicationIdAsync(
            Guid applicationId
        )
        {
            var list = await _unitOfWork.Authorization.GetByApplicationIdAsync(applicationId);
            return list?.ToDtos() ?? new List<AuthorizationDto>();
        }

        public async Task<PagedData<AuthorizationDto>> GetPagedListAsync(
            AuthorizationModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedData<Authorization> data = await _unitOfWork.Authorization.GetPagedListAsync(
                model,
                page,
                itemsPerPage
            );

            PagedData<AuthorizationDto> pageResult = new PagedData<AuthorizationDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<AuthorizationDto>(),
            };

            return pageResult;
        }

        public async Task<IEnumerable<AuthorizationDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.Authorization.GetAllAsync();
            return list?.ToDtos() ?? new List<AuthorizationDto>();
        }

        public async ValueTask<int> InsertAsync(AuthorizationDto dto)
        {
            Authorization model = _mapper.Map<Authorization>(dto);
            return await _unitOfWork.Authorization.InsertAsync(model);
        }

        public async ValueTask<bool> UpdateAsync(AuthorizationDto dto)
        {
            Authorization model = _mapper.Map<Authorization>(dto);
            return await _unitOfWork.Authorization.UpdateAsync(model);
        }

        public async ValueTask<bool> DeleteAsync(AuthorizationDto dto)
        {
            Authorization model = dto?.ToModel() ?? new Authorization();
            return await _unitOfWork.Authorization.DeleteAsync(model);
        }
    }
}
