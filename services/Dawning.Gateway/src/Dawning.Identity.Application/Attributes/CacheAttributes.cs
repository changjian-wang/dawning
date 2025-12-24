using System;

namespace Dawning.Identity.Application.Attributes
{
    /// <summary>
    /// 标记 API 端点使用响应缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CacheResponseAttribute : Attribute
    {
        /// <summary>
        /// 缓存持续时间（秒）
        /// </summary>
        public int DurationSeconds { get; }

        /// <summary>
        /// 是否按用户区分缓存
        /// </summary>
        public bool VaryByUser { get; set; }

        /// <summary>
        /// 按查询参数区分缓存（逗号分隔）
        /// </summary>
        public string? VaryByQueryKeys { get; set; }

        /// <summary>
        /// 缓存键前缀
        /// </summary>
        public string? CacheKeyPrefix { get; set; }

        /// <summary>
        /// 创建缓存响应特性
        /// </summary>
        /// <param name="durationSeconds">缓存持续时间（秒），默认60秒</param>
        public CacheResponseAttribute(int durationSeconds = 60)
        {
            DurationSeconds = durationSeconds;
        }
    }

    /// <summary>
    /// 标记 API 端点不使用缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class NoCacheAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记操作会使指定缓存失效
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InvalidateCacheAttribute : Attribute
    {
        /// <summary>
        /// 要失效的缓存键前缀
        /// </summary>
        public string[] CacheKeyPrefixes { get; }

        /// <summary>
        /// 创建缓存失效特性
        /// </summary>
        /// <param name="cacheKeyPrefixes">要失效的缓存键前缀</param>
        public InvalidateCacheAttribute(params string[] cacheKeyPrefixes)
        {
            CacheKeyPrefixes = cacheKeyPrefixes;
        }
    }
}
