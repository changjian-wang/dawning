using Dawning.Identity.Application.Interfaces.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Monitoring;

/// <summary>
/// Alert check background service
/// Periodically checks system metrics and triggers alerts
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

        // Default: check every minute
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

        // Wait for application startup to complete
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
                // Normal shutdown
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
