using Dawning.Identity.Api.Configurations;
using Serilog;

namespace Dawning.Identity.Api
{
    public class Program
    {
        public static void Main(string[] args)
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

            // ===== Database Configuration =====
            builder.Services.AddDatabaseConfiguration(builder.Configuration);

            // ===== AutoMapper =====
            builder.Services.AddAutoMapperConfiguration();

            // ===== Controllers =====
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serialization options
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // keep original property names
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
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");
            // app.UseAuthentication();  // If authentication is needed
            // app.UseAuthorization();   // If authorization is needed

            // ===== Map Endpoints =====
            app.MapControllers();
            app.MapHealthChecks("/health");

            // Error handling endpoint
            app.Map("/error", (HttpContext httpContext) =>
            {
                httpContext.Response.StatusCode = 500;
                return Results.Problem("An unexpected error occurred.");
            });

            // Run the app
            app.Run();
        }
    }
}