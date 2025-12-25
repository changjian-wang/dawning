using System;
using Dawning.ORM.Dapper;

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
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// 分配时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 分配者ID
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}
