using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.Stores;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using ApplicationEntity = Dawning.Identity.Domain.Aggregates.OpenIddict.Application;
using ScopeEntity = Dawning.Identity.Domain.Aggregates.OpenIddict.Scope;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// OpenIddict configuration
    /// </summary>
    public static class OpenIddictConfig
    {
        /// <summary>
        /// Add OpenIddict configuration
        /// </summary>
        public static IServiceCollection AddOpenIddictConfiguration(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment
        )
        {
            // Disable default claim type mapping to preserve original claim names
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Add default Authentication Scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            services.AddAuthorization();

            // Register custom Stores (based on Dapper Repository)
            services.AddScoped<
                IOpenIddictApplicationStore<ApplicationEntity>,
                OpenIddictApplicationStore
            >();
            services.AddScoped<IOpenIddictScopeStore<ScopeEntity>, OpenIddictScopeStore>();
            services.AddScoped<
                IOpenIddictAuthorizationStore<Authorization>,
                OpenIddictAuthorizationStore
            >();
            services.AddScoped<IOpenIddictTokenStore<Token>, OpenIddictTokenStore>();

            services
                .AddOpenIddict()
                // Register OpenIddict Core components
                .AddCore(options =>
                {
                    // Use custom Stores (based on Dapper + MySQL)
                    options
                        .SetDefaultApplicationEntity<ApplicationEntity>()
                        .SetDefaultScopeEntity<ScopeEntity>()
                        .SetDefaultAuthorizationEntity<Authorization>()
                        .SetDefaultTokenEntity<Token>();

                    // Register Quartz integration (for background tasks such as token cleanup)
                    options.UseQuartz();
                })
                // Register OpenIddict Server components
                .AddServer(options =>
                {
                    // Enable various OAuth 2.0 / OpenID Connect flows
                    options
                        .AllowPasswordFlow() // Password grant flow
                        .AllowClientCredentialsFlow() // Client credentials flow
                        .AllowAuthorizationCodeFlow() // Authorization code flow
                        .AllowRefreshTokenFlow(); // Refresh token flow

                    // Configure endpoints
                    options
                        .SetTokenEndpointUris("/connect/token")
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetRevocationEndpointUris("/connect/revoke");

                    // Register signing and encryption credentials
                    var useDevelopmentCertificate = configuration.GetValue<bool>(
                        "OpenIddict:UseDevelopmentCertificate"
                    );

                    // Fail fast: development certificates must never be used outside the development environment
                    if (useDevelopmentCertificate && !environment.IsDevelopment())
                    {
                        throw new InvalidOperationException(
                            $"OpenIddict:UseDevelopmentCertificate=true is not allowed in environment '{environment.EnvironmentName}'. Development certificates and disabling transport security are only permitted when the host is running in the Development environment."
                        );
                    }

                    if (useDevelopmentCertificate && environment.IsDevelopment())
                    {
                        // Use temporary certificates in development environment
                        options
                            .AddDevelopmentEncryptionCertificate()
                            .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        // Use real certificates in production environment
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
                        if (encryptionCert == null)
                        {
                            throw new InvalidOperationException(
                                "Encryption certificate could not be loaded; a dedicated encryption certificate is required in production (reusing the signing certificate is not allowed)"
                            );
                        }
                        options.AddEncryptionCertificate(encryptionCert);
                    }

                    // Register scopes
                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.OpenId,
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles,
                        "api"
                    );

                    // Configure token lifetime (read from configuration, can be overridden via database)
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

                    // Configure ASP.NET Core integration
                    var aspNetCoreBuilder = options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough();

                    // Only disable transport security in development; HTTPS must stay enforced in production
                    if (useDevelopmentCertificate && environment.IsDevelopment())
                    {
                        aspNetCoreBuilder.DisableTransportSecurityRequirement();
                    }
                })
                // Register OpenIddict Validation components
                .AddValidation(options =>
                {
                    // Import server configuration
                    options.UseLocalServer();

                    // Register ASP.NET Core integration
                    options.UseAspNetCore();

                    // Configure claim mappings - map 'role' claim to be recognized by ASP.NET Core
                    options.Configure(validationOptions =>
                    {
                        validationOptions.TokenValidationParameters.RoleClaimType = Claims.Role;
                        validationOptions.TokenValidationParameters.NameClaimType = Claims.Name;
                    });
                });

            return services;
        }
    }
}
