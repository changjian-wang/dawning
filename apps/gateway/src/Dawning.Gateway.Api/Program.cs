using Dawning.Gateway.Api.Configuration;
using Dawning.Identity.Infra.CrossCutting.IoC;
using Dawning.Identity.Infra.Data.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== 配置 Serilog =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ===== 数据库连接 =====
var connectionString = builder.Configuration.GetConnectionString("MySQL");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddScoped(provider => new DbContext(connectionString));
    Log.Information("Database connection configured");
}

// ===== 注册应用服务 (依赖注入) =====
builder.Services.AddApplicationServices();

// ===== YARP =====
var useDatabase = builder.Configuration.GetValue<bool>("Gateway:UseDatabase");

if (useDatabase)
{
    // 从数据库加载配置
    builder.Services.AddReverseProxy().LoadFromDatabase();
    Log.Information("YARP configured to load routes from database");
}
else
{
    // 从配置文件加载（静态配置，作为后备）
    builder
        .Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    Log.Information("YARP configured to load routes from appsettings.json");
}

// ===== 认证 =====
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Identity:Authority"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Identity:Issuer"],
            ClockSkew = TimeSpan.FromMinutes(5),
        };
    });

// ===== 授权 =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());

    options.AddPolicy("admin", policy => policy.RequireRole("admin"));
});

// ===== 限流策略 =====
builder.Services.AddRateLimiter(options =>
{
    // 固定窗口限流：每分钟 100 个请求
    options.AddFixedWindowLimiter(
        "fixed-window",
        config =>
        {
            config.PermitLimit = 100;
            config.Window = TimeSpan.FromMinutes(1);
            config.QueueProcessingOrder = System
                .Threading
                .RateLimiting
                .QueueProcessingOrder
                .OldestFirst;
            config.QueueLimit = 50;
        }
    );

    // 滑动窗口限流：每分钟 100 个请求，分 6 段
    options.AddSlidingWindowLimiter(
        "sliding-window",
        config =>
        {
            config.PermitLimit = 100;
            config.Window = TimeSpan.FromMinutes(1);
            config.SegmentsPerWindow = 6;
            config.QueueProcessingOrder = System
                .Threading
                .RateLimiting
                .QueueProcessingOrder
                .OldestFirst;
            config.QueueLimit = 50;
        }
    );

    // 令牌桶限流：持续补充令牌
    options.AddTokenBucketLimiter(
        "token-bucket",
        config =>
        {
            config.TokenLimit = 100;
            config.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            config.TokensPerPeriod = 20;
            config.QueueProcessingOrder = System
                .Threading
                .RateLimiting
                .QueueProcessingOrder
                .OldestFirst;
            config.QueueLimit = 50;
        }
    );

    // 并发限流：最多 50 个并发请求
    options.AddConcurrencyLimiter(
        "concurrency",
        config =>
        {
            config.PermitLimit = 50;
            config.QueueProcessingOrder = System
                .Threading
                .RateLimiting
                .QueueProcessingOrder
                .OldestFirst;
            config.QueueLimit = 25;
        }
    );

    // 限流响应
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Too many requests. Please try again later.\"}",
            cancellationToken
        );
    };
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// ===== Redis =====
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
    options.InstanceName = "DawningGateway_";
});

// ===== 健康检查 =====
builder
    .Services.AddHealthChecks()
    .AddUrlGroup(
        new Uri(builder.Configuration["Identity:Authority"] + "/health"),
        name: "identity-service",
        timeout: TimeSpan.FromSeconds(5)
    );

var app = builder.Build();

// ===== 初始化数据库配置 =====
if (useDatabase)
{
    await app.InitializeDatabaseProxyConfigAsync();
}

// ===== 开发环境 =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Health
app.MapHealthChecks("/health");

// Gateway config reload endpoint
if (useDatabase)
{
    app.MapProxyConfigReloadEndpoint("/gateway/reload");
}

// YARP
app.MapReverseProxy();

app.Run();
