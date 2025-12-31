using Dawning.Resilience.Options;
using Dawning.Resilience.Policies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Dawning.Resilience.Extensions;

/// <summary>
/// Service collection extension methods
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds resilience policy services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
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
    /// Adds an HttpClient with resilience policies
    /// </summary>
    /// <typeparam name="TClient">HttpClient type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configureClient">Configure HttpClient</param>
    /// <param name="configureResilience">Configure resilience options</param>
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
                                    "HTTP request retry attempt {AttemptNumber}, status code: {StatusCode}, exception: {Exception}",
                                    args.AttemptNumber,
                                    args.Outcome.Result?.StatusCode,
                                    args.Outcome.Exception?.Message ?? "None"
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
                                    "HTTP client circuit breaker opened for {BreakDuration} seconds",
                                    options.CircuitBreaker.BreakDurationSeconds
                                );
                                return default;
                            },
                            OnClosed = _ =>
                            {
                                logger?.LogInformation("HTTP client circuit breaker closed");
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
    /// Adds a named HttpClient with resilience policies
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
