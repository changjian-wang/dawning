using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// Swagger configuration
    /// </summary>
    public static class SwaggerConfig
    {
        private const string VersionZh = "v1-zh";
        private const string VersionEn = "v1-en";
        private const string TitleZh = "Dawning Identity API (Chinese)";
        private const string TitleEn = "Dawning Identity API (English)";

        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSwaggerGen(options =>
            {
                // Chinese documentation
                options.SwaggerDoc(
                    VersionZh,
                    new OpenApiInfo
                    {
                        Title = "Dawning Identity API",
                        Version = "v1",
                        Description = GetChineseDescription(),
                        Contact = new OpenApiContact
                        {
                            Name = "Dawning Team",
                            Email = "support@dawning.com",
                            Url = new Uri("https://github.com/dawning-gateway"),
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT"),
                        },
                    }
                );

                // English documentation
                options.SwaggerDoc(
                    VersionEn,
                    new OpenApiInfo
                    {
                        Title = "Dawning Identity API",
                        Version = "v1",
                        Description = GetEnglishDescription(),
                        Contact = new OpenApiContact
                        {
                            Name = "Dawning Team",
                            Email = "support@dawning.com",
                            Url = new Uri("https://github.com/dawning-gateway"),
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT"),
                        },
                    }
                );

                // Display all APIs in both documents
                options.DocInclusionPredicate((docName, api) => true);

                // Add XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Add Application layer XML comments
                var applicationXmlFile = "Dawning.Identity.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
                if (File.Exists(applicationXmlPath))
                {
                    options.IncludeXmlComments(applicationXmlPath);
                }

                // API group tag description
                options.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName };
                    }

                    var controllerName = api.ActionDescriptor.RouteValues["controller"];
                    return new[] { controllerName ?? "Other" };
                });

                // Add JWT Bearer authentication
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            @"
JWT Authorization header using the Bearer scheme.

Enter **Bearer {your_token}** in the input box below

Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

To obtain a Token, use the `/api/auth/connect/token` endpoint",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                    }
                );

                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                            },
                            Array.Empty<string>()
                        },
                    }
                );

                // Enable annotation support
                options.EnableAnnotations();
            });
        }

        public static void UseSwaggerSetup(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            app.UseSwagger(options =>
            {
                options.SerializeAsV2 = false;
            });

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "";
                options.SwaggerEndpoint($"/swagger/{VersionEn}/swagger.json", TitleEn);
                options.SwaggerEndpoint($"/swagger/{VersionZh}/swagger.json", TitleZh);
                options.DocumentTitle = "Dawning Identity API";

                // UI enhancement configuration
                options.DefaultModelsExpandDepth(2);
                options.DefaultModelExpandDepth(2);
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
                options.EnableTryItOutByDefault();
            });
        }

        private static string GetChineseDescription()
        {
            return @"
## Dawning Gateway Identity API (Chinese Version)

Dawning Gateway is a fully-featured API gateway and identity authentication system with the following core features:

### 🔐 Authentication
- OAuth 2.0 / OpenID Connect standard protocols
- JWT Token authentication and refresh
- Login failure lockout protection
- Password strength policy validation

### 👥 User Management
- User CRUD operations
- Role and permission management
- Password change and reset

### 🌐 Gateway Configuration
- Dynamic routing configuration
- Cluster management
- Health checks

### 📊 System Administration
- System configuration management
- Audit logs
- Health status monitoring

### ⚡ Error Codes
| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid parameters |
| 401 | Unauthorized - Login required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource does not exist |
| 409 | Conflict - Resource already exists |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error |
";
        }

        private static string GetEnglishDescription()
        {
            return @"
## Dawning Gateway Identity API

Dawning Gateway is a fully-featured API gateway and identity authentication system with the following core features:

### 🔐 Authentication
- OAuth 2.0 / OpenID Connect standard protocols
- JWT Token authentication and refresh
- Login failure lockout protection
- Password strength policy validation

### 👥 User Management
- User CRUD operations
- Role and permission management
- Password change and reset

### 🌐 Gateway Configuration
- Dynamic routing configuration
- Cluster management
- Health checks

### 📊 System Administration
- System configuration management
- Audit logs
- Health status monitoring

### ⚡ Error Codes
| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid parameters |
| 401 | Unauthorized - Login required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource does not exist |
| 409 | Conflict - Resource already exists |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error |
";
        }
    }
}
