using Dawning.Gateway.Api.Configuration;
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

// ===== 限流 =====
builder.Services.AddRateLimiter(options =>
{
    //
    options.AddFixedWindowLimiter(
        "FixedWindowPolicy",
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

    //
    options.AddSlidingWindowLimiter(
        "SlidingWindowPolicy",
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
