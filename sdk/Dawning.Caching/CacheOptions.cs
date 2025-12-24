namespace Dawning.Caching;

/// <summary>
/// 缓存配置选项
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Caching";

    /// <summary>
    /// 缓存提供程序类型
    /// </summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>
    /// 默认过期时间（分钟）
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// 键前缀
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Redis 配置
    /// </summary>
    public RedisOptions Redis { get; set; } = new();
}

/// <summary>
/// Redis 配置选项
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// 实例名称
    /// </summary>
    public string InstanceName { get; set; } = "Dawning:";

    /// <summary>
    /// 数据库索引
    /// </summary>
    public int Database { get; set; } = 0;
}

/// <summary>
/// 缓存提供程序类型
/// </summary>
public enum CacheProvider
{
    /// <summary>
    /// 内存缓存
    /// </summary>
    Memory,

    /// <summary>
    /// Redis 分布式缓存
    /// </summary>
    Redis
}
