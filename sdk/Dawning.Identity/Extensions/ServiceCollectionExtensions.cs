using System.IdentityModel.Tokens.Jwt;
using Dawning.Identity.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Dawning.Identity.Extensions;

/// <summary>
/// Service registration extension methods
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Dawning unified authentication
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
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
    /// Adds Dawning unified authentication (with configuration delegate)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
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
    /// Adds Dawning unified authentication (with options object)
    /// </summary>
    private static IServiceCollection AddDawningAuthentication(
        this IServiceCollection services,
        DawningAuthOptions options
    )
    {
        // Validate required configuration
        if (string.IsNullOrWhiteSpace(options.Authority))
            throw new ArgumentException("DawningAuth:Authority is required");
        if (string.IsNullOrWhiteSpace(options.Issuer))
            throw new ArgumentException("DawningAuth:Issuer is required");

        // Clear default claim type mappings (avoid sub -> nameidentifier conversions)
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // Register configuration options
        services.AddSingleton(options);

        // Configure authentication
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
                    // Issuer validation
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,

                    // Audience validation (optional)
                    ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
                    ValidAudience = options.Audience,

                    // Expiration validation
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(options.ClockSkewMinutes),

                    // Signature validation (automatically gets public key from Authority)
                    ValidateIssuerSigningKey = true,

                    // Keep original claim names
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };

                // Event handling
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failure reason
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
                        // Processing after token validation success (extensible)
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Custom 401 response (optional)
                        return Task.CompletedTask;
                    },
                };
            });

        // Register current user service
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// Adds Dawning authorization policies
    /// </summary>
    public static IServiceCollection AddDawningAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Basic policy
            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

            // Role policies
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
