using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// 角色查询模型
    /// </summary>
    public class RoleModel
    {
        /// <summary>
        /// 角色名称（模糊查询）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 显示名称（模糊查询）
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 是否为系统角色
        /// </summary>
        public bool? IsSystem { get; set; }
    }
}
