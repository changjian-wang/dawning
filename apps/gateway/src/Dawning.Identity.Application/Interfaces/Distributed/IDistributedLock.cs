namespace Dawning.Identity.Application.Interfaces.Distributed;

/// <summary>
/// Distributed lock interface
/// </summary>
 public interface IDistributedLock
{
    /// <summary>
    /// Try to acquire a lock
    /// </summary>
    /// <param name="lockKey">Lock key</param>
    /// <param name="expiry">Lock expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lock handle (if acquired successfully), null (if acquisition failed)</returns>
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Acquire a lock (blocks until acquired or timeout)
    /// </summary>
    /// <param name="lockKey">Lock key</param>
    /// <param name="expiry">Lock expiration time</param>
    /// <param name="timeout">Wait timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lock handle</returns>
    Task<IDistributedLockHandle> AcquireAsync(
        string lockKey,
        TimeSpan expiry,
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Distributed lock handle
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// Lock key
    /// </summary>
    string LockKey { get; }

    /// <summary>
    /// Whether the lock is still valid
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// Extend lock expiration time
    /// </summary>
    Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release the lock
    /// </summary>
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}
