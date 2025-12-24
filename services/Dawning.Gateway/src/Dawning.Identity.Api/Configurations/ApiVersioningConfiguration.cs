using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// API 版本控制配置
    /// </summary>
    public static class ApiVersioningConfiguration
    {
        /// <summary>
        /// 添加 API 版本控制服务
        /// </summary>
        public static IServiceCollection AddApiVersioningConfiguration(
            this IServiceCollection services
        )
        {
            services.AddApiVersioning(options =>
            {
                // 当客户端未指定版本时，使用默认版本
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;

                // 在响应头中返回支持的版本
                options.ReportApiVersions = true;

                // 支持多种版本指定方式
                options.ApiVersionReader = ApiVersionReader.Combine(
                    // URL 路径版本: /api/v1/users
                    new UrlSegmentApiVersionReader(),
                    // 查询字符串版本: /api/users?api-version=1.0
                    new QueryStringApiVersionReader("api-version"),
                    // 请求头版本: X-Api-Version: 1.0
                    new HeaderApiVersionReader("X-Api-Version"),
                    // Media Type 版本: Accept: application/json;v=1.0
                    new MediaTypeApiVersionReader("v")
                );
            });

            services.AddVersionedApiExplorer(options =>
            {
                // 版本格式: 'v'major[.minor][-status]
                options.GroupNameFormat = "'v'VVV";

                // 替换 URL 中的版本占位符
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
