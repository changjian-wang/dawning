using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.Stores;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using ApplicationEntity = Dawning.Identity.Domain.Aggregates.OpenIddict.Application;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// OpenIddict 配置
    /// </summary>
    public static class OpenIddictConfig
    {
        /// <summary>
        /// 添加 OpenIddict 配置
        /// </summary>
        public static IServiceCollection AddOpenIddictConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // 添加默认 Authentication Scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            // 注册自定义 Stores（基于 Dapper Repository）
            services.AddScoped<
                IOpenIddictApplicationStore<ApplicationEntity>,
                OpenIddictApplicationStore
            >();
            services.AddScoped<IOpenIddictScopeStore<Scope>, OpenIddictScopeStore>();
            services.AddScoped<
                IOpenIddictAuthorizationStore<Authorization>,
                OpenIddictAuthorizationStore
            >();
            services.AddScoped<IOpenIddictTokenStore<Token>, OpenIddictTokenStore>();

            services
                .AddOpenIddict()
                // 注册 OpenIddict Core 组件
                .AddCore(options =>
                {
                    // 使用自定义 Stores（基于 Dapper + MySQL）
                    options
                        .SetDefaultApplicationEntity<ApplicationEntity>()
                        .SetDefaultScopeEntity<Scope>()
                        .SetDefaultAuthorizationEntity<Authorization>()
                        .SetDefaultTokenEntity<Token>();

                    // 注册 Quartz 集成（用于令牌清理等后台任务）
                    options.UseQuartz();
                })
                // 注册 OpenIddict Server 组件
                .AddServer(options =>
                {
                    // 启用各种 OAuth 2.0 / OpenID Connect 流程
                    options
                        .AllowPasswordFlow() // 密码授权流程
                        .AllowClientCredentialsFlow() // 客户端凭证流程
                        .AllowAuthorizationCodeFlow() // 授权码流程
                        .AllowRefreshTokenFlow(); // 刷新令牌流程

                    // 配置端点
                    options
                        .SetTokenEndpointUris("/connect/token")
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetRevocationEndpointUris("/connect/revoke");

                    // 注册签名和加密凭据
                    if (configuration.GetValue<bool>("OpenIddict:UseDevelopmentCertificate"))
                    {
                        // 开发环境使用临时证书
                        options
                            .AddDevelopmentEncryptionCertificate()
                            .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        // 生产环境使用真实证书
                        var certConfig = configuration
                            .GetSection("OpenIddict:Certificates")
                            .Get<CertificateConfig>();

                        if (certConfig == null)
                        {
                            throw new InvalidOperationException(
                                "Certificate configuration is required when UseDevelopmentCertificate=false"
                            );
                        }

                        var signingCert = CertificateLoader.LoadCertificate(certConfig.Signing);
                        if (signingCert == null)
                        {
                            throw new InvalidOperationException(
                                "Signing certificate could not be loaded"
                            );
                        }
                        options.AddSigningCertificate(signingCert);

                        var encryptionCert = CertificateLoader.LoadCertificate(
                            certConfig.Encryption
                        );
                        if (encryptionCert != null)
                        {
                            options.AddEncryptionCertificate(encryptionCert);
                        }
                        else
                        {
                            // 如果没有配置加密证书，使用签名证书
                            options.AddEncryptionCertificate(signingCert);
                        }
                    }

                    // 注册作用域
                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles,
                        "api"
                    );

                    // 配置令牌生命周期（从配置文件读取，可通过数据库覆盖）
                    var accessTokenLifetimeMinutes = configuration.GetValue<int>(
                        "OpenIddict:AccessTokenLifetimeMinutes",
                        60
                    );
                    var refreshTokenLifetimeDays = configuration.GetValue<int>(
                        "OpenIddict:RefreshTokenLifetimeDays",
                        7
                    );
                    var identityTokenLifetimeMinutes = configuration.GetValue<int>(
                        "OpenIddict:IdentityTokenLifetimeMinutes",
                        10
                    );

                    options.SetAccessTokenLifetime(
                        TimeSpan.FromMinutes(accessTokenLifetimeMinutes)
                    );
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(refreshTokenLifetimeDays));
                    options.SetIdentityTokenLifetime(
                        TimeSpan.FromMinutes(identityTokenLifetimeMinutes)
                    );

                    // 配置 ASP.NET Core 集成
                    options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .DisableTransportSecurityRequirement(); // 仅在开发环境禁用，生产环境应该启用 HTTPS
                })
                // 注册 OpenIddict Validation 组件
                .AddValidation(options =>
                {
                    // 导入服务器配置
                    options.UseLocalServer();

                    // 注册 ASP.NET Core 集成
                    options.UseAspNetCore();
                });

            return services;
        }
    }
}
