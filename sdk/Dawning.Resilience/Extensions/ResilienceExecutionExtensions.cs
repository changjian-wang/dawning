using Polly;

namespace Dawning.Resilience.Extensions;

/// <summary>
/// Resilience policy execution extension methods
/// </summary>
public static class ResilienceExecutionExtensions
{
    /// <summary>
    /// Executes an operation with resilience policy
    /// </summary>
    public static async Task<TResult> ExecuteAsync<TResult>(
        this ResiliencePipeline<TResult> pipeline,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default
    )
    {
        return await pipeline.ExecuteAsync(async token => await operation(), cancellationToken);
    }

    /// <summary>
    /// Executes an operation with resilience policy
    /// </summary>
    public static async Task ExecuteAsync(
        this ResiliencePipeline pipeline,
        Func<Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        await pipeline.ExecuteAsync(
            async token =>
            {
                await operation();
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Executes an operation with resilience policy (with state)
    /// </summary>
    public static async Task<TResult> ExecuteAsync<TState, TResult>(
        this ResiliencePipeline<TResult> pipeline,
        TState state,
        Func<TState, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    )
    {
        return await pipeline.ExecuteAsync(
            async token => await operation(state),
            cancellationToken
        );
    }

    /// <summary>
    /// Executes a synchronous operation with resilience policy
    /// </summary>
    public static TResult Execute<TResult>(
        this ResiliencePipeline<TResult> pipeline,
        Func<TResult> operation
    )
    {
        return pipeline.Execute(_ => operation());
    }

    /// <summary>
    /// Executes a synchronous operation with resilience policy
    /// </summary>
    public static void Execute(this ResiliencePipeline pipeline, Action operation)
    {
        pipeline.Execute(_ => operation());
    }
}
