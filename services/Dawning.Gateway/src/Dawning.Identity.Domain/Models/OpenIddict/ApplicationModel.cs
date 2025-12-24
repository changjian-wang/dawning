using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// Application 查询模型
    /// </summary>
    public class ApplicationModel
    {
        /// <summary>
        /// 客户端 ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 客户端类型（confidential, public）
        /// </summary>
        public string? Type { get; set; }
    }
}
