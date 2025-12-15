using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 用户角色关联聚合根
    /// </summary>
    public class UserRole : IAggregateRoot
    {
        /// <summary>
        /// 关联ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 分配时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 分配者ID
        /// </summary>
        public Guid? CreatedBy { get; set; }
    }
}
