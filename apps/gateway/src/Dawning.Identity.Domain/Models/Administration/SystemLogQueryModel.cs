using System;

namespace Dawning.Identity.Domain.Models.Administration
{
    /// <summary>
    /// 系统日志查询模型
    /// </summary>
    public class SystemLogQueryModel
    {
        /// <summary>
        /// 日志级别（Info, Warning, Error）
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// 关键字搜索（在message中模糊查询）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 操作用户ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// 操作用户名（模糊查询）
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 请求路径（模糊查询）
        /// </summary>
        public string? RequestPath { get; set; }

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
