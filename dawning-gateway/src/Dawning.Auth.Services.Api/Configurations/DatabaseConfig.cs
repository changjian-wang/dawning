using Dawning.Auth.Infra.CrossCutting.IoC;
using Dawning.Auth.Infra.Data.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Auth.Services.Api.Configurations
{
    public static class DatabaseConfig
    {
        public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            string? connectionString = configuration.GetConnectionString("MySQL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("DB connection string can't be empty!");
            }
            else
            {
                services.AddScoped(provider => new DbContext(connectionString));
            }
        }
    }
}
