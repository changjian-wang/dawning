using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Mapping.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// Role Application Service Implementation
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public async Task<RoleDto?> GetAsync(Guid id)
        {
            var role = await _unitOfWork.Role.GetAsync(id);
            return role?.ToDto();
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        public async Task<RoleDto?> GetByNameAsync(string name)
        {
            var role = await _unitOfWork.Role.GetByNameAsync(name);
            return role?.ToDto();
        }

        /// <summary>
        /// Get paged role list
        /// </summary>
        public async Task<PagedData<RoleDto>> GetPagedListAsync(
            RoleModel model,
            int page,
            int itemsPerPage
        )
        {
            var pagedData = await _unitOfWork.Role.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<RoleDto>
            {
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
                TotalCount = pagedData.TotalCount,
                Items = pagedData.Items.ToDtos(),
            };
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _unitOfWork.Role.GetAllAsync();
            return roles.ToDtos();
        }

        /// <summary>
        /// Create role
        /// </summary>
        public async Task<RoleDto> CreateAsync(CreateRoleDto dto, Guid? operatorId = null)
        {
            // Validate if role name already exists
            if (await _unitOfWork.Role.NameExistsAsync(dto.Name))
            {
                throw new InvalidOperationException($"Role name '{dto.Name}' already exists.");
            }

            // Create role entity
            var role = dto.ToEntity();
            role.CreatedBy = operatorId;

            await _unitOfWork.Role.InsertAsync(role);

            return role.ToDto();
        }

        /// <summary>
        /// Update role
        /// </summary>
        public async Task<RoleDto> UpdateAsync(UpdateRoleDto dto, Guid? operatorId = null)
        {
            var role = await _unitOfWork.Role.GetAsync(dto.Id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID '{dto.Id}' not found.");
            }

            // System roles cannot have certain properties modified
            if (role.IsSystem)
            {
                throw new InvalidOperationException("System roles cannot be modified.");
            }

            // Update fields
            role.ApplyUpdate(dto);
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = operatorId;

            await _unitOfWork.Role.UpdateAsync(role);

            return role.ToDto();
        }

        /// <summary>
        /// Delete role
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, Guid? operatorId = null)
        {
            var role = await _unitOfWork.Role.GetAsync(id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID '{id}' not found.");
            }

            // System roles cannot be deleted
            if (role.IsSystem)
            {
                throw new InvalidOperationException("System roles cannot be deleted.");
            }

            role.UpdatedBy = operatorId;
            var result = await _unitOfWork.Role.DeleteAsync(role);

            return result;
        }

        /// <summary>
        /// Check if role name exists
        /// </summary>
        public async Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null)
        {
            return await _unitOfWork.Role.NameExistsAsync(name, excludeRoleId);
        }
    }
}
