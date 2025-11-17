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

// ===== 添加 YARP 反向代理 =====
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ===== 配置认证 =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.RequireHttpsMetadata = false; // 开发环境可设为 false
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// ===== 配置授权策略 =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("admin", policy =>
        policy.RequireRole("admin"));
});

// ===== 配置限流 =====
builder.Services.AddRateLimiter(options =>
{
    // 固定窗口限流
    options.AddFixedWindowLimiter("FixedWindowPolicy", config =>
    {
        config.PermitLimit = 100; // 每个时间窗口允许的请求数
        config.Window = TimeSpan.FromMinutes(1); // 时间窗口长度
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 50; // 队列长度
    });

    // 滑动窗口限流
    options.AddSlidingWindowLimiter("SlidingWindowPolicy", config =>
    {
        config.PermitLimit = 100; // 每个时间窗口允许的请求数
        config.Window = TimeSpan.FromMinutes(1); // 时间窗口长度
        config.SegmentsPerWindow = 6; // 分段数
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 50; // 队列长度
    });
});

// ===== 配置 CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== 配置 Redis 缓存 =====
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
    options.InstanceName = "DawningGateway_";
});

// ===== 配置健康检查 =====
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri(builder.Configuration["IdentityServer:Authority"] + "/health"),
        name: "identity-service",
        timeout: TimeSpan.FromSeconds(5));

var app = builder.Build();

// ===== 中间件管道 =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// 健康检查端点
app.MapHealthChecks("/health");

// 反向代理端点
app.MapReverseProxy();

app.Run();
