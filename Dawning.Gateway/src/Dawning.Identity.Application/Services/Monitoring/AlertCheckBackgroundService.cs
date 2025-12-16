using Dawning.Identity.Application.Interfaces.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Monitoring;

/// <summary>
/// 告警检查后台服务
/// 定期检查系统指标并触发告警
/// </summary>
public class AlertCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlertCheckBackgroundService> _logger;

    private TimeSpan _checkInterval;

    public AlertCheckBackgroundService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<AlertCheckBackgroundService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;

        // 默认每分钟检查一次
        _checkInterval = TimeSpan.FromMinutes(
            _configuration.GetValue("Alerting:CheckIntervalMinutes", 1)
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Alert check background service started. Check interval: {Interval}",
            _checkInterval
        );

        // 等待应用启动完成
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAlertsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled alert check");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // 正常停止
                break;
            }
        }

        _logger.LogInformation("Alert check background service stopped");
    }

    private async Task CheckAlertsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

        _logger.LogDebug("Starting scheduled alert check");
        var result = await alertService.TriggerAlertCheckAsync();

        if (result.AlertsTriggered > 0)
        {
            _logger.LogWarning(
                "Alert check completed: {RulesChecked} rules checked, {AlertsTriggered} alerts triggered, {NotificationsSent} notifications sent",
                result.RulesChecked,
                result.AlertsTriggered,
                result.NotificationsSent
            );
        }
        else
        {
            _logger.LogDebug(
                "Alert check completed: {RulesChecked} rules checked, no alerts triggered",
                result.RulesChecked
            );
        }

        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Alert check error: {Error}", error);
            }
        }
    }
}
