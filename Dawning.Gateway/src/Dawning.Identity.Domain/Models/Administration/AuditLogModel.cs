namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// 审计日志查询模型
    /// </summary>
    public class AuditLogModel
    {
        /// <summary>
        /// 操作用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作用户名（模糊查询）
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
