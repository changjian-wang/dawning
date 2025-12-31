using Dawning.Identity.Infra.Data.Context;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// Database configuration
    /// </summary>
    public static class DatabaseConfig
    {
        public static void AddDatabaseConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            string? connectionString = configuration.GetConnectionString("MySQL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    "MySQL connection string cannot be empty! Please check appsettings.json"
                );
            }

            // Register DbContext (Scoped lifetime)
            services.AddScoped(provider => new DbContext(connectionString));
        }
    }
}
