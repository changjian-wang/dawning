using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// Swagger 配置
    /// </summary>
    public static class SwaggerConfig
    {
        private const string Title = "Dawning Identity API";
        private const string Name = "v1";

        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(Name, new OpenApiInfo
                {
                    Title = Title,
                    Version = Name,
                    Description = "Dawning Identity 身份认证 API",
                    Contact = new OpenApiContact
                    {
                        Name = "Dawning Team",
                        Email = "support@dawning.com"
                    }
                });

                // 添加 XML 注释
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // 添加 JWT Bearer 认证
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public static void UseSwaggerSetup(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "swagger";
                options.SwaggerEndpoint($"/swagger/{Name}/swagger.json", Title);
                options.DocumentTitle = Title;
            });
        }
    }
}
