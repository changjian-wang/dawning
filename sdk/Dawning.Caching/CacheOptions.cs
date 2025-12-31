namespace Dawning.Caching;

/// <summary>
/// Cache configuration options
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Caching";

    /// <summary>
    /// Cache provider type
    /// </summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>
    /// Default expiration time in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Key prefix
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Redis configuration
    /// </summary>
    public RedisOptions Redis { get; set; } = new();
}

/// <summary>
/// Redis configuration options
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Instance name
    /// </summary>
    public string InstanceName { get; set; } = "Dawning:";

    /// <summary>
    /// Database index
    /// </summary>
    public int Database { get; set; } = 0;
}

/// <summary>
/// Cache provider type
/// </summary>
public enum CacheProvider
{
    /// <summary>
    /// In-memory cache
    /// </summary>
    Memory,

    /// <summary>
    /// Redis distributed cache
    /// </summary>
    Redis,
}
