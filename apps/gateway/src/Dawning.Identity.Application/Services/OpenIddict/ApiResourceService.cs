using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Application.Mapping.OpenIddict;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;

namespace Dawning.Identity.Application.Services.OpenIddict
{
    /// <summary>
    /// API资源服务实现
    /// </summary>
    public class ApiResourceService : IApiResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ApiResourceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// 记录审计日志
        /// </summary>
        private async Task LogAuditAsync(
            Guid userId,
            string action,
            string entityType,
            Guid entityId,
            string description
        )
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAt = DateTime.UtcNow,
            };
            await _unitOfWork.AuditLog.InsertAsync(auditLog);
        }

        public async Task<ApiResourceDto> GetAsync(Guid id)
        {
            var model = await _unitOfWork.ApiResource.GetAsync(id);
            return model?.ToDto() ?? new ApiResourceDto();
        }

        public async Task<ApiResourceDto?> GetByNameAsync(string name)
        {
            var model = await _unitOfWork.ApiResource.GetByNameAsync(name);
            return model?.ToDto();
        }

        public async Task<PagedData<ApiResourceDto>> GetPagedListAsync(
            ApiResourceModel model,
            int page,
            int itemsPerPage
        )
        {
            var data = await _unitOfWork.ApiResource.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<ApiResourceDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<ApiResourceDto>(),
            };
        }

        public async Task<IEnumerable<ApiResourceDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.ApiResource.GetAllAsync();
            return list?.ToDtos() ?? new List<ApiResourceDto>();
        }

        public async Task<IEnumerable<ApiResourceDto>?> GetByNamesAsync(IEnumerable<string> names)
        {
            var list = await _unitOfWork.ApiResource.GetByNamesAsync(names);
            return list?.ToDtos() ?? new List<ApiResourceDto>();
        }

        public async ValueTask<int> InsertAsync(ApiResourceDto dto)
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("API Resource name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
            {
                throw new ArgumentException("API Resource display name is required");
            }

            // 检查名称唯一性
            var existingResource = await _unitOfWork.ApiResource.GetByNameAsync(dto.Name);
            if (existingResource != null)
            {
                throw new InvalidOperationException(
                    $"API Resource with name '{dto.Name}' already exists"
                );
            }

            // 映射并准备模型
            var model = _mapper.Map<ApiResource>(dto);
            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            // 去重Scopes和Claims
            if (model.Scopes != null && model.Scopes.Any())
            {
                model.Scopes = model.Scopes.Distinct().ToList();
            }

            if (model.UserClaims != null && model.UserClaims.Any())
            {
                model.UserClaims = model.UserClaims.Distinct().ToList();
            }

            int result = await _unitOfWork.ApiResource.InsertAsync(model);

            // 记录审计日志
            if (dto.OperatorId.HasValue)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Create",
                    "ApiResource",
                    model.Id,
                    $"Created API Resource: {model.Name}"
                );
            }

            return result;
        }

        public async ValueTask<bool> UpdateAsync(ApiResourceDto dto)
        {
            // 验证ID
            if (dto.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("API Resource ID is required");
            }

            // 检查资源是否存在
            var existingResource = await _unitOfWork.ApiResource.GetAsync(dto.Id.Value);
            if (existingResource == null)
            {
                throw new InvalidOperationException($"API Resource with ID '{dto.Id}' not found");
            }

            // 如果名称被修改，检查唯一性
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existingResource.Name)
            {
                var duplicateResource = await _unitOfWork.ApiResource.GetByNameAsync(dto.Name);
                if (duplicateResource != null && duplicateResource.Id != dto.Id)
                {
                    throw new InvalidOperationException(
                        $"Another API Resource with name '{dto.Name}' already exists"
                    );
                }
            }

            // 映射并更新
            var model = _mapper.Map<ApiResource>(dto);

            // 去重Scopes和Claims
            if (model.Scopes != null && model.Scopes.Any())
            {
                model.Scopes = model.Scopes.Distinct().ToList();
            }

            if (model.UserClaims != null && model.UserClaims.Any())
            {
                model.UserClaims = model.UserClaims.Distinct().ToList();
            }

            bool result = await _unitOfWork.ApiResource.UpdateAsync(model);

            // 记录审计日志
            if (dto.OperatorId.HasValue)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Update",
                    "ApiResource",
                    model.Id,
                    $"Updated API Resource: {model.Name}"
                );
            }

            return result;
        }

        public async ValueTask<bool> DeleteAsync(ApiResourceDto dto)
        {
            // 验证ID
            if (dto?.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("API Resource ID is required");
            }

            // 检查资源是否存在
            var existingResource = await _unitOfWork.ApiResource.GetAsync(dto.Id.Value);
            if (existingResource == null)
            {
                throw new InvalidOperationException($"API Resource with ID '{dto.Id}' not found");
            }

            var model = dto?.ToModel() ?? new ApiResource();
            bool result = await _unitOfWork.ApiResource.DeleteAsync(model);

            // 记录审计日志
            if (dto?.OperatorId.HasValue == true)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Delete",
                    "ApiResource",
                    model.Id,
                    $"Deleted API Resource: {existingResource.Name}"
                );
            }

            return result;
        }
    }
}
