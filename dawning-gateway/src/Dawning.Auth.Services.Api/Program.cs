using Dawning.Auth.Services.Api.Configurations;

namespace Dawning.Auth.Services.Api
{
    /// <summary>
    /// The Program class is the entry point of the application. It sets up and configures the web application, including dependency injection, database, AutoMapper, controllers, routing, CORS, and Swagger documentation. This class also handles the execution of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point of the application. This method sets up and configures the web application, including dependency injection, database, AutoMapper, controllers, routing, CORS, and Swagger documentation, and then runs the application.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // AppSettings
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            // IoC
            builder.Services.AddDependencyInjectionConfiguration();

            // DB Config
            builder.Services.AddDatabaseConfiguration(builder.Configuration);

            // AutoMapper
            builder.Services.AddAutoMapperConfiguration();

            // Add Controllers
            builder.Services.AddControllers();

            // Add Routing
            builder.Services.Configure<RouteOptions>(options =>
            {
                // 组合使用官方转换器
                options.LowercaseUrls = true;      // 强制小写
                options.LowercaseQueryStrings = true; // 可选
            });

            // CORS
            builder.Services.AddCors(c =>
            {
                c.AddDefaultPolicy(options =>
                    options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            // SwaggerGen
            builder.Services.AddSwaggerConfiguration();

            // Run app
            var app = builder.Build();

            if(app.Environment.IsDevelopment())
            {
                app.UseSwaggerSetup();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Use Controller Configuration
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}