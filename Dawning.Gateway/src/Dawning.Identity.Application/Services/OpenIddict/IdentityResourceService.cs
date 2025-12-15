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
    /// 身份资源服务实现
    /// </summary>
    public class IdentityResourceService : IIdentityResourceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public IdentityResourceService(IUnitOfWork unitOfWork, IMapper mapper)
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

        public async Task<IdentityResourceDto> GetAsync(Guid id)
        {
            var model = await _unitOfWork.IdentityResource.GetAsync(id);
            return model?.ToDto() ?? new IdentityResourceDto();
        }

        public async Task<IdentityResourceDto?> GetByNameAsync(string name)
        {
            var model = await _unitOfWork.IdentityResource.GetByNameAsync(name);
            return model?.ToDto();
        }

        public async Task<PagedData<IdentityResourceDto>> GetPagedListAsync(
            IdentityResourceModel model,
            int page,
            int itemsPerPage
        )
        {
            var data = await _unitOfWork.IdentityResource.GetPagedListAsync(
                model,
                page,
                itemsPerPage
            );

            return new PagedData<IdentityResourceDto>
            {
                PageIndex = data.PageIndex,
                PageSize = data.PageSize,
                TotalCount = data.TotalCount,
                Items = data.Items.ToDtos() ?? new List<IdentityResourceDto>(),
            };
        }

        public async Task<IEnumerable<IdentityResourceDto>?> GetAllAsync()
        {
            var list = await _unitOfWork.IdentityResource.GetAllAsync();
            return list?.ToDtos() ?? new List<IdentityResourceDto>();
        }

        public async Task<IEnumerable<IdentityResourceDto>?> GetByNamesAsync(
            IEnumerable<string> names
        )
        {
            var list = await _unitOfWork.IdentityResource.GetByNamesAsync(names);
            return list?.ToDtos() ?? new List<IdentityResourceDto>();
        }

        public async ValueTask<int> InsertAsync(IdentityResourceDto dto)
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Identity Resource name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
            {
                throw new ArgumentException("Identity Resource display name is required");
            }

            // 检查名称唯一性
            var existingResource = await _unitOfWork.IdentityResource.GetByNameAsync(dto.Name);
            if (existingResource != null)
            {
                throw new InvalidOperationException(
                    $"Identity Resource with name '{dto.Name}' already exists"
                );
            }

            // 映射并准备模型
            var model = _mapper.Map<IdentityResource>(dto);
            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            // 去重Claims
            if (model.UserClaims != null && model.UserClaims.Any())
            {
                model.UserClaims = model.UserClaims.Distinct().ToList();
            }

            int result = await _unitOfWork.IdentityResource.InsertAsync(model);

            // 记录审计日志
            if (dto.OperatorId.HasValue)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Create",
                    "IdentityResource",
                    model.Id,
                    $"Created Identity Resource: {model.Name}"
                );
            }

            return result;
        }

        public async ValueTask<bool> UpdateAsync(IdentityResourceDto dto)
        {
            // 验证ID
            if (dto.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("Identity Resource ID is required");
            }

            // 检查资源是否存在
            var existingResource = await _unitOfWork.IdentityResource.GetAsync(dto.Id.Value);
            if (existingResource == null)
            {
                throw new InvalidOperationException(
                    $"Identity Resource with ID '{dto.Id}' not found"
                );
            }

            // 如果名称被修改，检查唯一性
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existingResource.Name)
            {
                var duplicateResource = await _unitOfWork.IdentityResource.GetByNameAsync(dto.Name);
                if (duplicateResource != null && duplicateResource.Id != dto.Id)
                {
                    throw new InvalidOperationException(
                        $"Another Identity Resource with name '{dto.Name}' already exists"
                    );
                }
            }

            // 映射并更新
            var model = _mapper.Map<IdentityResource>(dto);

            // 去重Claims
            if (model.UserClaims != null && model.UserClaims.Any())
            {
                model.UserClaims = model.UserClaims.Distinct().ToList();
            }

            bool result = await _unitOfWork.IdentityResource.UpdateAsync(model);

            // 记录审计日志
            if (dto.OperatorId.HasValue)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Update",
                    "IdentityResource",
                    model.Id,
                    $"Updated Identity Resource: {model.Name}"
                );
            }

            return result;
        }

        public async ValueTask<bool> DeleteAsync(IdentityResourceDto dto)
        {
            // 验证ID
            if (dto?.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("Identity Resource ID is required");
            }

            // 检查资源是否存在
            var existingResource = await _unitOfWork.IdentityResource.GetAsync(dto.Id.Value);
            if (existingResource == null)
            {
                throw new InvalidOperationException(
                    $"Identity Resource with ID '{dto.Id}' not found"
                );
            }

            var model = dto?.ToModel() ?? new IdentityResource();
            bool result = await _unitOfWork.IdentityResource.DeleteAsync(model);

            // 记录审计日志
            if (dto?.OperatorId.HasValue == true)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Delete",
                    "IdentityResource",
                    model.Id,
                    $"Deleted Identity Resource: {existingResource.Name}"
                );
            }

            return result;
        }
    }
}
