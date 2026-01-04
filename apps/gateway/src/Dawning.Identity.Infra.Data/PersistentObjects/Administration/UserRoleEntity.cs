using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// User-role association database entity
    /// </summary>
    [Table("user_roles")]
    public class UserRoleEntity
    {
        /// <summary>
        /// Association ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Role ID
        /// </summary>
        [Column("role_id")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// Assignment time
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Assigner ID
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}
