using Dawning.Resilience.Options;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Dawning.Resilience.Policies;

/// <summary>
/// 弹性策略构建器
/// </summary>
public class ResiliencePolicyBuilder
{
    private readonly ResilienceOptions _options;
    private readonly ILogger? _logger;

    public ResiliencePolicyBuilder(ResilienceOptions options, ILogger? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 构建组合弹性策略管道
    /// </summary>
    public ResiliencePipeline Build()
    {
        var builder = new ResiliencePipelineBuilder();

        if (_options.Timeout.Enabled)
        {
            builder.AddTimeout(BuildTimeoutStrategy());
        }

        if (_options.Retry.Enabled)
        {
            builder.AddRetry(BuildRetryStrategy());
        }

        if (_options.CircuitBreaker.Enabled)
        {
            builder.AddCircuitBreaker(BuildCircuitBreakerStrategy());
        }

        return builder.Build();
    }

    /// <summary>
    /// 构建泛型组合弹性策略管道
    /// </summary>
    public ResiliencePipeline<TResult> Build<TResult>()
    {
        var builder = new ResiliencePipelineBuilder<TResult>();

        if (_options.Timeout.Enabled)
        {
            builder.AddTimeout(BuildTimeoutStrategy());
        }

        if (_options.Retry.Enabled)
        {
            builder.AddRetry(BuildRetryStrategy<TResult>());
        }

        if (_options.CircuitBreaker.Enabled)
        {
            builder.AddCircuitBreaker(BuildCircuitBreakerStrategy<TResult>());
        }

        return builder.Build();
    }

    private RetryStrategyOptions BuildRetryStrategy()
    {
        return new RetryStrategyOptions
        {
            MaxRetryAttempts = _options.Retry.MaxRetryAttempts,
            BackoffType = _options.Retry.UseExponentialBackoff
                ? DelayBackoffType.Exponential
                : DelayBackoffType.Constant,
            Delay = TimeSpan.FromMilliseconds(_options.Retry.BaseDelayMs),
            MaxDelay = TimeSpan.FromMilliseconds(_options.Retry.MaxDelayMs),
            UseJitter = true,
            OnRetry = args =>
            {
                _logger?.LogWarning(
                    "重试第 {AttemptNumber} 次，延迟 {Delay}ms，异常: {Exception}",
                    args.AttemptNumber,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Exception?.Message ?? "无"
                );
                return default;
            },
        };
    }

    private RetryStrategyOptions<TResult> BuildRetryStrategy<TResult>()
    {
        return new RetryStrategyOptions<TResult>
        {
            MaxRetryAttempts = _options.Retry.MaxRetryAttempts,
            BackoffType = _options.Retry.UseExponentialBackoff
                ? DelayBackoffType.Exponential
                : DelayBackoffType.Constant,
            Delay = TimeSpan.FromMilliseconds(_options.Retry.BaseDelayMs),
            MaxDelay = TimeSpan.FromMilliseconds(_options.Retry.MaxDelayMs),
            UseJitter = true,
            OnRetry = args =>
            {
                _logger?.LogWarning(
                    "重试第 {AttemptNumber} 次，延迟 {Delay}ms，异常: {Exception}",
                    args.AttemptNumber,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Exception?.Message ?? "无"
                );
                return default;
            },
        };
    }

    private CircuitBreakerStrategyOptions BuildCircuitBreakerStrategy()
    {
        return new CircuitBreakerStrategyOptions
        {
            FailureRatio = _options.CircuitBreaker.FailureRatioThreshold,
            SamplingDuration = TimeSpan.FromSeconds(
                _options.CircuitBreaker.SamplingDurationSeconds
            ),
            MinimumThroughput = _options.CircuitBreaker.MinimumThroughput,
            BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreaker.BreakDurationSeconds),
            OnOpened = args =>
            {
                _logger?.LogError(
                    "熔断器已打开，持续 {BreakDuration} 秒，原因: {Exception}",
                    _options.CircuitBreaker.BreakDurationSeconds,
                    args.Outcome.Exception?.Message ?? "失败率过高"
                );
                return default;
            },
            OnClosed = _ =>
            {
                _logger?.LogInformation("熔断器已关闭，恢复正常服务");
                return default;
            },
            OnHalfOpened = _ =>
            {
                _logger?.LogInformation("熔断器半开状态，尝试恢复");
                return default;
            },
        };
    }

    private CircuitBreakerStrategyOptions<TResult> BuildCircuitBreakerStrategy<TResult>()
    {
        return new CircuitBreakerStrategyOptions<TResult>
        {
            FailureRatio = _options.CircuitBreaker.FailureRatioThreshold,
            SamplingDuration = TimeSpan.FromSeconds(
                _options.CircuitBreaker.SamplingDurationSeconds
            ),
            MinimumThroughput = _options.CircuitBreaker.MinimumThroughput,
            BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreaker.BreakDurationSeconds),
            OnOpened = args =>
            {
                _logger?.LogError(
                    "熔断器已打开，持续 {BreakDuration} 秒，原因: {Exception}",
                    _options.CircuitBreaker.BreakDurationSeconds,
                    args.Outcome.Exception?.Message ?? "失败率过高"
                );
                return default;
            },
            OnClosed = _ =>
            {
                _logger?.LogInformation("熔断器已关闭，恢复正常服务");
                return default;
            },
            OnHalfOpened = _ =>
            {
                _logger?.LogInformation("熔断器半开状态，尝试恢复");
                return default;
            },
        };
    }

    private TimeoutStrategyOptions BuildTimeoutStrategy()
    {
        return new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(_options.Timeout.TimeoutSeconds),
            OnTimeout = args =>
            {
                _logger?.LogWarning("操作超时，超时时间: {Timeout} 秒", args.Timeout.TotalSeconds);
                return default;
            },
        };
    }
}
