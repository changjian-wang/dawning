using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dawning.Identity.Application.Attributes;
using Dawning.Identity.Application.Interfaces.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Middleware
{
    /// <summary>
    /// API response caching middleware
    /// Supports cache behavior control via CacheResponseAttribute
    /// </summary>
    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseCacheMiddleware> _logger;
        private const string CacheKeyPrefix = "response:";

        public ResponseCacheMiddleware(
            RequestDelegate next,
            ILogger<ResponseCacheMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICacheService? cacheService = null)
        {
            // Only cache GET requests
            if (context.Request.Method != HttpMethods.Get || cacheService == null)
            {
                await _next(context);
                return;
            }

            // Get endpoint's cache attribute
            var endpoint = context.GetEndpoint();
            var cacheAttribute = endpoint?.Metadata.GetMetadata<CacheResponseAttribute>();
            var noCacheAttribute = endpoint?.Metadata.GetMetadata<NoCacheAttribute>();

            // Check if cache is disabled
            if (noCacheAttribute != null || cacheAttribute == null)
            {
                await _next(context);
                return;
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(context, cacheAttribute);

            // Try to get from cache
            var cachedResponse = await cacheService.GetAsync<CachedResponse>(cacheKey);
            if (cachedResponse != null)
            {
                _logger.LogDebug("Cache hit for: {Path}", context.Request.Path);

                // Add cache header
                context.Response.Headers.Append("X-Cache", "HIT");
                context.Response.ContentType = cachedResponse.ContentType;
                context.Response.StatusCode = cachedResponse.StatusCode;

                await context.Response.WriteAsync(cachedResponse.Body);
                return;
            }

            _logger.LogDebug("Cache miss for: {Path}", context.Request.Path);

            // Cache miss, execute request and cache result
            var originalBodyStream = context.Response.Body;

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await _next(context);

                // Only cache successful responses
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                    var responseToCache = new CachedResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType ?? "application/json",
                        Body = responseBody,
                        CachedAt = DateTimeOffset.UtcNow,
                    };

                    await cacheService.SetAsync(
                        cacheKey,
                        responseToCache,
                        CacheEntryOptions.FromMinutes(cacheAttribute.DurationSeconds / 60)
                    );

                    // Add cache headers
                    if (!context.Response.HasStarted)
                    {
                        context.Response.Headers.Append("X-Cache", "MISS");
                        context.Response.Headers.Append(
                            "Cache-Control",
                            $"max-age={cacheAttribute.DurationSeconds}"
                        );
                    }
                }

                // Write response back to original stream
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private string GenerateCacheKey(HttpContext context, CacheResponseAttribute cacheAttribute)
        {
            var keyBuilder = new StringBuilder(CacheKeyPrefix);

            // Add prefix
            if (!string.IsNullOrEmpty(cacheAttribute.CacheKeyPrefix))
            {
                keyBuilder.Append(cacheAttribute.CacheKeyPrefix);
                keyBuilder.Append(':');
            }

            // Add path
            keyBuilder.Append(context.Request.Path.Value?.ToLowerInvariant());

            // Vary by user
            if (cacheAttribute.VaryByUser && context.User.Identity?.IsAuthenticated == true)
            {
                var userId =
                    context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("id")?.Value
                    ?? "anonymous";
                keyBuilder.Append(":user:");
                keyBuilder.Append(userId);
            }

            // Vary by query parameters
            if (!string.IsNullOrEmpty(cacheAttribute.VaryByQueryKeys))
            {
                var queryKeys = cacheAttribute.VaryByQueryKeys.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var key in queryKeys)
                {
                    var trimmedKey = key.Trim();
                    if (context.Request.Query.TryGetValue(trimmedKey, out var value))
                    {
                        keyBuilder.Append(':');
                        keyBuilder.Append(trimmedKey);
                        keyBuilder.Append('=');
                        keyBuilder.Append(value.ToString());
                    }
                }
            }

            // Hash the full query string if no specific query keys are specified
            if (
                string.IsNullOrEmpty(cacheAttribute.VaryByQueryKeys)
                && context.Request.QueryString.HasValue
            )
            {
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(context.Request.QueryString.Value ?? "")
                );
                keyBuilder.Append(":q:");
                keyBuilder.Append(Convert.ToBase64String(hash)[..8]);
            }

            return keyBuilder.ToString();
        }

        /// <summary>
        /// Cached response object
        /// </summary>
        private class CachedResponse
        {
            public int StatusCode { get; set; }
            public string ContentType { get; set; } = "application/json";
            public string Body { get; set; } = string.Empty;
            public DateTimeOffset CachedAt { get; set; }
        }
    }
}
