namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public class IdentityResource
{
    public Guid Id { get; set; }

    /// <summary>
    /// 指示此资源是否已启用且可以请求。默认值为 true。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 标识资源的唯一名称。这是客户端将用于授权请求中的 scope 参数的值。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 例如，此值将用于同意屏幕上。
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 例如，此值将用于同意屏幕上。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 指定用户是否可以在同意屏幕上取消选择范围（如果同意屏幕想要实现此类功能）。默认值为 false。
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 指定同意屏幕是否强调此范围（如果同意屏幕想要实现此类功能）。将此设置用于敏感或重要作用域。默认值为 false。
    /// </summary>
    public bool Emphasize { get; set; }

    /// <summary>
    /// 指定此范围是否显示在发现文档中。默认值为 true。
    /// </summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;

    /// <summary>
    /// 应包含在标识令牌中的关联用户声明类型的列表。
    /// </summary>
    public List<IdentityResourceClaim> UserClaims { get; set; } = new();

    /// <summary>
    /// 表示与此身份资源相关的属性集合。
    /// </summary>
    public List<IdentityResourceProperty> Properties { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool NonEditable { get; set; }
}