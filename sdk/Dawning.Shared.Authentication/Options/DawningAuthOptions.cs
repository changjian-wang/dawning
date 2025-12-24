namespace Dawning.Shared.Authentication.Options;

/// <summary>
/// Dawning 认证配置选项
/// </summary>
public class DawningAuthOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "DawningAuth";

    /// <summary>
    /// Identity 服务器地址 (用于获取公钥验证 Token)
    /// 例如: https://identity.example.com 或 http://localhost:5202
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Token 签发者 (必须与 Identity 服务配置一致)
    /// 例如: https://identity.example.com
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 可选的受众验证 (留空则不验证)
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// 是否要求 HTTPS (生产环境应为 true)
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Token 过期时间容差 (分钟)
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// 是否启用角色验证
    /// </summary>
    public bool ValidateRoles { get; set; } = true;

    /// <summary>
    /// 是否启用权限验证
    /// </summary>
    public bool ValidatePermissions { get; set; } = true;

    /// <summary>
    /// 是否启用租户验证
    /// </summary>
    public bool ValidateTenant { get; set; } = false;

    /// <summary>
    /// API Gateway 地址 (用于服务间通信)
    /// </summary>
    public string? GatewayUrl { get; set; }
}
