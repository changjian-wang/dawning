using System;
using Dawning.Auth.Dapper.Contrib;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// Represents an API resource in the authentication and authorization system.
    /// This class is used to store information about an API that can be accessed by clients,
    /// including whether it is enabled, its name, display name, description, and more.
    /// It also maintains a list of secrets, scopes, user claims, and properties associated with the API.
    /// </summary>
    /// <remarks>
    /// The table in the database corresponding to this entity is named "api_resources".
    /// </remarks>
    [Table("api_resources")]
    public class ApiResource
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
        /// 
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column("allowed_access_token_signing_algorithms")]
        public string? AllowedAccessTokenSigningAlgorithms { get; set; }

        /// <summary>
        /// 指定此范围是否显示在发现文档中。默认为true。
        /// </summary>
        [Column("show_in_discovery_document")]
        public bool ShowInDiscoveryDocument { get; set; } = true;

        /// <summary>
        /// API密钥用于内省端点。API可以使用API名称和密钥进行内省验证。
        /// </summary>
        [Computed]
        public ICollection<ApiResourceSecret> Secrets { get; set; }

        /// <summary>
        /// API必须至少有一个范围。每个范围可以有不同的设置。
        /// </summary>
        [Computed]
        public ICollection<ApiResourceScope> Scopes { get; set; }

        /// <summary>
        /// 应包含在访问令牌中的关联用户声明类型的列表。
        /// </summary>
        [Computed]
        public ICollection<ApiResourceClaim> UserClaims { get; set; }

        /// <summary>
        /// 字典可根据需要保存任何自定义客户端特定值。
        /// </summary>
        [Computed]
        public ICollection<ApiResourceProperty> Properties { get; set; }

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
        /// 最近一次访问时间
        /// </summary>
        [Column("last_accessed")]
        public DateTime? LastAccessed { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        [Column("non_editable")]
        public bool NonEditable { get; set; }
    }
}

