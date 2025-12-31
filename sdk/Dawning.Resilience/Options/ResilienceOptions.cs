namespace Dawning.Resilience.Options;

/// <summary>
/// Resilience policy configuration options
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Retry configuration
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Circuit breaker configuration
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Timeout configuration
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new();
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Whether to enable retry
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay time (milliseconds)
    /// </summary>
    public int BaseDelayMs { get; set; } = 200;

    /// <summary>
    /// Whether to use exponential backoff
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Maximum delay time (milliseconds)
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;
}

/// <summary>
/// Circuit breaker policy configuration
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Whether to enable circuit breaker
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Sampling duration (seconds)
    /// </summary>
    public int SamplingDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Failure rate threshold to trigger circuit breaker (0.0 - 1.0)
    /// </summary>
    public double FailureRatioThreshold { get; set; } = 0.5;

    /// <summary>
    /// Minimum throughput during sampling period
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Circuit breaker duration (seconds)
    /// </summary>
    public int BreakDurationSeconds { get; set; } = 30;
}

/// <summary>
/// Timeout policy configuration
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// Whether to enable timeout
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Timeout duration (seconds)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
