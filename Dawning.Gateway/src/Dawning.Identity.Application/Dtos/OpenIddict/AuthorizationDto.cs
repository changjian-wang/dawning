using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class AuthorizationDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

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
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
