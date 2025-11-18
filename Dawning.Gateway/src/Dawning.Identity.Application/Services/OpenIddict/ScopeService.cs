using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Application.Mapping.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
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
    /// <summary>
    /// Service for managing OpenIddict scopes
    /// </summary>
    public class ScopeService : IScopeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScopeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ScopeDto> GetAsync(Guid id)
        {
            Scope model = await _unitOfWork.Scope.GetAsync(id);
            return model?.ToDto() ?? new ScopeDto();
        }

        public async Task<ScopeDto?> GetByNameAsync(string name)
        {
            Scope? model = await _unitOfWork.Scope.GetByNameAsync(name);
            return model?.ToDto();
        }

        public async Task<PagedData<ScopeDto>> GetPagedListAsync(
            ScopeModel model,
            int page,
            int itemsPerPage)
        {
            PagedData<Scope> data = await _unitOfWork.Scope.GetPagedListAsync(
                model,
                page,
                itemsPerPage);

            PagedData<ScopeDto> pageResult = new PagedData<ScopeDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<ScopeDto>()
            };

            return pageResult;
        }

        public async Task<IEnumerable<ScopeDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.Scope.GetAllAsync();
            return list?.ToDtos() ?? new List<ScopeDto>();
        }

        public async Task<IEnumerable<ScopeDto>?> GetByNamesAsync(IEnumerable<string> names)
        {
            var list = await _unitOfWork.Scope.GetByNamesAsync(names);
            return list?.ToDtos() ?? new List<ScopeDto>();
        }

        public async ValueTask<int> InsertAsync(ScopeDto dto)
        {
            Scope model = _mapper.Map<Scope>(dto);
            return await _unitOfWork.Scope.InsertAsync(model);
        }

        public async ValueTask<bool> UpdateAsync(ScopeDto dto)
        {
            Scope model = _mapper.Map<Scope>(dto);
            return await _unitOfWork.Scope.UpdateAsync(model);
        }

        public async ValueTask<bool> DeleteAsync(ScopeDto dto)
        {
            Scope model = dto?.ToModel() ?? new Scope();
            return await _unitOfWork.Scope.DeleteAsync(model);
        }
    }
}
