namespace Dawning.Identity.Application.Interfaces.Messaging;

/// <summary>
/// Kafka configuration options
/// </summary>
public class KafkaOptions
{
    public const string SectionName = "Kafka";

    /// <summary>
    /// Kafka broker address list
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Consumer group ID
    /// </summary>
    public string ConsumerGroupId { get; set; } = "dawning-identity-group";

    /// <summary>
    /// Whether to enable Kafka
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Producer configuration
    /// </summary>
    public KafkaProducerOptions Producer { get; set; } = new();

    /// <summary>
    /// Consumer configuration
    /// </summary>
    public KafkaConsumerOptions Consumer { get; set; } = new();

    /// <summary>
    /// Topic configuration
    /// </summary>
    public KafkaTopicOptions Topics { get; set; } = new();

    // Root-level consumer configuration properties (for direct access)

    /// <summary>
    /// Auto offset reset policy: Earliest, Latest
    /// </summary>
    public string AutoOffsetReset { get; set; } = "Earliest";

    /// <summary>
    /// Auto commit offset
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false;

    /// <summary>
    /// Session timeout (milliseconds)
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 45000;

    /// <summary>
    /// Heartbeat interval (milliseconds)
    /// </summary>
    public int HeartbeatIntervalMs { get; set; } = 15000;

    /// <summary>
    /// Max poll interval (milliseconds)
    /// </summary>
    public int MaxPollIntervalMs { get; set; } = 300000;
}

/// <summary>
/// Kafka producer configuration
/// </summary>
public class KafkaProducerOptions
{
    /// <summary>
    /// Message acknowledgment mode: None=no wait, Leader=Leader acknowledged, All=all replicas acknowledged
    /// </summary>
    public string Acks { get; set; } = "All";

    /// <summary>
    /// Enable idempotence
    /// </summary>
    public bool EnableIdempotence { get; set; } = true;

    /// <summary>
    /// Max in-flight requests
    /// </summary>
    public int MaxInFlight { get; set; } = 5;

    /// <summary>
    /// Max message size (bytes)
    /// </summary>
    public int MaxMessageBytes { get; set; } = 1048576;

    /// <summary>
    /// Send timeout (milliseconds)
    /// </summary>
    public int MessageTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Retry count
    /// </summary>
    public int Retries { get; set; } = 3;

    /// <summary>
    /// Batch send delay (milliseconds)
    /// </summary>
    public int LingerMs { get; set; } = 5;

    /// <summary>
    /// Batch size (bytes)
    /// </summary>
    public int BatchSize { get; set; } = 16384;

    /// <summary>
    /// Compression type: none, gzip, snappy, lz4, zstd
    /// </summary>
    public string CompressionType { get; set; } = "snappy";
}

/// <summary>
/// Kafka consumer configuration
/// </summary>
public class KafkaConsumerOptions
{
    /// <summary>
    /// Auto commit offset
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false;

    /// <summary>
    /// Auto offset reset policy: earliest, latest
    /// </summary>
    public string AutoOffsetReset { get; set; } = "earliest";

    /// <summary>
    /// Session timeout (milliseconds)
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Heartbeat interval (milliseconds)
    /// </summary>
    public int HeartbeatIntervalMs { get; set; } = 3000;

    /// <summary>
    /// Max poll records
    /// </summary>
    public int MaxPollRecords { get; set; } = 500;

    /// <summary>
    /// Fetch max wait time (milliseconds)
    /// </summary>
    public int FetchWaitMaxMs { get; set; } = 500;
}

/// <summary>
/// Kafka topic configuration
/// </summary>
public class KafkaTopicOptions
{
    /// <summary>
    /// Audit log topic
    /// </summary>
    public string AuditLog { get; set; } = "dawning.audit-log";

    /// <summary>
    /// Alert notification topic
    /// </summary>
    public string AlertNotification { get; set; } = "dawning.alert-notification";

    /// <summary>
    /// Email queue topic
    /// </summary>
    public string Email { get; set; } = "dawning.email";

    /// <summary>
    /// User event topic
    /// </summary>
    public string UserEvent { get; set; } = "dawning.user-event";

    /// <summary>
    /// System event topic
    /// </summary>
    public string SystemEvent { get; set; } = "dawning.system-event";

    /// <summary>
    /// Config changed topic
    /// </summary>
    public string ConfigChanged { get; set; } = "dawning.config-changed";

    /// <summary>
    /// Cache invalidation topic
    /// </summary>
    public string CacheInvalidation { get; set; } = "dawning.cache-invalidation";
}
