using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Dtos.OpenIddict
{
    public class ScopeDto
    {
        public Guid? Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 作用域名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<string> Resources { get; set; } = new();

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
