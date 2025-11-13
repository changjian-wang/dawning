using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// ApiScope
    /// </summary>
    [Table("api_scopes")]
    public class ApiScope
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 指示此资源是否已启用且可以请求。默认值为 true。
        /// </summary>
        [Column("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// API 的唯一名称。此值用于具有自省功能的身份验证，并将添加到传出访问令牌的受众中。
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 例如，可以在同意屏幕上使用此值。
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 例如，可以在同意屏幕上使用此值。
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 指定用户是否可以在同意屏幕上取消选择范围（如果同意屏幕想要实现此类功能）。默认值为 false。
        /// </summary>
        [Column("required")]
        public bool Required { get; set; }

        /// <summary>
        /// 指定同意屏幕是否强调此范围（如果同意屏幕想要实现此类功能）。将此设置用于敏感或重要作用域。默认值为 false。
        /// </summary>
        [Column("emphasize")]
        public bool Emphasize { get; set; }

        /// <summary>
        /// 指定此范围是否显示在发现文档中。默认值为 true。
        /// </summary>
        [Column("show_in_discovery_document")]
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// 应包含在访问令牌中的关联用户声明类型的列表。
        /// </summary>
        [Computed]
        public List<ApiScopeClaim> UserClaims { get; set; }

        /// <summary>
        /// 字典，用于根据需要保存任何自定义客户端特定的值。
        /// </summary>
        [Computed]
        public List<ApiScopeProperty> Properties { get; set; }
    }
}

