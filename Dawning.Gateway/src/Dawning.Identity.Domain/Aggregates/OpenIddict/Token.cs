using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict 令牌聚合根
    /// </summary>
    public class Token : IAggregateRoot
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 关联的应用程序 ID
        /// </summary>
        public Guid? ApplicationId { get; set; }

        /// <summary>
        /// 关联的授权 ID
        /// </summary>
        public Guid? AuthorizationId { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// 令牌类型（access_token, refresh_token, id_token）
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 令牌状态（valid, revoked, redeemed）
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 令牌负载（JWT）
        /// </summary>
        public string? Payload { get; set; }

        /// <summary>
        /// 引用 ID（用于令牌内省）
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 业务方法：检查令牌是否过期
        /// </summary>
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        }
    }
}

