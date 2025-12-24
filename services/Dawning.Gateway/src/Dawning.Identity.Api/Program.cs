using Dawning.Identity.Api.Configurations;
using Dawning.Identity.Api.Middleware;
using Dawning.Identity.Infra.Messaging;
using Serilog;

namespace Dawning.Identity.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== Kestrel Configuration =====
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
                // 禁用响应体长度验证，避免 OpenIddict Content-Length mismatch 错误
                options.AllowSynchronousIO = true;
            });

            // ===== Serilog Configuration =====
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/identity-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // AppSettings
            builder
                .Configuration.SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            // ===== Dependency Injection =====
            builder.Services.AddDependencyInjectionConfiguration();

            // ===== OpenIddict Configuration =====
            // 使用基于 Dapper + MySQL 的自定义 Stores
            builder.Services.AddOpenIddictConfiguration(builder.Configuration);

            // ===== Security Services =====
            builder.Services.AddScoped<
                Application.Interfaces.Security.ILoginLockoutService,
                Application.Services.Security.LoginLockoutService
            >();
            builder.Services.AddScoped<
                Application.Interfaces.Security.IPasswordPolicyService,
                Application.Services.Security.PasswordPolicyService
            >();
            builder.Services.AddSingleton<
                Domain.Core.Security.IDataEncryptionService,
                Domain.Core.Security.AesDataEncryptionService
            >();

            // ===== Token Management Services =====
            // 配置 Redis 分布式缓存（如果配置了连接字符串）
            var redisConnection = builder.Configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "DawningIdentity:";
                });
                builder.Services.AddScoped<
                    Application.Interfaces.Token.ITokenBlacklistService,
                    Application.Services.Token.TokenBlacklistService
                >();
            }
            else
            {
                // 使用内存分布式缓存作为回退
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddScoped<
                    Application.Interfaces.Token.ITokenBlacklistService,
                    Application.Services.Token.InMemoryTokenBlacklistService
                >();
            }

            // ===== Cache Service (统一缓存服务) =====
            builder.Services.AddScoped<
                Application.Interfaces.Caching.ICacheService,
                Application.Services.Caching.DistributedCacheService
            >();

            // ===== Cache Warmup Service (缓存预热) =====
            builder.Services.AddHostedService<Application.Services.Caching.CacheWarmupService>();

            builder.Services.AddScoped<
                Application.Interfaces.Token.ITokenManagementService,
                Application.Services.Token.TokenManagementService
            >();

            // ===== User Authentication Service =====
            builder.Services.AddScoped<
                Application.Interfaces.Authentication.IUserAuthenticationService,
                Application.Services.Authentication.UserAuthenticationService
            >();

            // ===== Rate Limit Service =====
            builder.Services.AddScoped<
                Application.Interfaces.IRateLimitService,
                Application.Services.RateLimit.RateLimitService
            >();

            // ===== Current User Service =====
            builder.Services.AddScoped<
                Application.Interfaces.ICurrentUserService,
                Application.Services.CurrentUserService
            >();

            // ===== Request Logging Service =====
            builder.Services.AddScoped<
                Application.Interfaces.Logging.IRequestLoggingService,
                Application.Services.Logging.RequestLoggingService
            >();

            // ===== Backup Service =====
            builder.Services.AddScoped<
                Application.Services.Administration.IDatabaseExportService,
                Application.Services.Administration.DatabaseExportService
            >();
            builder.Services.AddScoped<
                Application.Interfaces.Administration.IBackupService,
                Application.Services.Administration.BackupService
            >();

            // ===== CSRF Protection =====
            builder.Services.AddCsrfProtection();

            // ===== Audit Log Helper =====
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<Helpers.AuditLogHelper>();

            // ===== Database Configuration =====
            builder.Services.AddDatabaseConfiguration(builder.Configuration);

            // ===== AutoMapper =====
            builder.Services.AddAutoMapperConfiguration();

            // ===== Controllers =====
            builder
                .Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serialization options
                    options.JsonSerializerOptions.PropertyNamingPolicy = System
                        .Text
                        .Json
                        .JsonNamingPolicy
                        .CamelCase; // use camelCase for JSON
                    options.JsonSerializerOptions.WriteIndented = true; // format output
                });

            // ===== Routing =====
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true; // URL lowercase
                options.LowercaseQueryStrings = false; // Query strings not lowercase
            });

            // CORS (支持 SignalR)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAll",
                    policy =>
                    {
                        policy
                            .SetIsOriginAllowed(_ => true) // 允许任何源（生产环境应限制）
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials(); // SignalR 需要凭据支持
                    }
                );
            });

            // ===== Swagger =====
            builder.Services.AddSwaggerConfiguration();

            // ===== API Versioning =====
            builder.Services.AddApiVersioningConfiguration();

            // ===== Permission Authorization =====
            builder.Services.AddPermissionAuthorization();

            // ===== Health Checks =====
            builder.Services.AddHealthChecks();

            // ===== SignalR (实时通信) =====
            builder.Services.AddSignalRConfiguration(builder.Configuration);

            // ===== OpenTelemetry =====
            builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);

            // ===== MediatR (进程内领域事件) =====
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(Application.EventHandlers.UserCreatedEventHandler).Assembly
                )
            );

            // ===== Messaging Services (DDD Event-Driven Architecture) =====
            // MediatR 用于进程内领域事件
            // Kafka 用于进程间集成事件
            builder.Services.AddMessagingServices(builder.Configuration);

            // 启动 Kafka 集成事件消费者（后台服务）
            builder.Services.AddKafkaIntegrationEventConsumers();

            // ===== Distributed Lock (分布式锁) =====
            // 使用 Infrastructure 层的 Redis 实现
            builder.Services.AddSingleton<
                Application.Interfaces.Distributed.IDistributedLock,
                Infra.Messaging.Distributed.RedisDistributedLock
            >();

            // ===== Database Seeder =====
            builder.Services.AddScoped<Data.DatabaseSeeder>();

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

            // ===== Security Middleware =====
            app.UseSecurityHeaders();
            app.UseCsrfToken();

            // ===== Middleware Configuration =====
            // 所有中间件已测试
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<PerformanceMonitoringMiddleware>();
            app.UseMiddleware<ResponseCacheMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");

            // 启用认证和授权
            app.UseAuthentication();

            // ===== Tenant Resolution Middleware =====
            app.UseMiddleware<TenantMiddleware>();

            app.UseAuthorization();

            // ===== Map Endpoints =====
            app.MapControllers();
            app.MapHealthChecks("/health");

            // ===== SignalR Hubs =====
            app.MapSignalRHubs();

            // ===== OpenTelemetry Prometheus Endpoint =====
            app.UseOpenTelemetryConfiguration();

            // Error handling endpoint
            app.Map(
                "/error",
                (HttpContext httpContext) =>
                {
                    httpContext.Response.StatusCode = 500;
                    return Results.Problem("An unexpected error occurred.");
                }
            );

            // ===== Database Seeding =====
            // 在测试环境下跳过数据库种子
            if (!app.Environment.IsEnvironment("Testing"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<Data.DatabaseSeeder>();
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
            }

            // Run the app
            app.Run();
        }
    }
}
