using Dawning.Resilience.Options;
using Dawning.Resilience.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Dawning.Resilience.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加弹性策略服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns></returns>
    public static IServiceCollection AddDawningResilience(
        this IServiceCollection services,
        Action<ResilienceOptions>? configure = null
    )
    {
        var options = new ResilienceOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<ILogger<ResiliencePolicyBuilder>>();
            return new ResiliencePolicyBuilder(options, logger);
        });

        return services;
    }

    /// <summary>
    /// 添加具有弹性策略的 HttpClient
    /// </summary>
    /// <typeparam name="TClient">HttpClient 类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="configureClient">配置 HttpClient</param>
    /// <param name="configureResilience">配置弹性选项</param>
    /// <returns></returns>
    public static IHttpClientBuilder AddResilientHttpClient<TClient>(
        this IServiceCollection services,
        Action<HttpClient>? configureClient = null,
        Action<ResilienceOptions>? configureResilience = null
    )
        where TClient : class
    {
        var options = new ResilienceOptions();
        configureResilience?.Invoke(options);

        var builder = services.AddHttpClient<TClient>();

        if (configureClient != null)
        {
            builder.ConfigureHttpClient(configureClient);
        }

        builder.AddResilienceHandler(
            "default",
            (pipelineBuilder, context) =>
            {
                var logger = context.ServiceProvider.GetService<ILogger<TClient>>();

                if (options.Timeout.Enabled)
                {
                    pipelineBuilder.AddTimeout(
                        TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds)
                    );
                }

                if (options.Retry.Enabled)
                {
                    pipelineBuilder.AddRetry(
                        new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
                        {
                            MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                            BackoffType = options.Retry.UseExponentialBackoff
                                ? DelayBackoffType.Exponential
                                : DelayBackoffType.Constant,
                            Delay = TimeSpan.FromMilliseconds(options.Retry.BaseDelayMs),
                            UseJitter = true,
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>()
                                .Handle<TaskCanceledException>()
                                .HandleResult(r => (int)r.StatusCode >= 500),
                            OnRetry = args =>
                            {
                                logger?.LogWarning(
                                    "HTTP 请求重试第 {AttemptNumber} 次，状态码: {StatusCode}，异常: {Exception}",
                                    args.AttemptNumber,
                                    args.Outcome.Result?.StatusCode,
                                    args.Outcome.Exception?.Message ?? "无"
                                );
                                return default;
                            },
                        }
                    );
                }

                if (options.CircuitBreaker.Enabled)
                {
                    pipelineBuilder.AddCircuitBreaker(
                        new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
                        {
                            FailureRatio = options.CircuitBreaker.FailureRatioThreshold,
                            SamplingDuration = TimeSpan.FromSeconds(
                                options.CircuitBreaker.SamplingDurationSeconds
                            ),
                            MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                            BreakDuration = TimeSpan.FromSeconds(
                                options.CircuitBreaker.BreakDurationSeconds
                            ),
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>()
                                .Handle<TaskCanceledException>()
                                .HandleResult(r => (int)r.StatusCode >= 500),
                            OnOpened = args =>
                            {
                                logger?.LogError(
                                    "HTTP 客户端熔断器已打开，持续 {BreakDuration} 秒",
                                    options.CircuitBreaker.BreakDurationSeconds
                                );
                                return default;
                            },
                            OnClosed = _ =>
                            {
                                logger?.LogInformation("HTTP 客户端熔断器已关闭");
                                return default;
                            },
                        }
                    );
                }
            }
        );

        return builder;
    }

    /// <summary>
    /// 添加具有弹性策略的命名 HttpClient
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null,
        Action<ResilienceOptions>? configureResilience = null
    )
    {
        var options = new ResilienceOptions();
        configureResilience?.Invoke(options);

        var builder = services.AddHttpClient(name);

        if (configureClient != null)
        {
            builder.ConfigureHttpClient(configureClient);
        }

        builder.AddResilienceHandler(
            "default",
            (pipelineBuilder, context) =>
            {
                var logger = context
                    .ServiceProvider.GetService<ILoggerFactory>()
                    ?.CreateLogger($"HttpClient.{name}");

                if (options.Timeout.Enabled)
                {
                    pipelineBuilder.AddTimeout(
                        TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds)
                    );
                }

                if (options.Retry.Enabled)
                {
                    pipelineBuilder.AddRetry(
                        new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
                        {
                            MaxRetryAttempts = options.Retry.MaxRetryAttempts,
                            BackoffType = options.Retry.UseExponentialBackoff
                                ? DelayBackoffType.Exponential
                                : DelayBackoffType.Constant,
                            Delay = TimeSpan.FromMilliseconds(options.Retry.BaseDelayMs),
                            UseJitter = true,
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>()
                                .Handle<TaskCanceledException>()
                                .HandleResult(r => (int)r.StatusCode >= 500),
                        }
                    );
                }

                if (options.CircuitBreaker.Enabled)
                {
                    pipelineBuilder.AddCircuitBreaker(
                        new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
                        {
                            FailureRatio = options.CircuitBreaker.FailureRatioThreshold,
                            SamplingDuration = TimeSpan.FromSeconds(
                                options.CircuitBreaker.SamplingDurationSeconds
                            ),
                            MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                            BreakDuration = TimeSpan.FromSeconds(
                                options.CircuitBreaker.BreakDurationSeconds
                            ),
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>()
                                .Handle<TaskCanceledException>()
                                .HandleResult(r => (int)r.StatusCode >= 500),
                        }
                    );
                }
            }
        );

        return builder;
    }
}
