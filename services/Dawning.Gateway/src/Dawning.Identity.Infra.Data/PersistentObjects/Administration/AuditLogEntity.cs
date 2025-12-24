using System;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Administration
{
    /// <summary>
    /// 审计日志数据库实体
    /// </summary>
    [Table("audit_logs")]
    public class AuditLogEntity
    {
        /// <summary>
        /// 审计日志ID
        /// </summary>
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 操作用户ID
        /// </summary>
        [Column("user_id")]
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作用户名
        /// </summary>
        [Column("username")]
        public string? Username { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 实体类型
        /// </summary>
        [Column("entity_type")]
        public string? EntityType { get; set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        [Column("entity_id")]
        public Guid? EntityId { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        [Column("request_path")]
        public string? RequestPath { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        [Column("request_method")]
        public string? RequestMethod { get; set; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        [Column("status_code")]
        public int? StatusCode { get; set; }

        /// <summary>
        /// 修改前的值 (JSON字符串)
        /// </summary>
        [Column("old_values")]
        public string? OldValues { get; set; }

        /// <summary>
        /// 修改后的值 (JSON字符串)
        /// </summary>
        [Column("new_values")]
        public string? NewValues { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 时间戳（计算字段，不映射到数据库）
        /// </summary>
        [Computed]
        public long Timestamp { get; set; }
    }
}
