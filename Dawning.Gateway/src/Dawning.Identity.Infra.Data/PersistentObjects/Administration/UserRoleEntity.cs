using System;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 用户角色关联数据库实体
    /// </summary>
    [Table("user_roles")]
    public class UserRoleEntity
    {
        /// <summary>
        /// 关联ID
        /// </summary>
        [ExplicitKey]
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
