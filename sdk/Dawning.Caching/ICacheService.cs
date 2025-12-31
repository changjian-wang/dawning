namespace Dawning.Caching;

/// <summary>
/// Cache service interface
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value, returns default if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set cached value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Cache value</param>
    /// <param name="expiration">Expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get or set cached value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Value factory method</param>
    /// <param name="expiration">Expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value</returns>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Remove cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove cache by pattern
    /// </summary>
    /// <param name="pattern">Key pattern (supports wildcard *)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if cache exists
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Whether exists</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh cache expiration time
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="expiration">New expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RefreshAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );
}
