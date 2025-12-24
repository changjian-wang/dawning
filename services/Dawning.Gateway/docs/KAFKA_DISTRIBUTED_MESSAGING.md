# Kafka 分布式消息队列架构

## 概述

Dawning Gateway 使用 Apache Kafka 作为分布式消息队列，支持以下场景：

- **异步审计日志** - 高并发场景下的日志写入解耦
- **告警通知** - 告警规则触发后的异步通知推送
- **邮件队列** - 支持重试机制的邮件发送
- **配置同步** - 多实例配置变更同步
- **缓存失效** - 分布式缓存一致性
- **用户事件** - 用户行为事件的异步处理

## 架构图

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Dawning Gateway Cluster                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐         │
│  │ Identity API #1 │  │ Identity API #2 │  │ Identity API #3 │         │
│  │    (Producer)   │  │    (Producer)   │  │    (Producer)   │         │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘         │
│           │                    │                    │                   │
│           └──────────────┬─────┴────────────────────┘                   │
│                          ▼                                              │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                        Apache Kafka                               │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌────────────────┐  │  │
│  │  │ dawning.audit-log│  │ dawning.alert    │  │ dawning.email  │  │  │
│  │  │   (3 partitions) │  │   (3 partitions) │  │ (3 partitions) │  │  │
│  │  └──────────────────┘  └──────────────────┘  └────────────────┘  │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌────────────────┐  │  │
│  │  │ dawning.config   │  │ dawning.cache    │  │ dawning.user   │  │  │
│  │  │   (3 partitions) │  │   (3 partitions) │  │ (3 partitions) │  │  │
│  │  └──────────────────┘  └──────────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                          ▲                                              │
│           ┌──────────────┴─────┬────────────────────┐                   │
│           │                    │                    │                   │
│  ┌────────┴────────┐  ┌────────┴────────┐  ┌────────┴────────┐         │
│  │ Identity API #1 │  │ Identity API #2 │  │ Identity API #3 │         │
│  │   (Consumer)    │  │   (Consumer)    │  │   (Consumer)    │         │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐         │
│  │      MySQL      │  │      Redis      │  │    Zookeeper    │         │
│  │   (主数据库)    │  │   (分布式缓存)   │  │  (Kafka协调器)  │         │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘         │
└─────────────────────────────────────────────────────────────────────────┘
```

## 消息类型

### 1. 审计日志消息 (`AuditLogMessage`)

```csharp
public class AuditLogMessage : MessageBase
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
```

**Topic**: `dawning.audit-log`

**使用场景**:
- 用户登录/登出
- CRUD 操作记录
- 权限变更审计

### 2. 告警通知消息 (`AlertNotificationMessage`)

```csharp
public class AlertNotificationMessage : MessageBase
{
    public Guid AlertRuleId { get; set; }
    public string RuleName { get; set; }
    public string MetricType { get; set; }
    public string Severity { get; set; }
    public double CurrentValue { get; set; }
    public double Threshold { get; set; }
    public string? Description { get; set; }
}
```

**Topic**: `dawning.alert-notification`

**使用场景**:
- CPU/内存告警
- 错误率告警
- 请求延迟告警

### 3. 邮件消息 (`EmailMessage`)

```csharp
public class EmailMessage : MessageBase
{
    public string To { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; }
    public int Priority { get; set; }
    public int RetryCount { get; set; }
}
```

**Topic**: `dawning.email-queue`

**使用场景**:
- 密码重置邮件
- 欢迎邮件
- 告警邮件通知

### 4. 配置变更消息 (`ConfigChangedMessage`)

```csharp
public class ConfigChangedMessage : MessageBase
{
    public string ConfigGroup { get; set; }
    public string ConfigKey { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
}
```

**Topic**: `dawning.config-changed`

**使用场景**:
- 系统配置更新
- 多实例配置同步

### 5. 缓存失效消息 (`CacheInvalidationMessage`)

```csharp
public class CacheInvalidationMessage : MessageBase
{
    public string CacheKey { get; set; }
    public string? Pattern { get; set; }
    public string? Region { get; set; }
    public string? Reason { get; set; }
}
```

**Topic**: `dawning.cache-invalidation`

**使用场景**:
- 分布式缓存一致性
- 权限缓存失效
- 配置缓存失效

### 6. 用户事件消息 (`UserEventMessage`)

```csharp
public class UserEventMessage : MessageBase
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string EventType { get; set; }
    public Dictionary<string, object>? EventData { get; set; }
}
```

**Topic**: `dawning.user-event`

**使用场景**:
- 用户创建/删除
- 密码变更
- 角色分配

## 配置

### appsettings.json

```json
{
  "Kafka": {
    "Enabled": true,
    "BootstrapServers": "localhost:9092",
    "ConsumerGroupId": "dawning-identity-group",
    "AutoOffsetReset": "Earliest",
    "EnableAutoCommit": false,
    "SessionTimeoutMs": 45000,
    "HeartbeatIntervalMs": 15000,
    "MaxPollIntervalMs": 300000,
    "Producer": {
      "Acks": "All",
      "EnableIdempotence": true,
      "MaxInFlight": 5,
      "LingerMs": 5,
      "BatchSize": 16384,
      "CompressionType": "snappy"
    },
    "Topics": {
      "AuditLog": "dawning.audit-log",
      "AlertNotification": "dawning.alert-notification",
      "EmailQueue": "dawning.email-queue",
      "UserEvent": "dawning.user-event",
      "SystemEvent": "dawning.system-event",
      "ConfigChanged": "dawning.config-changed",
      "CacheInvalidation": "dawning.cache-invalidation"
    }
  }
}
```

### Docker Compose

```yaml
services:
  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    depends_on:
      - zookeeper
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_AUTO_CREATE_TOPICS_ENABLE: "true"
      KAFKA_NUM_PARTITIONS: 3
