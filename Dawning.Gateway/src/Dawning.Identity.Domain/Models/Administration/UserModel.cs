using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// 用户查询过滤模型
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// 用户名（模糊搜索）
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 邮箱（模糊搜索）
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 显示名称（模糊搜索）
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 角色筛选
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 是否包含已删除用户
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }
}
