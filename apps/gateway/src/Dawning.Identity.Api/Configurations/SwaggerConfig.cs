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
        private const string TitleZh = "Dawning Identity API (中文)";
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
## Dawning Gateway 身份认证 API

Dawning Gateway 是一个功能完备的 API 网关和身份认证系统，提供以下核心功能：

### 🔐 身份认证 (Authentication)
- OAuth 2.0 / OpenID Connect 标准协议
- JWT Token 认证和刷新
- 登录失败锁定保护
- 密码强度策略验证

### 👥 用户管理 (User Management)
- 用户 CRUD 操作
- 角色和权限管理
- 密码修改和重置

### 🌐 网关配置 (Gateway Configuration)
- 动态路由配置
- 集群管理
- 健康检查

### 📊 系统管理 (System Administration)
- 系统配置管理
- 审计日志
- 健康状态监控

### ⚡ 错误码说明
| 错误码 | 描述 |
|--------|------|
| 400 | 请求参数错误 |
| 401 | 未授权（需要登录） |
| 403 | 权限不足 |
| 404 | 资源不存在 |
| 409 | 资源冲突（如用户名已存在） |
| 429 | 请求过于频繁 |
| 500 | 服务器内部错误 |
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
