using Dawning.Core.Extensions;
using Dawning.Identity.Extensions;
using Dawning.Logging.Extensions;
using Dawning.Resilience.Extensions;
using WebApiSample.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. Configure Dawning Logging (Serilog)
// ========================================
builder.Host.UseDawningLogging(options =>
{
    options.ApplicationName = "WebApiSample";
    options.EnableConsole = true;
    options.EnableFile = true;
    options.LogFilePath = "logs/app-.log";
});

// ========================================
// 2. Configure Dawning Authentication (JWT)
// ========================================
builder.Services.AddDawningAuthentication(options =>
{
    options.Authority = builder.Configuration["Auth:Authority"] ?? "https://localhost:5001";
    options.Audience = builder.Configuration["Auth:Audience"] ?? "api";
    options.RequireHttpsMetadata = false; // Development environment
});

// ========================================
// 3. Configure Dawning Resilience (Polly)
// ========================================
builder.Services.AddDawningResilience(options =>
{
    // Retry configuration
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BaseDelayMs = 200;
    options.Retry.UseExponentialBackoff = true;

    // Circuit breaker configuration
    options.CircuitBreaker.Enabled = true;
    options.CircuitBreaker.FailureRatioThreshold = 0.5;
    options.CircuitBreaker.SamplingDurationSeconds = 30;
    options.CircuitBreaker.BreakDurationSeconds = 30;

    // Timeout configuration
    options.Timeout.TimeoutSeconds = 30;
});

// ========================================
// 4. Add Resilient HttpClient
// ========================================
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ========================================
// 5. Use Dawning Exception Handling Middleware
// ========================================
app.UseDawningExceptionHandling();

// ========================================
// 6. Use Dawning Logging Enrichment Middleware
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
