using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// API versioning configuration
    /// </summary>
    public static class ApiVersioningConfiguration
    {
        /// <summary>
        /// Add API versioning services
        /// </summary>
        public static IServiceCollection AddApiVersioningConfiguration(
            this IServiceCollection services
        )
        {
            services.AddApiVersioning(options =>
            {
                // Use default version when client does not specify version
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;

                // Return supported versions in response header
                options.ReportApiVersions = true;

                // Support multiple version specification methods
                options.ApiVersionReader = ApiVersionReader.Combine(
                    // URL path version: /api/v1/users
                    new UrlSegmentApiVersionReader(),
                    // Query string version: /api/users?api-version=1.0
                    new QueryStringApiVersionReader("api-version"),
                    // Request header version: X-Api-Version: 1.0
                    new HeaderApiVersionReader("X-Api-Version"),
                    // Media Type version: Accept: application/json;v=1.0
                    new MediaTypeApiVersionReader("v")
                );
            });

            services.AddVersionedApiExplorer(options =>
            {
                // Version format: 'v'major[.minor][-status]
                options.GroupNameFormat = "'v'VVV";

                // Replace version placeholder in URL
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
