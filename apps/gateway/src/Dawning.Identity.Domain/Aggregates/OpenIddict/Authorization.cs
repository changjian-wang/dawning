using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict 授权聚合根
    /// </summary>
    public class Authorization : IAggregateRoot
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 关联的应用程序 ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// 授权类型
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 授权状态（valid, revoked）
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 授权的作用域列表
        /// </summary>
        public List<string> Scopes { get; set; } = new();

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
