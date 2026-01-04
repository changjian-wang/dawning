using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Caching
{
    /// <summary>
    /// Unified cache service interface
    /// Provides Cache-Aside pattern, cache penetration protection, etc.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get or set cache value (Cache-Aside pattern)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Data factory method (called on cache miss)</param>
        /// <param name="options">Cache options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cached value or newly retrieved value</returns>
        Task<T?> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get cache value
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set cache value
        /// </summary>
        Task SetAsync<T>(
            string key,
            T value,
            CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Remove cache entry
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove cache entries by prefix (supports wildcards)
        /// </summary>
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if cache entry exists
        /// </summary>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh cache expiration time
        /// </summary>
        Task RefreshAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get or set cache (with cache penetration protection)
        /// When data source returns null, cache null value to prevent cache penetration
        /// </summary>
        Task<T?> GetOrSetWithNullProtectionAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            CacheEntryOptions? options = null,
            TimeSpan? nullValueTtl = null,
            CancellationToken cancellationToken = default
        )
            where T : class;
    }

    /// <summary>
    /// Cache entry options
    /// </summary>
    public class CacheEntryOptions
    {
        /// <summary>
        /// Absolute expiration time
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Absolute expiration relative to now
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Sliding expiration time (resets on each access)
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Default 5 minute expiration
        /// </summary>
        public static CacheEntryOptions Default =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        /// <summary>
        /// Short-term cache (1 minute)
        /// </summary>
        public static CacheEntryOptions Short =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };

        /// <summary>
        /// Medium-term cache (15 minutes)
        /// </summary>
        public static CacheEntryOptions Medium =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

        /// <summary>
        /// Long-term cache (1 hour)
        /// </summary>
        public static CacheEntryOptions Long =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

        /// <summary>
        /// Create custom expiration time options
        /// </summary>
        public static CacheEntryOptions FromMinutes(int minutes) =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes) };

        /// <summary>
        /// Create sliding expiration options
        /// </summary>
        public static CacheEntryOptions Sliding(TimeSpan slidingExpiration) =>
            new() { SlidingExpiration = slidingExpiration };
    }

    /// <summary>
    /// Cache key generator
    /// </summary>
    public static class CacheKeys
    {
        private const string Prefix = "dawning:";

        // System Configuration
        public static string SystemConfig(string group, string key) =>
            $"{Prefix}config:{group}:{key}";

        public static string SystemConfigGroup(string group) => $"{Prefix}config:{group}:*";

        public static string SystemConfigAll => $"{Prefix}config:*";

        // User
        public static string User(Guid id) => $"{Prefix}user:{id}";

        public static string UserByUsername(string username) => $"{Prefix}user:name:{username}";

        public static string UserPermissions(Guid userId) => $"{Prefix}user:{userId}:permissions";

        public static string UserRoles(Guid userId) => $"{Prefix}user:{userId}:roles";

        // Role
        public static string Role(Guid id) => $"{Prefix}role:{id}";

        public static string RoleByName(string name) => $"{Prefix}role:name:{name}";

        public static string AllRoles => $"{Prefix}roles:all";

        // Permission
        public static string Permission(Guid id) => $"{Prefix}permission:{id}";

        public static string AllPermissions => $"{Prefix}permissions:all";

        // Gateway Configuration
        public static string GatewayCluster(Guid id) => $"{Prefix}gateway:cluster:{id}";

        public static string GatewayRoute(Guid id) => $"{Prefix}gateway:route:{id}";

        public static string AllGatewayClusters => $"{Prefix}gateway:clusters:all";
        public static string AllGatewayRoutes => $"{Prefix}gateway:routes:all";

        // Alert
        public static string AlertRules => $"{Prefix}alert:rules";
        public static string AlertStatistics => $"{Prefix}alert:statistics";

        // IP Access Rules
        public static string IpRules => $"{Prefix}ip:rules";

        // Null value marker (for cache penetration protection)
        public const string NullMarker = "__NULL__";
    }
}
