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
    private int _defaultExpirationMinutes = 30;
    public int DefaultExpirationMinutes
    {
        get => _defaultExpirationMinutes;
        set => _defaultExpirationMinutes = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "DefaultExpirationMinutes must be greater than 0.");
    }

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
    /// Database index (valid range: 0–15 with default Redis configuration)
    /// </summary>
    private int _database = 0;
    public int Database
    {
        get => _database;
        set => _database = value is >= 0 and <= 15
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Database index must be between 0 and 15.");
    }
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
