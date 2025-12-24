using Dawning.Identity.Application.Interfaces.Caching;
using Dawning.Identity.Application.Interfaces.Messaging;
using Dawning.Identity.Application.Interfaces.Notification;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dawning.Identity.Application.Services.Messaging.Consumers;

/// <summary>
/// 审计日志消息消费者
/// </summary>
public class AuditLogConsumer : KafkaConsumerService<AuditLogMessage>
{
    public AuditLogConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<AuditLogConsumer> logger)
        : base(scopeFactory, options, logger, options.Value.Topics.AuditLog)
    {
    }

    protected override async Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        AuditLogMessage message,
        CancellationToken cancellationToken)
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        // AuditLog 没有构造函数，使用对象初始化器
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = message.UserId,
            Username = message.UserName,
            Action = message.Action,
            EntityType = message.EntityType,
            EntityId = string.IsNullOrEmpty(message.EntityId) ? null : Guid.TryParse(message.EntityId, out var eid) ? eid : null,
            Description = message.Description,
            OldValues = message.OldValue,
            NewValues = message.NewValue,
            IpAddress = message.IpAddress,
            UserAgent = message.UserAgent,
            CreatedAt = DateTime.UtcNow,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await unitOfWork.AuditLog.InsertAsync(auditLog);
        await unitOfWork.CommitAsync();
    }
}

/// <summary>
/// 告警通知消息消费者
/// </summary>
public class AlertNotificationConsumer : KafkaConsumerService<AlertNotificationMessage>
{
    private readonly ILogger<AlertNotificationConsumer> _logger;

    public AlertNotificationConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<AlertNotificationConsumer> logger)
        : base(scopeFactory, options, logger, options.Value.Topics.AlertNotification)
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        AlertNotificationMessage message,
        CancellationToken cancellationToken)
    {
        // 获取实时通知服务（如果可用）
        var notificationService = serviceProvider.GetService<IRealTimeNotificationService>();
        
        if (notificationService != null)
        {
            // 创建实时告警通知
            var alert = new RealTimeAlertNotification
            {
                Type = "alert",
                Title = message.RuleName,
                Message = message.Description ?? $"{message.MetricType} alert triggered",
                Severity = message.Severity,
                RuleName = message.RuleName,
                MetricType = message.MetricType,
                Value = (decimal)message.CurrentValue,
                Threshold = (decimal)message.Threshold
            };

            // 推送实时通知
            await notificationService.SendAlertNotificationAsync(alert);
        }

        // 记录系统日志
        _logger.LogWarning(
            "Alert triggered - Rule: {RuleName}, Metric: {MetricType}, Value: {Value}, Threshold: {Threshold}",
            message.RuleName, message.MetricType, message.CurrentValue, message.Threshold);

        // TODO: 可以在这里添加其他通知渠道（邮件、短信、Webhook等）
    }
}

/// <summary>
/// 邮件发送消息消费者
/// 注意: 需要实现 IEmailService 后才能使用
/// </summary>
public class EmailConsumer : KafkaConsumerService<EmailMessage>
{
    private readonly ILogger<EmailConsumer> _logger;

    public EmailConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<EmailConsumer> logger)
        : base(scopeFactory, options, logger, options.Value.Topics.Email)
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        EmailMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: 当 IEmailService 实现后取消注释
            // var emailService = serviceProvider.GetService<IEmailService>();
            // if (emailService == null)
            // {
            //     _logger.LogWarning("Email service not available. Email to {To} was not sent.", message.To);
            //     return;
            // }
            // await emailService.SendEmailAsync(message.To, message.Subject, message.Body, message.IsHtml);
            
            _logger.LogInformation("Email queued for {To}. Subject: {Subject} (Email service not implemented yet)", 
                message.To, message.Subject);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Retry count: {RetryCount}", message.To, message.RetryCount);
            
            // 如果还有重试次数，重新发布消息
            if (message.RetryCount < 3)
            {
                var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
                message.RetryCount++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, message.RetryCount)), cancellationToken);
                await messageBus.PublishAsync(message, cancellationToken: cancellationToken);
            }
        }
    }
}

/// <summary>
/// 配置变更消息消费者（分布式配置同步）
/// </summary>
public class ConfigChangedConsumer : KafkaConsumerService<ConfigChangedMessage>
{
    private readonly ILogger<ConfigChangedConsumer> _logger;

    public ConfigChangedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<ConfigChangedConsumer> logger)
        : base(scopeFactory, options, logger, options.Value.Topics.ConfigChanged)
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        ConfigChangedMessage message,
        CancellationToken cancellationToken)
    {
        // 获取缓存服务，失效相关缓存
        var cacheService = serviceProvider.GetService<ICacheService>();
        
        if (cacheService != null)
        {
            var cacheKey = $"config:{message.ConfigGroup}:{message.ConfigKey}";
            await cacheService.RemoveAsync(cacheKey);
            
            _logger.LogInformation(
                "Configuration cache invalidated. Group: {Group}, Key: {Key}, ChangedBy: {ChangedBy}",
                message.ConfigGroup, message.ConfigKey, message.ChangedBy);
        }
    }
}

/// <summary>
/// 缓存失效消息消费者（分布式缓存同步）
/// </summary>
public class CacheInvalidationConsumer : KafkaConsumerService<CacheInvalidationMessage>
{
    private readonly ILogger<CacheInvalidationConsumer> _logger;

    public CacheInvalidationConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<CacheInvalidationConsumer> logger)
        : base(scopeFactory, options, logger, options.Value.Topics.CacheInvalidation)
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        CacheInvalidationMessage message,
        CancellationToken cancellationToken)
    {
        var cacheService = serviceProvider.GetService<ICacheService>();
        
        if (cacheService == null)
        {
            return;
        }

        // 如果有模式，使用前缀匹配删除
        if (!string.IsNullOrEmpty(message.Pattern))
        {
            await cacheService.RemoveByPrefixAsync(message.Pattern);
            _logger.LogDebug("Cache invalidated by pattern: {Pattern}", message.Pattern);
        }
        else
        {
            await cacheService.RemoveAsync(message.CacheKey);
            _logger.LogDebug("Cache invalidated: {Key}", message.CacheKey);
        }
    }
}

/// <summary>
/// 用户事件消息消费者
/// </summary>
public class UserEventConsumer : KafkaConsumerService<UserEventMessage>
{
    private readonly ILogger<UserEventConsumer> _logger;

    public UserEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<UserEventConsumer> logger)
        : base(scopeFactory, options, logger, options.Value.Topics.UserEvent)
    {
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(
        IServiceProvider serviceProvider,
        UserEventMessage message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User event received - Type: {EventType}, UserId: {UserId}, UserName: {UserName}",
            message.EventType, message.UserId, message.UserName);

        // 根据事件类型处理
        switch (message.EventType)
        {
            case "UserCreated":
                // 可以触发欢迎邮件等
                break;
            case "UserDeleted":
                // 可以清理用户相关数据
                break;
            case "PasswordChanged":
                // 可以发送安全通知
                break;
            case "RoleAssigned":
                // 失效权限缓存
                var cacheService = serviceProvider.GetService<ICacheService>();
                if (cacheService != null)
                {
                    await cacheService.RemoveByPrefixAsync($"user:permissions:{message.UserId}:");
                }
                break;
        }

        await Task.CompletedTask;
    }
}
