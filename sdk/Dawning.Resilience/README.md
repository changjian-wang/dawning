# Dawning.Resilience

弹性策略库（基于 Polly 8）。

## 安装

```bash
dotnet add package Dawning.Resilience
```

## 功能

- **重试策略** - 指数退避、抖动
- **熔断器** - 失败率触发、自动恢复
- **超时处理** - 请求超时控制
- **HttpClient 集成** - 弹性 HTTP 客户端

## 使用

### 方式 1: 注册弹性服务

```csharp
builder.Services.AddDawningResilience(options =>
{
    // 重试配置
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BaseDelayMs = 200;
    options.Retry.UseExponentialBackoff = true;
    
    // 熔断器配置
    options.CircuitBreaker.FailureRatioThreshold = 0.5;
    options.CircuitBreaker.BreakDurationSeconds = 30;
    
    // 超时配置
    options.Timeout.TimeoutSeconds = 30;
});
```

### 方式 2: 弹性 HttpClient

```csharp
builder.Services.AddResilientHttpClient<IMyApiClient>(
    client => client.BaseAddress = new Uri("https://api.example.com"),
    options => options.Retry.MaxRetryAttempts = 5
);
```

### 方式 3: 手动使用

```csharp
public class MyService
{
    private readonly ResiliencePolicyBuilder _policyBuilder;

    public async Task<string> CallApiAsync()
    {
        var pipeline = _policyBuilder.Build<string>();
        
        return await pipeline.ExecuteAsync(async () =>
        {
            return await _httpClient.GetStringAsync("/api/data");
        });
    }
}
```

## 配置选项

### 重试 (RetryOptions)

| 选项 | 说明 | 默认值 |
|------|------|--------|
| `Enabled` | 是否启用 | true |
| `MaxRetryAttempts` | 最大重试次数 | 3 |
| `BaseDelayMs` | 基础延迟(ms) | 200 |
| `UseExponentialBackoff` | 指数退避 | true |
| `MaxDelayMs` | 最大延迟(ms) | 30000 |

### 熔断器 (CircuitBreakerOptions)

| 选项 | 说明 | 默认值 |
|------|------|--------|
| `Enabled` | 是否启用 | true |
| `FailureRatioThreshold` | 失败率阈值 | 0.5 |
| `SamplingDurationSeconds` | 采样时长(s) | 30 |
| `MinimumThroughput` | 最小吞吐量 | 10 |
| `BreakDurationSeconds` | 熔断时长(s) | 30 |

### 超时 (TimeoutOptions)

| 选项 | 说明 | 默认值 |
|------|------|--------|
| `Enabled` | 是否启用 | true |
| `TimeoutSeconds` | 超时时间(s) | 30 |
