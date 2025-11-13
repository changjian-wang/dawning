using Dawning.Auth.Dapper.Contrib;
using Dawning.Auth.Domain.Core.Interfaces;

namespace Dawning.Auth.Domain.Aggregates.Administration
{
    public class ClaimType : IAggregateRoot
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 类型。String, Int, DateTime, Boolean, Enum
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否必须项
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 用户是否可编辑
        /// </summary>
        public bool NonEditable { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [IgnoreUpdate]
        public DateTime Created { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? Updated { get; set; }
    }
}
