using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Caching
{
    /// <summary>
    /// 统一缓存服务接口
    /// 提供 Cache-Aside 模式、缓存穿透防护等功能
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 获取或设置缓存值（Cache-Aside 模式）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">数据获取工厂方法（缓存未命中时调用）</param>
        /// <param name="options">缓存选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>缓存值或新获取的值</returns>
        Task<T?> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// 获取缓存值
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置缓存值
        /// </summary>
        Task SetAsync<T>(
            string key,
            T value,
            CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// 删除缓存
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 按前缀删除缓存（支持通配符）
        /// </summary>
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查缓存是否存在
        /// </summary>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 刷新缓存过期时间
        /// </summary>
        Task RefreshAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取或设置缓存（带缓存穿透防护）
        /// 当数据源返回 null 时，缓存空值以防止缓存穿透
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
    /// 缓存条目选项
    /// </summary>
    public class CacheEntryOptions
    {
        /// <summary>
        /// 绝对过期时间
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// 相对于当前时间的绝对过期时间
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// 滑动过期时间（每次访问后重置）
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// 默认5分钟过期
        /// </summary>
        public static CacheEntryOptions Default =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        /// <summary>
        /// 短期缓存（1分钟）
        /// </summary>
        public static CacheEntryOptions Short =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };

        /// <summary>
        /// 中期缓存（15分钟）
        /// </summary>
        public static CacheEntryOptions Medium =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

        /// <summary>
        /// 长期缓存（1小时）
        /// </summary>
        public static CacheEntryOptions Long =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

        /// <summary>
        /// 创建自定义过期时间选项
        /// </summary>
        public static CacheEntryOptions FromMinutes(int minutes) =>
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes) };

        /// <summary>
        /// 创建滑动过期选项
        /// </summary>
        public static CacheEntryOptions Sliding(TimeSpan slidingExpiration) =>
            new() { SlidingExpiration = slidingExpiration };
    }

    /// <summary>
    /// 缓存键生成器
    /// </summary>
    public static class CacheKeys
    {
        private const string Prefix = "dawning:";

        // 系统配置
        public static string SystemConfig(string group, string key) =>
            $"{Prefix}config:{group}:{key}";

        public static string SystemConfigGroup(string group) => $"{Prefix}config:{group}:*";

        public static string SystemConfigAll => $"{Prefix}config:*";

        // 用户
        public static string User(Guid id) => $"{Prefix}user:{id}";

        public static string UserByUsername(string username) => $"{Prefix}user:name:{username}";

        public static string UserPermissions(Guid userId) => $"{Prefix}user:{userId}:permissions";

        public static string UserRoles(Guid userId) => $"{Prefix}user:{userId}:roles";

        // 角色
        public static string Role(Guid id) => $"{Prefix}role:{id}";

        public static string RoleByName(string name) => $"{Prefix}role:name:{name}";

        public static string AllRoles => $"{Prefix}roles:all";

        // 权限
        public static string Permission(Guid id) => $"{Prefix}permission:{id}";

        public static string AllPermissions => $"{Prefix}permissions:all";

        // 网关配置
        public static string GatewayCluster(Guid id) => $"{Prefix}gateway:cluster:{id}";

        public static string GatewayRoute(Guid id) => $"{Prefix}gateway:route:{id}";

        public static string AllGatewayClusters => $"{Prefix}gateway:clusters:all";
        public static string AllGatewayRoutes => $"{Prefix}gateway:routes:all";

        // 告警
        public static string AlertRules => $"{Prefix}alert:rules";
        public static string AlertStatistics => $"{Prefix}alert:statistics";

        // IP 访问规则
        public static string IpRules => $"{Prefix}ip:rules";

        // 空值标记（用于缓存穿透防护）
        public const string NullMarker = "__NULL__";
    }
}
