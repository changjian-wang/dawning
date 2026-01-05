using System.Net.Http.Json;
using System.Text.Json;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Dawning.Identity.Application.Interfaces.Notification;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Dawning.Identity.Application.Services.Monitoring;

/// <summary>
/// Alert notification service implementation
/// </summary>
public class AlertNotificationService : IAlertNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IRealTimeNotificationService _realTimeNotificationService;
    private readonly ILogger<AlertNotificationService> _logger;

    public AlertNotificationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IRealTimeNotificationService realTimeNotificationService,
        ILogger<AlertNotificationService> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _realTimeNotificationService = realTimeNotificationService;
        _logger = logger;
    }

    public async Task<NotificationResult> SendNotificationsAsync(AlertNotificationContext context)
    {
        var results = new List<NotificationResult>();
        var errors = new List<string>();

        // First send real-time push notification (SignalR)
        try
        {
            await _realTimeNotificationService.SendAlertNotificationAsync(
                new RealTimeAlertNotification
                {
                    Title = $"Alert: {context.RuleName}",
                    Message = context.Message,
                    Severity = context.Severity,
                    RuleId = context.AlertId,
                    RuleName = context.RuleName,
                    MetricType = context.MetricType,
                    Value = context.MetricValue,
                    Threshold = context.Threshold,
                    CreatedAt = context.TriggeredAt,
                    Type = "alert",
                    Data = new Dictionary<string, object>
                    {
                        ["operator"] = context.Operator,
                        ["alertId"] = context.AlertId,
                    },
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Real-time alert push failed, continuing with other notification channels"
            );
        }

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
            // Get email configuration
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

            // Use MailKit to send real email
            var smtpHost = smtpConfig.GetValue<string>("Host") ?? "localhost";
            var smtpPort = smtpConfig.GetValue<int>("Port", 587);
            var smtpUsername = smtpConfig.GetValue<string>("Username");
            var smtpPassword = smtpConfig.GetValue<string>("Password");
            var fromEmail = smtpConfig.GetValue<string>("FromEmail") ?? smtpUsername;
            var fromName = smtpConfig.GetValue<string>("FromName") ?? "Dawning Alert System";
            var useSsl = smtpConfig.GetValue<bool>("UseSsl", true);

            var subject = $"[{context.Severity.ToUpper()}] Alert: {context.RuleName}";
            var body = BuildEmailBody(context);

            // Build email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));

            // Parse recipient email addresses (comma-separated)
            var emailAddresses = context.NotifyEmails.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var email in emailAddresses)
            {
                var trimmedEmail = email.Trim();
                if (!string.IsNullOrEmpty(trimmedEmail))
                {
                    message.To.Add(MailboxAddress.Parse(trimmedEmail));
                }
            }

            message.Subject = subject;

            // Create HTML body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body,
                TextBody = BuildPlainTextBody(context),
            };
            message.Body = bodyBuilder.ToMessageBody();

            _logger.LogInformation(
                "Sending email notification to {Emails}: {Subject}",
                context.NotifyEmails,
                subject
            );

            // Send email
            using var client = new SmtpClient();

            var secureSocketOptions = useSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;
            await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);

            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
            }

            var messageId = await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", messageId);

            return new NotificationResult
            {
                Success = true,
                NotificationId = messageId ?? Guid.NewGuid().ToString(),
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
            _logger.LogError(
                ex,
                "Failed to send webhook notification to {Url}",
                context.WebhookUrl
            );
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
                        <h2 style="margin:0;">⚠️ System Alert Notification</h2>
                        <p style="margin:10px 0 0 0;">{{context.RuleName}}</p>
                    </div>
                    <div class="content">
                        <p class="metric">{{GetMetricTypeDisplay(
                context.MetricType
            )}}: {{context.MetricValue:F2}}</p>
                        <p>{{context.Message}}</p>
                        <table>
                            <tr><td class="label">Severity:</td><td>{{GetSeverityDisplay(
                context.Severity
            )}}</td></tr>
                            <tr><td class="label">Metric Type:</td><td>{{GetMetricTypeDisplay(
                context.MetricType
            )}}</td></tr>
                            <tr><td class="label">Current Value:</td><td>{{context.MetricValue:F2}}</td></tr>
                            <tr><td class="label">Threshold:</td><td>{{GetOperatorDisplay(
                context.Operator
            )}} {{context.Threshold}}</td></tr>
                            <tr><td class="label">Trigger Time:</td><td>{{context.TriggeredAt:yyyy-MM-dd HH:mm:ss}} UTC</td></tr>
                        </table>
                    </div>
                    <div class="footer">
                        <p>This email is automatically sent by Dawning Gateway. Please do not reply directly.</p>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    private static string GetMetricTypeDisplay(string metricType) =>
        metricType switch
        {
            "cpu" => "CPU Usage (%)",
            "memory" => "Memory Usage (%)",
            "response_time" => "Response Time (ms)",
            "error_rate" => "Error Rate (%)",
            "request_count" => "Request Count",
            _ => metricType,
        };

    private static string GetSeverityDisplay(string severity) =>
        severity switch
        {
            "critical" => "🔴 Critical",
            "error" => "🟠 Error",
            "warning" => "🟡 Warning",
            "info" => "🔵 Info",
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

    private static string BuildPlainTextBody(AlertNotificationContext context)
    {
        return $"""
            ⚠️ System Alert Notification
            ================

            Rule Name: {context.RuleName}
            {context.Message}

            Details:
            - Severity: {GetSeverityDisplay(context.Severity)}
            - Metric Type: {GetMetricTypeDisplay(context.MetricType)}
            - Current Value: {context.MetricValue:F2}
            - Threshold: {GetOperatorDisplay(context.Operator)} {context.Threshold}
            - Trigger Time: {context.TriggeredAt:yyyy-MM-dd HH:mm:ss} UTC

            ---
            This email is automatically sent by Dawning Gateway. Please do not reply directly.
            """;
    }
}
