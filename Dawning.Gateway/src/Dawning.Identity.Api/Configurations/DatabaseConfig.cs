using Dawning.Identity.Infra.Data.Context;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public static class DatabaseConfig
    {
        public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            string? connectionString = configuration.GetConnectionString("MySQL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("MySQL connection string cannot be empty! Please check appsettings.json");
            }

            // 注册 DbContext（Scoped 生命周期）
            services.AddScoped(provider => new DbContext(connectionString));
        }
    }
}
