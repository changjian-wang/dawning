using Dawning.Identity.Domain.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    public class SystemMetadata : IAggregateRoot
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 类型：Client，IdentityResource，ApiResource，ApiScope，也可以存储上级key来形成上下级联动查询
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 用户是否可编辑
        /// </summary>
        public bool NonEditable { get; set; } = true;

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}
