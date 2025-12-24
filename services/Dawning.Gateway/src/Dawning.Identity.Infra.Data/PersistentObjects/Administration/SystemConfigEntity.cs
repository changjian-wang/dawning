using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    [Table("system_configs")]
    public class SystemConfigEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 类型：Client，IdentityResource，ApiResource，ApiScope，也可以存储上级key来形成上下级联动查询
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        [Column("key")]
        public string? Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [Column("value")]
        public string? Value { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 用户是否可编辑
        /// </summary>
        [Column("non_editable")]
        public bool NonEditable { get; set; } = true;

        /// <summary>
        /// 时间戳
        /// </summary>
        [Column("timestamp")]
        [DefaultSortName]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// 创建时间
        /// </summary>
        [IgnoreUpdate]
        [Column("created_at")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? Updated { get; set; }
    }
}
