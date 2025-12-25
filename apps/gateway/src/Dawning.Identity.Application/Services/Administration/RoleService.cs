using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// 角色应用服务实现
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        public async Task<RoleDto?> GetAsync(Guid id)
        {
            var role = await _uow.Role.GetAsync(id);
            return role != null ? _mapper.Map<RoleDto>(role) : null;
        }

        /// <summary>
        /// 根据名称获取角色
        /// </summary>
        public async Task<RoleDto?> GetByNameAsync(string name)
        {
            var role = await _uow.Role.GetByNameAsync(name);
            return role != null ? _mapper.Map<RoleDto>(role) : null;
        }

        /// <summary>
        /// 获取分页角色列表
        /// </summary>
        public async Task<PagedData<RoleDto>> GetPagedListAsync(
            RoleModel model,
            int page,
            int itemsPerPage
        )
        {
            var pagedData = await _uow.Role.GetPagedListAsync(model, page, itemsPerPage);

            return new PagedData<RoleDto>
            {
                PageIndex = pagedData.PageIndex,
                PageSize = pagedData.PageSize,
                TotalCount = pagedData.TotalCount,
                Items = pagedData.Items.Select(r => _mapper.Map<RoleDto>(r)),
            };
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _uow.Role.GetAllAsync();
            return roles.Select(r => _mapper.Map<RoleDto>(r));
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        public async Task<RoleDto> CreateAsync(CreateRoleDto dto, Guid? operatorId = null)
        {
            // 验证角色名称是否已存在
            if (await _uow.Role.NameExistsAsync(dto.Name))
            {
                throw new InvalidOperationException($"Role name '{dto.Name}' already exists.");
            }

            // 创建角色实体
            var role = _mapper.Map<Role>(dto);
            role.Id = Guid.NewGuid();
            role.CreatedAt = DateTime.UtcNow;
            role.CreatedBy = operatorId;

            await _uow.Role.InsertAsync(role);

            return _mapper.Map<RoleDto>(role);
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        public async Task<RoleDto> UpdateAsync(UpdateRoleDto dto, Guid? operatorId = null)
        {
            var role = await _uow.Role.GetAsync(dto.Id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID '{dto.Id}' not found.");
            }

            // 系统角色不允许修改某些属性
            if (role.IsSystem)
            {
                throw new InvalidOperationException("System roles cannot be modified.");
            }

            // 更新字段
            if (dto.DisplayName != null)
                role.DisplayName = dto.DisplayName;
            if (dto.Description != null)
                role.Description = dto.Description;
            if (dto.IsActive.HasValue)
                role.IsActive = dto.IsActive.Value;
            if (dto.Permissions != null)
            {
                role.Permissions = dto.Permissions.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(dto.Permissions)
                    : null;
            }

            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = operatorId;

            await _uow.Role.UpdateAsync(role);

            return _mapper.Map<RoleDto>(role);
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, Guid? operatorId = null)
        {
            var role = await _uow.Role.GetAsync(id);
            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID '{id}' not found.");
            }

            // 系统角色不允许删除
            if (role.IsSystem)
            {
                throw new InvalidOperationException("System roles cannot be deleted.");
            }

            role.UpdatedBy = operatorId;
            var result = await _uow.Role.DeleteAsync(role);

            return result;
        }

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        public async Task<bool> NameExistsAsync(string name, Guid? excludeRoleId = null)
        {
            return await _uow.Role.NameExistsAsync(name, excludeRoleId);
        }
    }
}
