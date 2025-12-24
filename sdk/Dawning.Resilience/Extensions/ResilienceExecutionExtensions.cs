using Polly;

namespace Dawning.Resilience.Extensions;

/// <summary>
/// 弹性策略执行扩展方法
/// </summary>
public static class ResilienceExecutionExtensions
{
    /// <summary>
    /// 使用弹性策略执行操作
    /// </summary>
    public static async Task<TResult> ExecuteAsync<TResult>(
        this ResiliencePipeline<TResult> pipeline,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return await pipeline.ExecuteAsync(
            async token => await operation(),
            cancellationToken);
    }

    /// <summary>
    /// 使用弹性策略执行操作
    /// </summary>
    public static async Task ExecuteAsync(
        this ResiliencePipeline pipeline,
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        await pipeline.ExecuteAsync(
            async token =>
            {
                await operation();
            },
            cancellationToken);
    }

    /// <summary>
    /// 使用弹性策略执行操作（带状态）
    /// </summary>
    public static async Task<TResult> ExecuteAsync<TState, TResult>(
        this ResiliencePipeline<TResult> pipeline,
        TState state,
        Func<TState, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return await pipeline.ExecuteAsync(
            async token => await operation(state),
            cancellationToken);
    }

    /// <summary>
    /// 使用弹性策略执行同步操作
    /// </summary>
    public static TResult Execute<TResult>(
        this ResiliencePipeline<TResult> pipeline,
        Func<TResult> operation)
    {
        return pipeline.Execute(_ => operation());
    }

    /// <summary>
    /// 使用弹性策略执行同步操作
    /// </summary>
    public static void Execute(
        this ResiliencePipeline pipeline,
        Action operation)
    {
        pipeline.Execute(_ => operation());
    }
}
