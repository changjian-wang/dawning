# Dawning.Messaging

消息服务库，支持 RabbitMQ 和 Azure Service Bus。

## 安装

```bash
dotnet add package Dawning.Messaging
```

## 功能特性

- ✅ 统一的消息发布/订阅接口
- ✅ RabbitMQ 支持
- ✅ Azure Service Bus 支持
- ✅ 批量消息发送
- ✅ 自动重连
- ✅ JSON 序列化

## 快速开始

### RabbitMQ

```csharp
builder.Services.AddDawningRabbitMQ(options =>
{
    options.HostName = "localhost";
    options.Port = 5672;
    options.UserName = "guest";
    options.Password = "guest";
    options.PrefetchCount = 10;
});
```

### Azure Service Bus

```csharp
builder.Services.AddDawningServiceBus(
    "Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=xxx;SharedAccessKey=xxx",
    options =>
    {
        options.TopicName = "my-topic";
        options.SubscriptionName = "my-subscription";
        options.MaxConcurrentCalls = 10;
    });
```

### 发布消息

```csharp
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task CreateOrderAsync(Order order)
    {
        // 保存订单...
        
        // 发布事件
        await _publisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow
        });
    }
}
```

### 订阅消息

```csharp
public class OrderEventHandler : BackgroundService
{
    private readonly IMessageSubscriber _subscriber;
    private readonly ILogger<OrderEventHandler> _logger;

    public OrderEventHandler(IMessageSubscriber subscriber, ILogger<OrderEventHandler> logger)
    {
        _subscriber = subscriber;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _subscriber.SubscribeAsync<OrderCreatedEvent>(
            async (message, ct) =>
            {
                _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", 
                    message.OrderId, message.CustomerId);
                
                // 处理订单创建事件...
            },
            subscriptionName: "order-processor",
            cancellationToken: stoppingToken);
    }
}
```

## 配置选项

### appsettings.json (RabbitMQ)

```json
{
  "Messaging": {
    "Provider": "RabbitMQ",
    "DefaultExchange": "my-app.events",
    "RabbitMQ": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "VirtualHost": "/",
      "ExchangeType": "topic",
      "Durable": true,
      "PrefetchCount": 10
    }
  }
}
```

### appsettings.json (Azure Service Bus)

```json
{
  "Messaging": {
    "Provider": "AzureServiceBus",
    "ServiceBus": {
      "ConnectionString": "Endpoint=sb://xxx.servicebus.windows.net/;...",
      "TopicName": "my-topic",
      "SubscriptionName": "my-subscription",
      "MaxConcurrentCalls": 10,
      "AutoCompleteMessages": true
    }
  }
}
```

## API 参考

### IMessagePublisher

| 方法 | 描述 |
|------|------|
| `PublishAsync<T>(message, routingKey?)` | 发布单条消息 |
| `PublishBatchAsync<T>(messages, routingKey?)` | 批量发布消息 |

### IMessageSubscriber

| 方法 | 描述 |
|------|------|
| `SubscribeAsync<T>(handler, subscriptionName?)` | 订阅消息 |
| `UnsubscribeAsync(subscriptionName)` | 取消订阅 |
