using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Gateway
{
    /// <summary>
    /// 限流策略聚合根
    /// </summary>
    public class RateLimitPolicy : IAggregateRoot
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 策略名称（唯一标识）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 策略类型: fixed-window, sliding-window, token-bucket, concurrency
        /// </summary>
        public string PolicyType { get; set; } = "fixed-window";

        /// <summary>
        /// 每窗口允许的请求数
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// 时间窗口（秒）
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// 滑动窗口分段数（仅用于 sliding-window）
        /// </summary>
        public int SegmentsPerWindow { get; set; } = 6;

        /// <summary>
        /// 队列限制
        /// </summary>
        public int QueueLimit { get; set; } = 0;

        /// <summary>
        /// 令牌补充速率（仅用于 token-bucket）
        /// </summary>
        public int TokensPerPeriod { get; set; } = 10;

        /// <summary>
        /// 令牌补充周期（秒）（仅用于 token-bucket）
        /// </summary>
        public int ReplenishmentPeriodSeconds { get; set; } = 1;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
