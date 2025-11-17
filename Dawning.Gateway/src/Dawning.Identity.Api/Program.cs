namespace Dawning.Identity.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // AppSettings
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            // 配置数据库（使用Dapper）
            // builder.Services.AddDbContext<IdentityDbContext>

            // IoC
            // builder.Services.AddDependencyInjectionConfiguration(builder.Configuration);
        }
    }
}