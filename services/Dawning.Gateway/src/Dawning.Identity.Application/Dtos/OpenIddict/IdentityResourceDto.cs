using System;
using System.Collections.Generic;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    /// <summary>
    /// 身份资源DTO
    /// </summary>
    public class IdentityResourceDto
    {
        public Guid? Id { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 用户是否必须同意
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// 是否在同意界面中强调
        /// </summary>
        public bool Emphasize { get; set; } = false;

        /// <summary>
        /// 是否在发现文档中显示
        /// </summary>
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// 用户声明类型
        /// </summary>
        public List<string> UserClaims { get; set; } = new();

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 操作者ID (用于审计日志)
        /// </summary>
        public Guid? OperatorId { get; set; }
    }
}
