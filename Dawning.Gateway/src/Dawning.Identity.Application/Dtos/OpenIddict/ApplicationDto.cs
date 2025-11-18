using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class ApplicationDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 客户端 ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 客户端类型（confidential, public）
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 同意类型（explicit, implicit, systematic）
        /// </summary>
        public string? ConsentType { get; set; }

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// 重定向 URI 列表
        /// </summary>
        public List<string> RedirectUris { get; set; } = new();

        /// <summary>
        /// 登出重定向 URI 列表
        /// </summary>
        public List<string> PostLogoutRedirectUris { get; set; } = new();

        /// <summary>
        /// 要求列表
        /// </summary>
        public List<string> Requirements { get; set; } = new();

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
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
