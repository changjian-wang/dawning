namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

/// <summary>
/// Represents an API resource in the IdentityServer, which can be protected by IdentityServer.
/// An API resource defines how an API is exposed to clients and what claims are included in the access token for that API.
/// </summary>
public class ApiResource
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
    /// 
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? AllowedAccessTokenSigningAlgorithms { get; set; }

    /// <summary>
    /// 指定此范围是否显示在发现文档中。默认为true。
    /// </summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;

    /// <summary>
    /// API密钥用于内省端点。API可以使用API名称和密钥进行内省验证。
    /// </summary>
    public ICollection<ApiResourceSecret> Secrets { get; set; }

    /// <summary>
    /// API必须至少有一个范围。每个范围可以有不同的设置。
    /// </summary>
    public ICollection<ApiResourceScope> Scopes { get; set; }

    /// <summary>
    /// 应包含在访问令牌中的关联用户声明类型的列表。
    /// </summary>
    public ICollection<ApiResourceClaim> UserClaims { get; set; }

    /// <summary>
    /// 字典可根据需要保存任何自定义客户端特定值。
    /// </summary>
    public ICollection<ApiResourceProperty> Properties { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// 最近一次访问时间
    /// </summary>
    public DateTime? LastAccessed { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool NonEditable { get; set; }
}