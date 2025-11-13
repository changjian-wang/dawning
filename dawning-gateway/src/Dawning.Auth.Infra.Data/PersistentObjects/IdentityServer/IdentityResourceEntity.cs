using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Infra.Data.PersistentObjects.IdentityServer;

[Table("identity_resources")]
public class IdentityResourceEntity
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
        /// 标识资源的唯一名称。这是客户端将用于授权请求中的 scope 参数的值。
        /// </summary>
        [Column("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 例如，此值将用于同意屏幕上。
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 例如，此值将用于同意屏幕上。
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
        /// 创建时间
        /// </summary>
        [IgnoreUpdate]
        [Column("created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated")]
        public DateTime? Updated { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        [Column("non_editable")]
        public bool NonEditable { get; set; }
    }