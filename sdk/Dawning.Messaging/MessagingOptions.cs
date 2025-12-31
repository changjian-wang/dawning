namespace Dawning.Messaging;

/// <summary>
/// Messaging configuration options
/// </summary>
public class MessagingOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// Messaging provider type
    /// </summary>
    public MessagingProvider Provider { get; set; } = MessagingProvider.RabbitMQ;

    /// <summary>
    /// Default exchange/topic name
    /// </summary>
    public string DefaultExchange { get; set; } = "dawning.events";

    /// <summary>
    /// Message serialization format
    /// </summary>
    public SerializationFormat SerializationFormat { get; set; } = SerializationFormat.Json;

    /// <summary>
    /// RabbitMQ configuration
    /// </summary>
    public RabbitMQOptions RabbitMQ { get; set; } = new();

    /// <summary>
    /// Azure Service Bus configuration
    /// </summary>
    public ServiceBusOptions ServiceBus { get; set; } = new();
}

/// <summary>
/// RabbitMQ configuration options
/// </summary>
public class RabbitMQOptions
{
    /// <summary>
    /// Host address
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Exchange type
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Whether to persist
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Prefetch count
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
}

/// <summary>
/// Azure Service Bus configuration options
/// </summary>
public class ServiceBusOptions
{
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Topic name
    /// </summary>
    public string TopicName { get; set; } = "dawning-events";

    /// <summary>
    /// Subscription name
    /// </summary>
    public string SubscriptionName { get; set; } = "default";

    /// <summary>
    /// Maximum concurrent calls
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>
    /// Auto complete messages
    /// </summary>
    public bool AutoCompleteMessages { get; set; } = true;
}

/// <summary>
/// Messaging provider type
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
    AzureServiceBus,
}

/// <summary>
/// Message serialization format
/// </summary>
public enum SerializationFormat
{
    /// <summary>
    /// JSON format
    /// </summary>
    Json,
}
