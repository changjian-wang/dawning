using System.Net.Http.Json;
using System.Text.Json;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Monitoring;

/// <summary>
/// 告警通知服务实现
/// </summary>
public class AlertNotificationService : IAlertNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlertNotificationService> _logger;

    public AlertNotificationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AlertNotificationService> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<NotificationResult> SendNotificationsAsync(AlertNotificationContext context)
    {
        var results = new List<NotificationResult>();
        var errors = new List<string>();

        foreach (var channel in context.NotifyChannels)
        {
            try
            {
                var result = channel switch
                {
                    "email" => await SendEmailNotificationAsync(context),
                    "webhook" => await SendWebhookNotificationAsync(context),
                    _ => new NotificationResult
                    {
                        Success = false,
                        ErrorMessage = $"Unknown channel: {channel}",
                    },
                };

                results.Add(result);
                if (!result.Success && result.ErrorMessage != null)
                {
                    errors.Add(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{channel}: {ex.Message}");
                _logger.LogError(
                    ex,
                    "Failed to send {Channel} notification for alert {AlertId}",
                    channel,
                    context.AlertId
                );
            }
        }

        return new NotificationResult
        {
            Success = results.Any(r => r.Success),
            NotificationId = context.AlertId.ToString(),
            SentAt = DateTime.UtcNow,
            ErrorMessage = errors.Count > 0 ? string.Join("; ", errors) : null,
        };
    }

    public async Task<NotificationResult> SendEmailNotificationAsync(
        AlertNotificationContext context
    )
    {
        if (string.IsNullOrEmpty(context.NotifyEmails))
        {
            return new NotificationResult
            {
                Success = false,
                ErrorMessage = "No email addresses configured",
            };
        }

        try
        {
            // 获取邮件配置
            var smtpConfig = _configuration.GetSection("Email:Smtp");
            var smtpEnabled = smtpConfig.GetValue<bool>("Enabled");

            if (!smtpEnabled)
            {
                _logger.LogWarning(
                    "Email notifications disabled. Would send to: {Emails}",
                    context.NotifyEmails
                );
                return new NotificationResult
                {
                    Success = true,
                    NotificationId = Guid.NewGuid().ToString(),
                    SentAt = DateTime.UtcNow,
                    ErrorMessage = "Email service disabled - notification simulated",
                };
            }

            // TODO: 实现真实的邮件发送
            // 可以使用 MailKit 或其他邮件库
            var subject = $"[{context.Severity.ToUpper()}] 告警: {context.RuleName}";
            var body = BuildEmailBody(context);

            _logger.LogInformation(
                "Sending email notification to {Emails}: {Subject}",
                context.NotifyEmails,
                subject
            );

            // 模拟发送延迟
            await Task.Delay(100);

            return new NotificationResult
            {
                Success = true,
                NotificationId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification");
            return new NotificationResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<NotificationResult> SendWebhookNotificationAsync(
        AlertNotificationContext context
    )
    {
        if (string.IsNullOrEmpty(context.WebhookUrl))
        {
            return new NotificationResult
            {
                Success = false,
                ErrorMessage = "No webhook URL configured",
            };
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var payload = new
            {
                alertId = context.AlertId,
                ruleName = context.RuleName,
                metricType = context.MetricType,
                metricValue = context.MetricValue,
                threshold = context.Threshold,
                @operator = context.Operator,
                severity = context.Severity,
                message = context.Message,
                triggeredAt = context.TriggeredAt.ToString("o"),
            };

            _logger.LogInformation(
                "Sending webhook notification to {Url} for alert {AlertId}",
                context.WebhookUrl,
                context.AlertId
            );

            var response = await client.PostAsJsonAsync(context.WebhookUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                return new NotificationResult
                {
                    Success = true,
                    NotificationId = Guid.NewGuid().ToString(),
                    SentAt = DateTime.UtcNow,
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new NotificationResult
                {
                    Success = false,
                    ErrorMessage =
                        $"Webhook returned {response.StatusCode}: {errorContent[..Math.Min(200, errorContent.Length)]}",
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook notification to {Url}", context.WebhookUrl);
            return new NotificationResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static string BuildEmailBody(AlertNotificationContext context)
    {
        var severityColor = context.Severity switch
        {
            "critical" => "#dc3545",
            "error" => "#fd7e14",
            "warning" => "#ffc107",
            _ => "#17a2b8",
        };

        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: {{severityColor}}; color: white; padding: 20px; border-radius: 8px 8px 0 0; }
                    .content { background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }
                    .metric { font-size: 24px; font-weight: bold; color: {{severityColor}}; }
                    .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
                    table { width: 100%; border-collapse: collapse; }
                    td { padding: 8px; border-bottom: 1px solid #eee; }
                    .label { font-weight: bold; width: 120px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">
                        <h2 style="margin:0;">⚠️ 系统告警通知</h2>
                        <p style="margin:10px 0 0 0;">{{context.RuleName}}</p>
                    </div>
                    <div class="content">
                        <p class="metric">{{GetMetricTypeDisplay(context.MetricType)}}: {{context.MetricValue:F2}}</p>
                        <p>{{context.Message}}</p>
                        <table>
                            <tr><td class="label">告警级别:</td><td>{{GetSeverityDisplay(context.Severity)}}</td></tr>
                            <tr><td class="label">指标类型:</td><td>{{GetMetricTypeDisplay(context.MetricType)}}</td></tr>
                            <tr><td class="label">当前值:</td><td>{{context.MetricValue:F2}}</td></tr>
                            <tr><td class="label">阈值:</td><td>{{GetOperatorDisplay(context.Operator)}} {{context.Threshold}}</td></tr>
                            <tr><td class="label">触发时间:</td><td>{{context.TriggeredAt:yyyy-MM-dd HH:mm:ss}} UTC</td></tr>
                        </table>
                    </div>
                    <div class="footer">
                        <p>此邮件由 Dawning Gateway 系统自动发送，请勿直接回复。</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    private static string GetMetricTypeDisplay(string metricType) =>
        metricType switch
        {
            "cpu" => "CPU 使用率 (%)",
            "memory" => "内存使用率 (%)",
            "response_time" => "响应时间 (ms)",
            "error_rate" => "错误率 (%)",
            "request_count" => "请求数量",
            _ => metricType,
        };

    private static string GetSeverityDisplay(string severity) =>
        severity switch
        {
            "critical" => "🔴 严重",
            "error" => "🟠 错误",
            "warning" => "🟡 警告",
            "info" => "🔵 信息",
            _ => severity,
        };

    private static string GetOperatorDisplay(string op) =>
        op switch
        {
            "gt" => ">",
            "gte" => ">=",
            "lt" => "<",
            "lte" => "<=",
            "eq" => "=",
            _ => op,
        };
}
