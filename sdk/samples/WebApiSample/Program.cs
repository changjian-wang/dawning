using Dawning.Core.Extensions;
using Dawning.Identity.Extensions;
using Dawning.Logging.Extensions;
using Dawning.Resilience.Extensions;
using WebApiSample.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. 配置 Dawning 日志 (Serilog)
// ========================================
builder.Host.UseDawningLogging(options =>
{
    options.ApplicationName = "WebApiSample";
    options.EnableConsole = true;
    options.EnableFile = true;
    options.LogFilePath = "logs/app-.log";
});

// ========================================
// 2. 配置 Dawning 认证 (JWT)
// ========================================
builder.Services.AddDawningAuthentication(options =>
{
    options.Authority = builder.Configuration["Auth:Authority"] ?? "https://localhost:5001";
    options.Audience = builder.Configuration["Auth:Audience"] ?? "api";
    options.RequireHttpsMetadata = false; // 开发环境
});

// ========================================
// 3. 配置 Dawning 弹性策略 (Polly)
// ========================================
builder.Services.AddDawningResilience(options =>
{
    // 重试配置
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BaseDelayMs = 200;
    options.Retry.UseExponentialBackoff = true;
    
    // 熔断器配置
    options.CircuitBreaker.Enabled = true;
    options.CircuitBreaker.FailureRatioThreshold = 0.5;
    options.CircuitBreaker.SamplingDurationSeconds = 30;
    options.CircuitBreaker.BreakDurationSeconds = 30;
    
    // 超时配置
    options.Timeout.TimeoutSeconds = 30;
});

// ========================================
// 4. 添加弹性 HttpClient
// ========================================
builder.Services.AddResilientHttpClient<ExternalApiClient>(
    client =>
    {
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
    },
    options =>
    {
        options.Retry.MaxRetryAttempts = 5;
    });
builder.Services.AddScoped<IExternalApiClient>(sp => sp.GetRequiredService<ExternalApiClient>());

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ========================================
// 5. 使用 Dawning 异常处理中间件
// ========================================
app.UseDawningExceptionHandling();

// ========================================
// 6. 使用 Dawning 日志富化中间件
// ========================================
app.UseDawningLoggingEnrichment();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
