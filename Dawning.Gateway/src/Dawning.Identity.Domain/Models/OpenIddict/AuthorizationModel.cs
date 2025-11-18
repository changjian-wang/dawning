using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Authorization 查询模型
    /// </summary>
    public class AuthorizationModel
    {
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
    }
}
