using System;
using Dawning.Identity.Domain.Core.Interfaces;

namespace Dawning.Identity.Domain.Aggregates.Administration
{
    /// <summary>
    /// 审计日志聚合根
    /// </summary>
    public class AuditLog : IAggregateRoot
    {
        /// <summary>
        /// 审计日志唯一标识
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 操作用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 操作类型 (Create, Update, Delete, Login, Logout等)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 实体类型 (User, Role, Application等)
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        public string? RequestPath { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// 修改前的值 (JSON字符串)
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// 修改后的值 (JSON字符串)
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }
}