```

## 使用示例

### 发布消息

```csharp
public class AuditService
{
    private readonly IMessageBus _messageBus;

    public AuditService(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task LogActionAsync(Guid userId, string action, string entityType)
    {
        await _messageBus.PublishAsync(new AuditLogMessage
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            IpAddress = GetClientIp()
        });
    }
}
```

### 发布配置变更

```csharp
public class SystemConfigService
{
    private readonly IMessageBus _messageBus;

    public async Task UpdateConfigAsync(string group, string key, string value)
    {
        // 更新数据库...

        // 通知所有实例
        await _messageBus.PublishAsync(new ConfigChangedMessage
        {
            ConfigGroup = group,
            ConfigKey = key,
            NewValue = value,
            ChangedBy = GetCurrentUser()
        });
    }
}
```

### 失效分布式缓存

```csharp
public class UserService
{
    private readonly IMessageBus _messageBus;

    public async Task UpdateUserRolesAsync(Guid userId, List<string> roles)
    {
        // 更新角色...

        // 通知所有实例失效缓存
        await _messageBus.PublishAsync(new CacheInvalidationMessage
        {
            Pattern = $"user:permissions:{userId}:*",
            Reason = "User roles updated"
        });
    }
}
```

## 分布式锁

使用 Redis 实现分布式锁，防止多实例同时处理同一资源：

```csharp
public class ScheduledTaskService
{
    private readonly IDistributedLock _distributedLock;

    public async Task ExecuteScheduledTaskAsync()
    {
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            "scheduled-task-cleanup",
            TimeSpan.FromMinutes(5));

        if (lockHandle == null)
        {
            // 其他实例正在执行
            return;
        }

        // 执行任务...
    }
}
```

## 消费者组

所有 Identity API 实例加入同一消费者组 (`dawning-identity-group`)，Kafka 自动进行负载均衡：

- 每个分区只被组内一个消费者处理
- 实例故障时自动重新分配分区
- 支持水平扩展

## 可靠性保证

### Producer

- **幂等性**: `EnableIdempotence = true` 防止重复发送
- **确认机制**: `Acks = All` 确保所有副本确认
- **重试**: 自动重试临时失败

### Consumer

- **手动提交**: `EnableAutoCommit = false` 防止消息丢失
- **至少一次**: 处理成功后才提交 offset
- **死信队列**: 失败消息可转发到 DLQ

## 监控

### Kafka UI

启动调试模式查看 Kafka 状态：

```bash
docker compose --profile debug up -d
```

访问 http://localhost:8080 查看：

- Topic 列表和分区状态
- 消费者组 Lag
- 消息浏览

### 指标

通过 OpenTelemetry 导出 Kafka 指标：

- `kafka.producer.send.total` - 发送消息总数
- `kafka.consumer.lag` - 消费延迟
- `kafka.consumer.poll.time` - 轮询耗时

## 故障处理

### Kafka 不可用

当 Kafka 不可用时，使用 `NullMessageBus` 回退：

```csharp
services.AddSingleton<IMessageBus>(sp =>
{
    var options = sp.GetRequiredService<IOptions<KafkaOptions>>();
    
    if (!options.Value.Enabled)
    {
        return new NullMessageBus(logger);
    }
    
    try
    {
        return new KafkaMessageBus(options, logger);
    }
    catch
    {
        return new NullMessageBus(logger);
    }
});
```

### 消息重试

邮件等重要消息支持自动重试：

```csharp
if (message.RetryCount < 3)
{
    message.RetryCount++;
    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, message.RetryCount)));
    await messageBus.PublishAsync(message);
}
```

## 性能优化

### 批量发送

```csharp
Producer = {
    LingerMs = 5,      // 等待 5ms 积攒批次
    BatchSize = 16384  // 批次大小 16KB
}
```

### 压缩

```csharp
CompressionType = "snappy"  // 使用 Snappy 压缩
```

### 分区策略

按 `TenantId` 或 `UserId` 分区，保证同一用户的消息顺序：

```csharp
await producer.ProduceAsync(topic, new Message<string, string>
{
    Key = message.UserId.ToString(),  // 分区键
    Value = JsonSerializer.Serialize(message)
});
```

## 版本历史

| 版本 | 日期 | 变更 |
|------|------|------|
| 1.0.0 | 2025-01-XX | 初始 Kafka 集成 |
