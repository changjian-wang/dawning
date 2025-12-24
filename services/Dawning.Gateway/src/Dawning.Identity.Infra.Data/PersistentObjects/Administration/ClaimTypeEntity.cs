using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    [Table("claim_types")]
    public class ClaimTypeEntity
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 类型。String, Int, DateTime, Boolean, Enum
        /// </summary>
        [Column("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否必须项
        /// </summary>
        [Column("required")]
        public bool Required { get; set; } = false;

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
        [Column("created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated")]
        public DateTime? Updated { get; set; }
    }
}
