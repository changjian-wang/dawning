using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// Swagger 配置
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
                // 中文文档
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

                // 英文文档
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

                // 让所有 API 同时显示在两个文档中
                options.DocInclusionPredicate((docName, api) => true);

                // 添加 XML 注释
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // 添加 Application 层的 XML 注释
                var applicationXmlFile = "Dawning.Identity.Application.xml";
                var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
                if (File.Exists(applicationXmlPath))
                {
                    options.IncludeXmlComments(applicationXmlPath);
                }

                // API 分组标签描述
                options.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName };
                    }

                    var controllerName = api.ActionDescriptor.RouteValues["controller"];
                    return new[] { controllerName ?? "Other" };
                });

                // 添加 JWT Bearer 认证
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            @"
JWT 授权头使用 Bearer 方案。

在下面输入框中输入 **Bearer {你的token}**

示例: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

获取 Token 请使用 `/api/auth/connect/token` 接口",
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

                // 启用注解支持
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

                // UI 增强配置
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
