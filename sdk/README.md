# Dawning SDK

A collection of .NET libraries for building enterprise applications.

## Libraries

| Package | Description |
|---------|-------------|
| `Dawning.Core` | Core utilities, result types, and middleware |
| `Dawning.Identity` | Authentication and authorization utilities |
| `Dawning.Logging` | Structured logging with correlation |
| `Dawning.Caching` | Redis and in-memory caching |
| `Dawning.Messaging` | Message queue abstractions (RabbitMQ, Azure Service Bus) |
| `Dawning.Resilience` | Retry policies and circuit breakers |
| `Dawning.Extensions` | Common extension methods |
| `Dawning.ORM.Dapper` | Dapper extensions for CRUD operations |

## Installation

```bash
# Install from NuGet
dotnet add package Dawning.Core
dotnet add package Dawning.Identity
dotnet add package Dawning.Logging
dotnet add package Dawning.Caching
```

## Quick Start

### Dawning.Core

```csharp
using Dawning.Core.Results;
using Dawning.Core.Middleware;

// Use Result type for operation outcomes
public Result<User> GetUser(string id)
{
    var user = _repository.Find(id);
    if (user == null)
        return Result<User>.Failure("User not found");
    
    return Result<User>.Success(user);
}

// Add exception handling middleware
app.UseExceptionHandling();
```

### Dawning.Identity

```csharp
using Dawning.Identity;
using Dawning.Identity.Extensions;

// Configure identity services
services.AddDawningIdentity(options =>
{
    options.JwtSecret = "your-secret";
    options.TokenExpiration = TimeSpan.FromHours(1);
});

// Get current user in controllers
var currentUser = HttpContext.GetCurrentUser();
```

### Dawning.Logging

```csharp
using Dawning.Logging.Extensions;

// Configure structured logging
services.AddDawningLogging(options =>
{
    options.ApplicationName = "MyService";
    options.EnableConsole = true;
    options.EnableFile = true;
});

// Use correlation middleware
app.UseCorrelationId();
app.UseRequestLogging();
```

### Dawning.Caching

```csharp
using Dawning.Caching;

// Configure Redis caching
services.AddDawningCaching(options =>
{
    options.ConnectionString = "localhost:6379";
    options.DefaultExpiration = TimeSpan.FromMinutes(5);
});

// Use cache service
public class MyService
{
    private readonly ICacheService _cache;
    
    public async Task<User?> GetUserAsync(string id)
    {
        return await _cache.GetOrSetAsync(
            $"user:{id}",
            async () => await _repository.GetByIdAsync(id),
            TimeSpan.FromMinutes(10));
    }
}
```

### Dawning.Messaging

```csharp
using Dawning.Messaging;

// Configure RabbitMQ
services.AddDawningMessaging(options =>
{
    options.UseRabbitMQ(cfg =>
    {
        cfg.HostName = "localhost";
        cfg.UserName = "guest";
        cfg.Password = "guest";
    });
});

// Publish messages
await _messageBus.PublishAsync(new OrderCreatedEvent { OrderId = orderId });

// Subscribe to messages
services.AddMessageHandler<OrderCreatedEvent, OrderCreatedHandler>();
```

### Dawning.Resilience

```csharp
using Dawning.Resilience;

// Configure resilience policies
services.AddDawningResilience(options =>
{
    options.RetryCount = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
    options.CircuitBreakerThreshold = 5;
});

// Apply to HTTP clients
services.AddHttpClient<IMyApiClient, MyApiClient>()
    .AddDawningResilience();
```

### Dawning.Extensions

```csharp
using Dawning.Extensions;

// String extensions
string? value = null;
bool isEmpty = value.IsNullOrEmpty();  // true

// Collection extensions
var items = new List<string> { "a", "b", "c" };
bool hasItems = items.IsNotEmpty();  // true

// JSON extensions
var obj = json.FromJson<MyClass>();
var jsonString = obj.ToJson();

// DateTime extensions
var timestamp = DateTime.UtcNow.ToUnixTimestamp();
var dateTime = timestamp.FromUnixTimestamp();
```

### Dawning.ORM.Dapper

```csharp
using Dawning.ORM.Dapper;

// CRUD operations
var user = await connection.GetAsync<User>(id);
var users = await connection.GetAllAsync<User>();

await connection.InsertAsync(newUser);
await connection.UpdateAsync(existingUser);
await connection.DeleteAsync(user);

// Bulk operations
await connection.InsertAllAsync(users);
```

## Configuration

All libraries support configuration via `IOptions<T>`:

```json
{
  "DawningLogging": {
    "ApplicationName": "MyApp",
    "EnableConsole": true,
    "LogLevel": "Information"
  },
  "DawningCaching": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "myapp"
  }
}
```

## Samples

See the `samples/` directory for complete examples:

- `BasicWebApi/` - Simple API with all SDK components
- `MicroserviceTemplate/` - Microservice with messaging
- `AuthenticationSample/` - Identity integration

## Benchmarks

Performance benchmarks are in `benchmarks/Dawning.Benchmarks/`:

```bash
cd benchmarks/Dawning.Benchmarks
dotnet run -c Release
```

## Testing

```bash
cd tests
dotnet test
```

## License

MIT License - see [LICENSE](../LICENSE) for details.

---

For gateway documentation, see [Developer Guide](../docs/DEVELOPER_GUIDE.md).
