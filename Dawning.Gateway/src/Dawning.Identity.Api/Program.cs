using Dawning.Identity.Api.Configurations;
using Serilog;

namespace Dawning.Identity.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== Serilog Configuration =====
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/identity-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // AppSettings
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            // ===== Dependency Injection =====
            builder.Services.AddDependencyInjectionConfiguration();

            // ===== OpenIddict Configuration =====
            // 使用基于 Dapper + MySQL 的自定义 Stores
            builder.Services.AddOpenIddictConfiguration(builder.Configuration);

            // ===== User Authentication Service =====
            builder.Services.AddScoped<Dawning.Identity.Application.Interfaces.Authentication.IUserAuthenticationService, 
                Dawning.Identity.Application.Services.Authentication.UserAuthenticationService>();

            // ===== Audit Log Helper =====
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<Dawning.Identity.Api.Helpers.AuditLogHelper>();

            // ===== Database Configuration =====
            builder.Services.AddDatabaseConfiguration(builder.Configuration);

            // ===== AutoMapper =====
            builder.Services.AddAutoMapperConfiguration();

            // ===== Controllers =====
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serialization options
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // use camelCase for JSON
                    options.JsonSerializerOptions.WriteIndented = true; // format output
                });

            // ===== Routing =====
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;           // URL lowercase
                options.LowercaseQueryStrings = false;   // Query strings not lowercase
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ===== Swagger =====
            builder.Services.AddSwaggerConfiguration();

            // ===== Health Checks =====
            builder.Services.AddHealthChecks();

            // ===== Database Seeder =====
            builder.Services.AddScoped<Dawning.Identity.Api.Data.DatabaseSeeder>();

            // Build the app
            var app = builder.Build();

            // Development environment settings
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerSetup();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Serilog request logging
            app.UseSerilogRequestLogging();

            // ===== Middleware Configuration =====
            // 异常处理中间件（必须在最前面）
            app.UseMiddleware<Dawning.Identity.Api.Middleware.ExceptionHandlingMiddleware>();
            
            // 请求日志中间件
            app.UseMiddleware<Dawning.Identity.Api.Middleware.RequestLoggingMiddleware>();
            
            // 性能监控中间件
            app.UseMiddleware<Dawning.Identity.Api.Middleware.PerformanceMonitoringMiddleware>();
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");
            
            // 启用认证和授权
            app.UseAuthentication();
            app.UseAuthorization();

            // ===== Map Endpoints =====
            app.MapControllers();
            app.MapHealthChecks("/health");

            // Error handling endpoint
            app.Map("/error", (HttpContext httpContext) =>
            {
                httpContext.Response.StatusCode = 500;
                return Results.Problem("An unexpected error occurred.");
            });

            // ===== Database Seeding =====
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<Dawning.Identity.Api.Data.DatabaseSeeder>();
                try
                {
                    await seeder.SeedAsync();
                    Log.Information("Database seeding completed successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while seeding the database");
                }
            }

            // Run the app
            app.Run();
        }
    }
}