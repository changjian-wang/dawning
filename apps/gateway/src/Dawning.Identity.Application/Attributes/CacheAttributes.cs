using System;

namespace Dawning.Identity.Application.Attributes
{
    /// <summary>
    /// Mark API endpoint to use response caching
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CacheResponseAttribute : Attribute
    {
        /// <summary>
        /// Cache duration (seconds)
        /// </summary>
        public int DurationSeconds { get; }

        /// <summary>
        /// Whether to vary cache by user
        /// </summary>
        public bool VaryByUser { get; set; }

        /// <summary>
        /// Vary cache by query parameters (comma separated)
        /// </summary>
        public string? VaryByQueryKeys { get; set; }

        /// <summary>
        /// Cache key prefix
        /// </summary>
        public string? CacheKeyPrefix { get; set; }

        /// <summary>
        /// Create cache response attribute
        /// </summary>
        /// <param name="durationSeconds">Cache duration (seconds), default 60 seconds</param>
        public CacheResponseAttribute(int durationSeconds = 60)
        {
            DurationSeconds = durationSeconds;
        }
    }

    /// <summary>
    /// Mark API endpoint to not use caching
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class NoCacheAttribute : Attribute { }

    /// <summary>
    /// Mark operation to invalidate specified cache
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InvalidateCacheAttribute : Attribute
    {
        /// <summary>
        /// Cache key prefixes to invalidate
        /// </summary>
        public string[] CacheKeyPrefixes { get; }

        /// <summary>
        /// Create cache invalidation attribute
        /// </summary>
        /// <param name="cacheKeyPrefixes">Cache key prefixes to invalidate</param>
        public InvalidateCacheAttribute(params string[] cacheKeyPrefixes)
        {
            CacheKeyPrefixes = cacheKeyPrefixes;
        }
    }
}
