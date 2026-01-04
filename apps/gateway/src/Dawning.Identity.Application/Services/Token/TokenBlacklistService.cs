using System;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Token;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Token
{
    /// <summary>
    /// Token blacklist service implementation (Redis-based)
    /// </summary>
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<TokenBlacklistService> _logger;
        private const string TokenBlacklistPrefix = "token:blacklist:";
        private const string UserBlacklistPrefix = "user:blacklist:";

        public TokenBlacklistService(IDistributedCache cache, ILogger<TokenBlacklistService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Add token to blacklist
        /// </summary>
        public async Task AddToBlacklistAsync(string jti, DateTime expiration)
        {
            if (string.IsNullOrEmpty(jti))
                return;

            try
            {
                var key = $"{TokenBlacklistPrefix}{jti}";
                var ttl = expiration - DateTime.UtcNow;

                // Only add if not expired
                if (ttl > TimeSpan.Zero)
                {
                    await _cache.SetStringAsync(
                        key,
                        "1",
                        new DistributedCacheEntryOptions { AbsoluteExpiration = expiration }
                    );

                    _logger.LogDebug(
                        "Token {Jti} added to blacklist, expires at {Expiration}",
                        jti,
                        expiration
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add token {Jti} to blacklist", jti);
            }
        }

        /// <summary>
        /// Check if token is in blacklist
        /// </summary>
        public async Task<bool> IsBlacklistedAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti))
                return false;

            try
            {
                var key = $"{TokenBlacklistPrefix}{jti}";
                var value = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check blacklist for token {Jti}", jti);
                return false; // Default to not blocking on failure
            }
        }

        /// <summary>
        /// Blacklist all tokens for a user
        /// </summary>
        public async Task BlacklistUserTokensAsync(Guid userId, DateTime expiration)
        {
            try
            {
                var key = $"{UserBlacklistPrefix}{userId}";
                var ttl = expiration - DateTime.UtcNow;

                if (ttl > TimeSpan.Zero)
                {
                    // Store blacklist timestamp - all previous tokens become invalid
                    await _cache.SetStringAsync(
                        key,
                        DateTime.UtcNow.Ticks.ToString(),
                        new DistributedCacheEntryOptions { AbsoluteExpiration = expiration }
                    );

                    _logger.LogInformation(
                        "User {UserId} tokens blacklisted until {Expiration}",
                        userId,
                        expiration
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to blacklist user {UserId} tokens", userId);
            }
        }

        /// <summary>
        /// Check if user is in global blacklist
        /// </summary>
        public async Task<bool> IsUserBlacklistedAsync(Guid userId)
        {
            try
            {
                var key = $"{UserBlacklistPrefix}{userId}";
                var value = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check user blacklist for {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Clean up expired blacklist entries (Redis handles expiration automatically, this method can be used for manual cleanup)
        /// </summary>
        public Task CleanupExpiredEntriesAsync()
        {
            // Redis automatically handles expired entries, no manual cleanup needed
            _logger.LogDebug("Cleanup not needed - Redis handles expiration automatically");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// In-memory cache version of Token blacklist service (for scenarios without Redis)
    /// </summary>
    public class InMemoryTokenBlacklistService : ITokenBlacklistService
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private readonly ILogger<InMemoryTokenBlacklistService> _logger;
        private const string TokenBlacklistPrefix = "token:blacklist:";
        private const string UserBlacklistPrefix = "user:blacklist:";

        public InMemoryTokenBlacklistService(
            Microsoft.Extensions.Caching.Memory.IMemoryCache cache,
            ILogger<InMemoryTokenBlacklistService> logger
        )
        {
            _cache = cache;
            _logger = logger;
        }

        public Task AddToBlacklistAsync(string jti, DateTime expiration)
        {
            if (string.IsNullOrEmpty(jti))
                return Task.CompletedTask;

            var key = $"{TokenBlacklistPrefix}{jti}";
            var options = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiration,
            };
            _cache.Set(key, true, options);
            _logger.LogDebug("Token {Jti} added to in-memory blacklist", jti);
            return Task.CompletedTask;
        }

        public Task<bool> IsBlacklistedAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti))
                return Task.FromResult(false);

            var key = $"{TokenBlacklistPrefix}{jti}";
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }

        public Task BlacklistUserTokensAsync(Guid userId, DateTime expiration)
        {
            var key = $"{UserBlacklistPrefix}{userId}";
            var options = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiration,
            };
            _cache.Set(key, DateTime.UtcNow.Ticks, options);
            _logger.LogInformation("User {UserId} tokens blacklisted in memory", userId);
            return Task.CompletedTask;
        }

        public Task<bool> IsUserBlacklistedAsync(Guid userId)
        {
            var key = $"{UserBlacklistPrefix}{userId}";
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }

        public Task CleanupExpiredEntriesAsync()
        {
            // MemoryCache automatically handles expiration
            return Task.CompletedTask;
        }
    }
}
