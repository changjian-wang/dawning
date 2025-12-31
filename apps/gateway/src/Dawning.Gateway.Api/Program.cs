using Dawning.Gateway.Api.Configuration;
using Dawning.Identity.Infra.CrossCutting.IoC;
using Dawning.Identity.Infra.Data.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===== Configure Serilog =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ===== Database Connection =====
var connectionString = builder.Configuration.GetConnectionString("MySQL");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddScoped(provider => new DbContext(connectionString));
    Log.Information("Database connection configured");
}

// ===== Register Application Services (Dependency Injection) =====
builder.Services.AddApplicationServices();

// ===== YARP =====
var useDatabase = builder.Configuration.GetValue<bool>("Gateway:UseDatabase");

if (useDatabase)
{
    // Load configuration from database
    builder.Services.AddReverseProxy().LoadFromDatabase();
    Log.Information("YARP configured to load routes from database");
}
else
{
    // Load from configuration file (static configuration, as fallback)
    builder
        .Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    Log.Information("YARP configured to load routes from appsettings.json");
}

// ===== Authentication =====
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

// ===== Authorization =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());

    options.AddPolicy("admin", policy => policy.RequireRole("admin"));
});

// ===== Rate Limiting Policies =====
builder.Services.AddRateLimiter(options =>
{
    // Fixed window rate limiting: 100 requests per minute
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

    // Sliding window rate limiting: 100 requests per minute, divided into 6 segments
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

    // Token bucket rate limiting: continuous token replenishment
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

    // Concurrency rate limiting: max 50 concurrent requests
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

    // Rate limit response
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

// ===== Health Checks =====
builder
    .Services.AddHealthChecks()
    .AddUrlGroup(
        new Uri(builder.Configuration["Identity:Authority"] + "/health"),
        name: "identity-service",
        timeout: TimeSpan.FromSeconds(5)
    );

var app = builder.Build();

// ===== Initialize Database Configuration =====
if (useDatabase)
{
    await app.InitializeDatabaseProxyConfigAsync();
}

// ===== Development Environment =====
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
