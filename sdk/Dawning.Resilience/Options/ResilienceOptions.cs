namespace Dawning.Resilience.Options;

/// <summary>
/// 弹性策略配置选项
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// 重试配置
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// 熔断器配置
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// 超时配置
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new();
}

/// <summary>
/// 重试策略配置
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// 是否启用重试
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 基础延迟时间（毫秒）
    /// </summary>
    public int BaseDelayMs { get; set; } = 200;

    /// <summary>
    /// 是否使用指数退避
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// 最大延迟时间（毫秒）
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;
}

/// <summary>
/// 熔断器策略配置
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// 是否启用熔断器
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 采样时长（秒）
    /// </summary>
    public int SamplingDurationSeconds { get; set; } = 30;

    /// <summary>
    /// 触发熔断的失败率阈值 (0.0 - 1.0)
    /// </summary>
    public double FailureRatioThreshold { get; set; } = 0.5;

    /// <summary>
    /// 采样期间最小吞吐量
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// 熔断持续时间（秒）
    /// </summary>
    public int BreakDurationSeconds { get; set; } = 30;
}

/// <summary>
/// 超时策略配置
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// 是否启用超时
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
