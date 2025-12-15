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
    /// Service for managing OpenIddict tokens
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TokenService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TokenDto> GetAsync(Guid id)
        {
            Token model = await _unitOfWork.Token.GetAsync(id);
            return model?.ToDto() ?? new TokenDto();
        }

        public async Task<TokenDto?> GetByReferenceIdAsync(string referenceId)
        {
            Token? model = await _unitOfWork.Token.GetByReferenceIdAsync(referenceId);
            return model?.ToDto();
        }

        public async Task<IEnumerable<TokenDto>?> GetBySubjectAsync(string subject)
        {
            var list = await _unitOfWork.Token.GetBySubjectAsync(subject);
            return list?.ToDtos() ?? new List<TokenDto>();
        }

        public async Task<IEnumerable<TokenDto>?> GetByApplicationIdAsync(Guid applicationId)
        {
            var list = await _unitOfWork.Token.GetByApplicationIdAsync(applicationId);
            return list?.ToDtos() ?? new List<TokenDto>();
        }

        public async Task<IEnumerable<TokenDto>?> GetByAuthorizationIdAsync(Guid authorizationId)
        {
            var list = await _unitOfWork.Token.GetByAuthorizationIdAsync(authorizationId);
            return list?.ToDtos() ?? new List<TokenDto>();
        }

        public async Task<PagedData<TokenDto>> GetPagedListAsync(
            TokenModel model,
            int page,
            int itemsPerPage
        )
        {
            PagedData<Token> data = await _unitOfWork.Token.GetPagedListAsync(
                model,
                page,
                itemsPerPage
            );

            PagedData<TokenDto> pageResult = new PagedData<TokenDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<TokenDto>(),
            };

            return pageResult;
        }

        public async Task<IEnumerable<TokenDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.Token.GetAllAsync();
            return list?.ToDtos() ?? new List<TokenDto>();
        }

        public async ValueTask<int> InsertAsync(TokenDto dto)
        {
            Token model = _mapper.Map<Token>(dto);
            return await _unitOfWork.Token.InsertAsync(model);
        }

        public async ValueTask<bool> UpdateAsync(TokenDto dto)
        {
            Token model = _mapper.Map<Token>(dto);
            return await _unitOfWork.Token.UpdateAsync(model);
        }

        public async ValueTask<bool> DeleteAsync(TokenDto dto)
        {
            Token model = dto?.ToModel() ?? new Token();
            return await _unitOfWork.Token.DeleteAsync(model);
        }

        /// <summary>
        /// 清理过期的令牌
        /// </summary>
        public async Task<int> PruneExpiredTokensAsync()
        {
            return await _unitOfWork.Token.PruneExpiredTokensAsync();
        }
    }
}
