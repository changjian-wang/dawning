using Dawning.Identity.Application.Dtos.User;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Administration;
using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Administration
{
    /// <summary>
    /// 用户应用服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        Task<UserDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        Task<UserDto?> GetByUsernameAsync(string username);

        /// <summary>
        /// 获取分页用户列表
        /// </summary>
        Task<PagedData<UserDto>> GetPagedListAsync(UserModel model, int page, int itemsPerPage);

        /// <summary>
        /// 创建用户
        /// </summary>
        Task<UserDto> CreateAsync(CreateUserDto dto, Guid? operatorId = null);

        /// <summary>
        /// 更新用户
        /// </summary>
        Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid? operatorId = null);

        /// <summary>
        /// 删除用户
        /// </summary>
        Task<bool> DeleteAsync(Guid id, Guid? operatorId = null);

        /// <summary>
        /// 修改密码
        /// </summary>
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);

        /// <summary>
        /// 更新最后登录时间
        /// </summary>
        Task UpdateLastLoginAsync(Guid userId);

        /// <summary>
        /// 验证用户密码
        /// </summary>
        Task<UserDto?> ValidatePasswordAsync(string username, string password);

        /// <summary>
        /// 验证用户凭据并更新登录时间
        /// </summary>
        Task<UserDto?> ValidateCredentialsAndUpdateLoginAsync(string username, string password);
    }
}
