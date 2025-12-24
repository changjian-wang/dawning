namespace Dawning.Identity.Application.Interfaces.Messaging;

/// <summary>
/// Kafka 配置选项
/// </summary>
public class KafkaOptions
{
    public const string SectionName = "Kafka";

    /// <summary>
    /// Kafka Broker 地址列表
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// 消费者组 ID
    /// </summary>
    public string ConsumerGroupId { get; set; } = "dawning-identity-group";

    /// <summary>
    /// 是否启用 Kafka
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 生产者配置
    /// </summary>
    public KafkaProducerOptions Producer { get; set; } = new();

    /// <summary>
    /// 消费者配置
    /// </summary>
    public KafkaConsumerOptions Consumer { get; set; } = new();

    /// <summary>
    /// 主题配置
    /// </summary>
    public KafkaTopicOptions Topics { get; set; } = new();
}

/// <summary>
/// Kafka 生产者配置
/// </summary>
public class KafkaProducerOptions
{
    /// <summary>
    /// 消息确认模式: 0=不等待, 1=Leader确认, -1=所有副本确认
    /// </summary>
    public int Acks { get; set; } = -1;

    /// <summary>
    /// 启用幂等性
    /// </summary>
    public bool EnableIdempotence { get; set; } = true;

    /// <summary>
    /// 消息最大大小（字节）
    /// </summary>
    public int MaxMessageBytes { get; set; } = 1048576;

    /// <summary>
    /// 发送超时（毫秒）
    /// </summary>
    public int MessageTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// 重试次数
    /// </summary>
    public int Retries { get; set; } = 3;

    /// <summary>
    /// 批量发送延迟（毫秒）
    /// </summary>
    public int LingerMs { get; set; } = 5;

    /// <summary>
    /// 批量大小（字节）
    /// </summary>
    public int BatchSize { get; set; } = 16384;

    /// <summary>
    /// 启用压缩
    /// </summary>
    public string CompressionType { get; set; } = "snappy";
}

/// <summary>
/// Kafka 消费者配置
/// </summary>
public class KafkaConsumerOptions
{
    /// <summary>
    /// 自动提交偏移量
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false;

    /// <summary>
    /// 自动偏移量重置策略: earliest, latest
    /// </summary>
    public string AutoOffsetReset { get; set; } = "earliest";

    /// <summary>
    /// 会话超时（毫秒）
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// 心跳间隔（毫秒）
    /// </summary>
    public int HeartbeatIntervalMs { get; set; } = 3000;

    /// <summary>
    /// 最大拉取记录数
    /// </summary>
    public int MaxPollRecords { get; set; } = 500;

    /// <summary>
    /// 拉取最大等待时间（毫秒）
    /// </summary>
    public int FetchWaitMaxMs { get; set; } = 500;
}

/// <summary>
/// Kafka 主题配置
/// </summary>
public class KafkaTopicOptions
{
    /// <summary>
    /// 审计日志主题
    /// </summary>
    public string AuditLog { get; set; } = "dawning.audit-log";

    /// <summary>
    /// 告警通知主题
    /// </summary>
    public string AlertNotification { get; set; } = "dawning.alert-notification";

    /// <summary>
    /// 邮件队列主题
    /// </summary>
    public string Email { get; set; } = "dawning.email";

    /// <summary>
    /// 用户事件主题
    /// </summary>
    public string UserEvent { get; set; } = "dawning.user-event";

    /// <summary>
    /// 系统事件主题
    /// </summary>
    public string SystemEvent { get; set; } = "dawning.system-event";

    /// <summary>
    /// 配置变更主题
    /// </summary>
    public string ConfigChanged { get; set; } = "dawning.config-changed";

    /// <summary>
    /// 缓存失效主题
    /// </summary>
    public string CacheInvalidation { get; set; } = "dawning.cache-invalidation";
}
