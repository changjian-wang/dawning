using Dawning.Auth.Dapper.Contrib;
using Dawning.Auth.Domain.Enums;

namespace Dawning.Auth.Domain.Entities.IdentityServer
{
    /// <summary>
    /// Client
    /// </summary>
    [Table("clients")]
    public class Client
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 指定是否启用客户端。默认值为 true。
        /// </summary>
        [Column("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 客户端的唯一 ID
        /// </summary>
        [Column("client_id")]
        [IgnoreUpdate]
        public string? ClientId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column("protocol_type")]
        public string? ProtocolType { get; set; } = "oidc";

        /// <summary>
        /// 客户端密码列表 - 用于访问令牌终结点的凭据。
        /// </summary>
        [Computed]
        public required ICollection<ClientSecret> ClientSecrets { get; set; }

        /// <summary>
        /// 指定此客户端是否需要机密才能从令牌终结点请求令牌（默认为true)
        /// </summary>
        [Column("require_client_secret")]
        public bool RequireClientSecret { get; set; } = true;

        /// <summary>
        /// 客户端显示名称（用于日志记录和同意屏幕）
        /// </summary>
        [Column("client_name")]
        public string? ClientName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 有关客户端的更多信息的 URI（在同意屏幕上使用）
        /// </summary>
        [Column("client_uri")]
        public string? ClientUri { get; set; }

        /// <summary>
        /// 客户端徽标的 URI（在同意屏幕上使用）
        /// </summary>
        [Column("logo_uri")]
        public string? LogoUri { get; set; }

        /// <summary>
        /// 指定是否需要同意屏幕。默认为false。
        /// </summary>
        [Column("require_consent")]
        public bool RequireConsent { get; set; } = false;

        /// <summary>
        /// 指定用户是否可以选择存储同意决策。默认为true。
        /// </summary>
        [Column("allow_remember_consent")]
        public bool AllowRememberConsent { get; set; } = true;

        /// <summary>
        /// 请求 id 令牌和访问令牌时，应始终将用户声明添加到 id 令牌中，而不是要求客户端使用 userinfo 终结点。默认值为false。
        /// </summary>
        [Column("always_include_user_claims_in_id_token")]
        public bool AlwaysIncludeUserClaimsInIdToken { get; set; } = false;

        /// <summary>
        /// 指定允许客户端使用的授权类型。将该类用于常见组合。GrantTypes
        /// </summary>
        [Column("allowed_grant_types")]
        public required ICollection<ClientGrantType> AllowedGrantTypes { get; set; }

        /// <summary>
        /// 指定使用基于授权代码的授权类型(HyBrid)的客户端是否必须发送证明密钥（默认为true）。
        /// </summary>
        [Column("require_pkce")]
        public bool RequirePkce { get; set; } = true;

        /// <summary>
        /// 指定使用 PKCE 的客户端是否可以使用纯文本代码质询（不建议 - 默认为false)
        /// </summary>
        [Column("allow_plain_text_pkce")]
        public bool AllowPlainTextPkce { get; set; } = false;

        /// <summary>
        /// 指定此客户端是否需要将授权请求参数包装在 JWT 中（默认为false)
        /// </summary>
        [Column("require_request_object")]
        public bool RequireRequestObject { get; set; } = false;

        /// <summary>
        /// 指定是否允许此客户端通过浏览器接收访问令牌。 这对于强化允许多种响应类型的流非常有用 （例如，通过禁止应该使用代码id_token添加令牌响应类型的混合流客户端 从而将令牌泄漏到浏览器。
        /// </summary>
        [Column("allow_access_tokens_via_browser")]
        public bool AllowAccessTokensViaBrowser { get; set; } = true;

        /// <summary>
        /// 指定允许将令牌或授权代码返回到的 URI
        /// </summary>
        [Computed]
        public required ICollection<ClientRedirectUri> RedirectUris { get; set; }

        /// <summary>
        /// 指定注销后允许重定向到的 URI。
        /// </summary>
        [Computed]
        public required ICollection<ClientPostLogoutRedirectUri> PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// 在客户端为基于 HTTP 的前端通道注销指定注销 URI。
        /// </summary>
        [Column("front_channel_logout_uri")]
        public string? FrontChannelLogoutUri { get; set; }

        /// <summary>
        /// 指定是否应将用户的会话 ID 发送到 FrontChannelLogoutUri。默认值为 true。
        /// </summary>
        [Column("front_channel_logout_session_required")]
        public bool FrontChannelLogoutSessionRequired { get; set; } = true;

        /// <summary>
        /// 在客户端为基于 HTTP 的反向通道注销指定注销 URI。
        /// </summary>
        [Column("back_channel_logout_uri")]
        public string? BackChannelLogoutUri { get; set; }

        /// <summary>
        /// 指定是否应在向 BackChannelLogoutUri 的请求中发送用户的会话 ID。默认值为 true。
        /// </summary>
        [Column("back_channel_logout_session_required")]
        public bool BackChannelLogoutSessionRequired { get; set; } = true;

        /// <summary>
        /// 指定此客户端是否可以请求刷新令牌（请求范围）offline_access
        /// </summary>
        [Column("allow_offline_access")]
        public bool AllowOfflineAccess { get; set; } = false;

        /// <summary>
        /// 默认情况下，客户端无权访问任何资源 - 通过添加相应的作用域名称来指定允许的资源
        /// </summary>
        [Computed]
        public required ICollection<ClientScope> AllowedScopes { get; set; }

        /// <summary>
        /// 标识令牌的生存期（以秒为单位）（默认为 300秒/5分钟）
        /// </summary>
        [Column("identity_token_lifetime")]
        public int IdentityTokenLifetime { get; set; } = 300;

        /// <summary>
        /// 标识令牌允许的签名算法列表。如果为空，则将使用服务器默认签名算法RS256。推荐RS256或ES256
        /// </summary>
        [Column("allowed_identity_token_signing_algorithms")]
        public string? AllowedIdentityTokenSigningAlgorithms { get; set; } = "RS256,ES256";

        /// <summary>
        /// 访问令牌的生存期（以秒为单位）（默认为 3600秒/1小时）
        /// </summary>
        [Column("access_token_lifetime")]
        public int AccessTokenLifetime { get; set; } = 3600;

        /// <summary>
        /// 授权代码的生存期（以秒为单位）（默认为 300秒/5分钟）
        /// </summary>
        [Column("authorization_code_lifetime")]
        public int AuthorizationCodeLifetime { get; set; } = 300;

        /// <summary>
        /// 用户同意的生存期（以秒为单位）。默认值为 null（无过期）。
        /// </summary>
        [Column("consent_lifetime")]
        public int? ConsentLifetime { get; set; } = null;

        /// <summary>
        /// 刷新令牌的最长生存期（以秒为单位）。默认值为 2592000秒/30天
        /// </summary>
        [Column("absolute_refresh_token_lifetime")]
        public int AbsoluteRefreshTokenLifetime { get; set; } = 2592000;

        /// <summary>
        /// 刷新令牌的滑动生存期（以秒为单位）。默认值为 1296000秒/15天
        /// </summary>
        [Column("sliding_refresh_token_lifetime")]
        public int SlidingRefreshTokenLifetime { get; set; } = 1296000;

        /// <summary>
        /// ReUse刷新令牌时，刷新令牌句柄将保持不变
        /// OneTime刷新令牌时，刷新令牌句柄将更新。这是默认设置。
        /// </summary>
        [Column("refresh_token_usage")]
        public int RefreshTokenUsage { get; set; } = (int)TokenUsage.OneTimeOnly;

        /// <summary>
        /// 获取或设置一个值，该值指示是否应在刷新令牌请求上更新访问令牌（及其声明）。
        /// </summary>
        [Column("update_access_token_claims_on_refresh")]
        public bool UpdateAccessTokenClaimsOnRefresh { get; set; } = false;

        /// <summary>
        /// Absolute刷新令牌将在固定时间点（由 AbsoluteRefreshTokenLifetime 指定）过期。这是默认设置。
        /// Sliding刷新令牌时，刷新令牌的生存期将续订（按 SlidingRefreshTokenLifetime 中指定的量）。生存期不会超过 AbsoluteRefreshTokenLifetime。
        /// </summary>
        [Column("refresh_token_expiration")]
        public int RefreshTokenExpiration { get; set; } = (int)TokenExpiration.Absolute;

        /// <summary>
        /// 指定访问令牌是引用令牌还是自包含的 JWT 令牌（默认为 Jwt）。
        /// </summary>
        [Column("access_token_type")]
        public int AccessTokenType { get; set; } = 0; // AccessTokenType.Jwt;

        /// <summary>
        /// 指定此客户端是可以使用本地帐户，还是只能使用外部 IdP。默认值为 true。
        /// </summary>
        [Column("enable_local_login")]
        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>
        /// 指定哪些外部 IdP 可以与此客户端一起使用（如果列表为空，则允许所有 IdP）。默认为空。
        /// </summary>
        [Computed]
        public required ICollection<ClientIdPRestriction> IdentityProviderRestrictions { get; set; }

        /// <summary>
        /// 指定 JWT 访问令牌是否应具有嵌入的唯一 ID（通过 jti 声明）。默认为true
        /// </summary>
        [Column("include_jwt_id")]
        public bool IncludeJwtId { get; set; } = true;

        /// <summary>
        /// 允许客户端的设置声明（将包含在访问令牌中）。
        /// </summary>
        [Computed]
        public required ICollection<ClientClaim> Claims { get; set; }

        /// <summary>
        /// 如果设置，将为每个流发送客户端声明。如果不是，则仅适用于客户端凭据流（默认值为 false)
        /// </summary>
        [Column("always_send_client_claims")]
        public bool AlwaysSendClientClaims { get; set; } = false;

        /// <summary>
        /// 如果设置，则前缀客户端声明类型将以为前缀。默认为 client_。目的是确保它们不会意外地与用户声明发生冲突。
        /// </summary>
        [Column("client_claims_prefix")]
        public string? ClientClaimsPrefix { get; set; } = "client_";

        /// <summary>
        /// 为此客户端的用户生成成对 subjectId 时使用的 Salt 值。
        /// </summary>
        [Column("pair_wise_subject_salt")]
        public string? PairWiseSubjectSalt { get; set; }

        /// <summary>
        /// 如果指定，默认 CORS 策略服务实现（内存中和 EF）将用于为 JavaScript 客户端生成 CORS 策略。
        /// </summary>
        [Computed]
        public required ICollection<ClientCorsOrigin> AllowedCorsOrigins { get; set; }

        /// <summary>
        /// 字典，用于根据需要保存任何自定义客户端特定的值。
        /// </summary>
        [Computed]
        public required ICollection<ClientProperty> Properties { get; set; }

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
        /// 自用户上次进行身份验证以来的最长持续时间（以秒为单位）。默认为null。 您可以调整会话令牌的生存期，以控制用户在使用 Web 应用程序时需要重新输入凭据的时间和频率，而不是以静默方式进行身份验证。
        /// </summary>
        [Column("user_sso_lifetime")]
        public int? UserSsoLifetime { get; set; } = null;

        /// <summary>
        /// 指定要用于客户端的用户代码的类型。否则，将回退到默认值。
        /// </summary>
        [Column("user_code_type")]
        public string? UserCodeType { get; set; } = "";

        /// <summary>
        /// 设备代码的生存期（以秒为单位）（默认为 300 秒/5 分钟）
        /// </summary>
        [Column("device_code_lifetime")]
        public int DeviceCodeLifetime { get; set; } = 300;

        /// <summary>
        /// 是否可编辑
        /// </summary>
        [Column("non_editable")]
        [IgnoreUpdate]
        public bool NonEditable { get; set; }
    }
}
