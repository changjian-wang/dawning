using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 角色权限关联实体
    /// </summary>
    public class RolePermission : IAggregateRoot
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        public Guid PermissionId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}
