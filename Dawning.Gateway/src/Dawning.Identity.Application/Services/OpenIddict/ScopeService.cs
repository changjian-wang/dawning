using AutoMapper;
using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Application.Mapping.OpenIddict;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.OpenIddict;
using Dawning.Identity.Domain.Aggregates.Administration;
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

        /// <summary>
        /// Logs an audit entry for scope operations
        /// </summary>
        private async Task LogAuditAsync(Guid userId, string action, string entityType, Guid entityId, string description)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLog.InsertAsync(auditLog);
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
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Scope name is required");
            }

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
            {
                throw new ArgumentException("Scope display name is required");
            }

            // Check if scope with same name already exists
            var existingScope = await _unitOfWork.Scope.GetByNameAsync(dto.Name);
            if (existingScope != null)
            {
                throw new InvalidOperationException($"Scope with name '{dto.Name}' already exists");
            }

            // Map and prepare model
            Scope model = _mapper.Map<Scope>(dto);
            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            // Remove duplicate resources
            if (model.Resources != null && model.Resources.Any())
            {
                model.Resources = model.Resources.Distinct().ToList();
            }

            int result = await _unitOfWork.Scope.InsertAsync(model);

            // Log audit
            if (dto.OperatorId.HasValue)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Create",
                    "Scope",
                    model.Id,
                    $"Created scope: {model.Name}");
            }

            return result;
        }

        public async ValueTask<bool> UpdateAsync(ScopeDto dto)
        {
            // Validate ID
            if (dto.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("Scope ID is required");
            }

            // Check if scope exists
            var existingScope = await _unitOfWork.Scope.GetAsync(dto.Id.Value);
            if (existingScope == null)
            {
                throw new InvalidOperationException($"Scope with ID '{dto.Id}' not found");
            }

            // If name is being changed, check for duplicates
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existingScope.Name)
            {
                var duplicateScope = await _unitOfWork.Scope.GetByNameAsync(dto.Name);
                if (duplicateScope != null && duplicateScope.Id != dto.Id)
                {
                    throw new InvalidOperationException($"Another scope with name '{dto.Name}' already exists");
                }
            }

            // Map and update
            Scope model = _mapper.Map<Scope>(dto);

            // Remove duplicate resources
            if (model.Resources != null && model.Resources.Any())
            {
                model.Resources = model.Resources.Distinct().ToList();
            }

            bool result = await _unitOfWork.Scope.UpdateAsync(model);

            // Log audit
            if (dto.OperatorId.HasValue)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Update",
                    "Scope",
                    model.Id,
                    $"Updated scope: {model.Name}");
            }

            return result;
        }

        public async ValueTask<bool> DeleteAsync(ScopeDto dto)
        {
            // Validate ID
            if (dto?.Id == null || dto.Id == Guid.Empty)
            {
                throw new ArgumentException("Scope ID is required");
            }

            // Check if scope exists
            var existingScope = await _unitOfWork.Scope.GetAsync(dto.Id.Value);
            if (existingScope == null)
            {
                throw new InvalidOperationException($"Scope with ID '{dto.Id}' not found");
            }

            Scope model = dto?.ToModel() ?? new Scope();
            bool result = await _unitOfWork.Scope.DeleteAsync(model);

            // Log audit
            if (dto?.OperatorId.HasValue == true)
            {
                await LogAuditAsync(
                    dto.OperatorId.Value,
                    "Delete",
                    "Scope",
                    model.Id,
                    $"Deleted scope: {existingScope.Name}");
            }

            return result;
        }
    }
}
