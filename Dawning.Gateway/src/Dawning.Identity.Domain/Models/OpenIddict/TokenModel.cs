using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Token 查询模型
    /// </summary>
    public class TokenModel
    {
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
    }
}
