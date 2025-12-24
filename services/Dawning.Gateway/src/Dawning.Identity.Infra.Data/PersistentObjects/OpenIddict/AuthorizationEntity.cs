using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// OpenIddict 授权持久化对象
    /// </summary>
    [Table("openiddict_authorizations")]
    public class AuthorizationEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("application_id")]
        public Guid? ApplicationId { get; set; }

        [Column("subject")]
        public string? Subject { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        /// <summary>
        /// 作用域列表（JSON 格式存储）
        /// </summary>
        [Column("scopes")]
        public string? ScopesJson { get; set; }

        /// <summary>
        /// 扩展属性（JSON 格式存储）
        /// </summary>
        [Column("properties")]
        public string? PropertiesJson { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [Column("timestamp")]
        [IgnoreUpdate]
        [DefaultSortName]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        [Column("created_at")]
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
