using System.IdentityModel.Tokens.Jwt;
using Dawning.Identity.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Dawning.Identity.Extensions;

/// <summary>
/// 服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Dawning 统一认证
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    /// <example>
    /// <code>
    /// // appsettings.json:
    /// // {
    /// //   "DawningAuth": {
    /// //     "Authority": "http://localhost:5202",
    /// //     "Issuer": "http://localhost:5202",
    /// //     "RequireHttpsMetadata": false
    /// //   }
    /// // }
    ///
    /// builder.Services.AddDawningAuthentication(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddDawningAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var options = new DawningAuthOptions();
        configuration.GetSection(DawningAuthOptions.SectionName).Bind(options);

        return services.AddDawningAuthentication(options);
    }

    /// <summary>
    /// 添加 Dawning 统一认证 (使用配置委托)
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddDawningAuthentication(options =>
    /// {
    ///     options.Authority = "http://localhost:5202";
    ///     options.Issuer = "http://localhost:5202";
    ///     options.RequireHttpsMetadata = false;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddDawningAuthentication(
        this IServiceCollection services,
        Action<DawningAuthOptions> configure
    )
    {
        var options = new DawningAuthOptions();
        configure(options);

        return services.AddDawningAuthentication(options);
    }

    /// <summary>
    /// 添加 Dawning 统一认证 (使用选项对象)
    /// </summary>
    private static IServiceCollection AddDawningAuthentication(
        this IServiceCollection services,
        DawningAuthOptions options
    )
    {
        // 验证必填配置
        if (string.IsNullOrWhiteSpace(options.Authority))
            throw new ArgumentException("DawningAuth:Authority is required");
        if (string.IsNullOrWhiteSpace(options.Issuer))
            throw new ArgumentException("DawningAuth:Issuer is required");

        // 清除默认的 Claim 类型映射 (避免 sub -> nameidentifier 等转换)
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // 注册配置选项
        services.AddSingleton(options);

        // 配置认证
        services
            .AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = options.Authority;
                jwtOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;

                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    // 签发者验证
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,

                    // 受众验证 (可选)
                    ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
                    ValidAudience = options.Audience,

                    // 过期验证
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(options.ClockSkewMinutes),

                    // 签名验证 (从 Authority 自动获取公钥)
                    ValidateIssuerSigningKey = true,

                    // 保持原始 Claim 名称
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };

                // 事件处理
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // 记录认证失败原因
                        var logger = context
                            .HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()
                            ?.CreateLogger("DawningAuthentication");

                        logger?.LogWarning(
                            context.Exception,
                            "Authentication failed: {Message}",
                            context.Exception.Message
                        );

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Token 验证成功后的处理 (可扩展)
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // 自定义 401 响应 (可选)
                        return Task.CompletedTask;
                    },
                };
            });

        // 注册当前用户服务
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// 添加 Dawning 授权策略
    /// </summary>
    public static IServiceCollection AddDawningAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // 基础策略
            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

            // 角色策略
            options.AddPolicy(
                "SuperAdmin",
                policy => policy.RequireRole(Constants.DawningRoles.SuperAdmin)
            );

            options.AddPolicy(
                "Admin",
                policy =>
                    policy.RequireRole(
                        Constants.DawningRoles.Admin,
                        Constants.DawningRoles.SuperAdmin
                    )
            );

            options.AddPolicy(
                "AdminOrAuditor",
                policy =>
                    policy.RequireRole(
                        Constants.DawningRoles.Admin,
                        Constants.DawningRoles.SuperAdmin,
                        Constants.DawningRoles.Auditor
                    )
            );
        });

        return services;
    }
}
