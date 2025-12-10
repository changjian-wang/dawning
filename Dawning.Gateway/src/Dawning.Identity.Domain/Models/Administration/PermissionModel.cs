using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// 权限查询模型
    /// </summary>
    public class PermissionModel
    {
        /// <summary>
        /// 权限代码（模糊查询）
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 权限名称（模糊查询）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public string? Resource { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// 权限分类
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
