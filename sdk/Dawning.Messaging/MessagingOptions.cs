namespace Dawning.Messaging;

/// <summary>
/// 消息配置选项
/// </summary>
public class MessagingOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// 消息提供程序类型
    /// </summary>
    public MessagingProvider Provider { get; set; } = MessagingProvider.RabbitMQ;

    /// <summary>
    /// 默认交换机/主题名称
    /// </summary>
    public string DefaultExchange { get; set; } = "dawning.events";

    /// <summary>
    /// 消息序列化格式
    /// </summary>
    public SerializationFormat SerializationFormat { get; set; } = SerializationFormat.Json;

    /// <summary>
    /// RabbitMQ 配置
    /// </summary>
    public RabbitMQOptions RabbitMQ { get; set; } = new();

    /// <summary>
    /// Azure Service Bus 配置
    /// </summary>
    public ServiceBusOptions ServiceBus { get; set; } = new();
}

/// <summary>
/// RabbitMQ 配置选项
/// </summary>
public class RabbitMQOptions
{
    /// <summary>
    /// 主机地址
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// 虚拟主机
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// 交换机类型
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// 是否持久化
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// 预取数量
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
}

/// <summary>
/// Azure Service Bus 配置选项
/// </summary>
public class ServiceBusOptions
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 主题名称
    /// </summary>
    public string TopicName { get; set; } = "dawning-events";

    /// <summary>
    /// 订阅名称
    /// </summary>
    public string SubscriptionName { get; set; } = "default";

    /// <summary>
    /// 最大并发调用数
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>
    /// 自动完成消息
    /// </summary>
    public bool AutoCompleteMessages { get; set; } = true;
}

/// <summary>
/// 消息提供程序类型
/// </summary>
public enum MessagingProvider
{
    /// <summary>
    /// RabbitMQ
    /// </summary>
    RabbitMQ,

    /// <summary>
    /// Azure Service Bus
    /// </summary>
    AzureServiceBus
}

/// <summary>
/// 消息序列化格式
/// </summary>
public enum SerializationFormat
{
    /// <summary>
    /// JSON 格式
    /// </summary>
    Json
}
