using System.Diagnostics;
using System.Text.Json;
using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Monitoring;

/// <summary>
/// 告警服务实现
/// </summary>
public class AlertService : IAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAlertNotificationService _notificationService;
    private readonly ILogger<AlertService> _logger;

    // 用于存储指标值的内存缓存（简单实现）
    private static readonly Dictionary<string, List<MetricSample>> _metricHistory = new();
    private static readonly object _metricLock = new();

    public AlertService(
        IUnitOfWork unitOfWork,
        IAlertNotificationService notificationService,
        ILogger<AlertService> logger
    )
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    #region 告警规则管理

    public async Task<IEnumerable<AlertRuleDto>> GetAllRulesAsync()
    {
        var rules = await _unitOfWork.AlertRule.GetAllAsync();
        return rules.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertRuleDto>> GetEnabledRulesAsync()
    {
        var rules = await _unitOfWork.AlertRule.GetEnabledAsync();
        return rules.Select(MapToDto);
    }

    public async Task<AlertRuleDto?> GetRuleByIdAsync(long id)
    {
        var rule = await _unitOfWork.AlertRule.GetByIdAsync(id);
        return rule == null ? null : MapToDto(rule);
    }

    public async Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleRequest request)
    {
        var rule = new AlertRule
        {
            Name = request.Name,
            Description = request.Description,
            MetricType = request.MetricType,
            Operator = request.Operator,
            Threshold = request.Threshold,
            DurationSeconds = request.DurationSeconds,
            Severity = request.Severity,
            IsEnabled = request.IsEnabled,
            NotifyChannels = JsonSerializer.Serialize(request.NotifyChannels),
            NotifyEmails = request.NotifyEmails,
            WebhookUrl = request.WebhookUrl,
            CooldownMinutes = request.CooldownMinutes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        rule.Id = await _unitOfWork.AlertRule.CreateAsync(rule);
        _logger.LogInformation("Created alert rule: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return MapToDto(rule);
    }

    public async Task<AlertRuleDto?> UpdateRuleAsync(long id, UpdateAlertRuleRequest request)
    {
        var existingRule = await _unitOfWork.AlertRule.GetByIdAsync(id);
        if (existingRule == null)
            return null;

        existingRule.Name = request.Name;
        existingRule.Description = request.Description;
        existingRule.MetricType = request.MetricType;
        existingRule.Operator = request.Operator;
        existingRule.Threshold = request.Threshold;
        existingRule.DurationSeconds = request.DurationSeconds;
        existingRule.Severity = request.Severity;
        existingRule.IsEnabled = request.IsEnabled;
        existingRule.NotifyChannels = JsonSerializer.Serialize(request.NotifyChannels);
        existingRule.NotifyEmails = request.NotifyEmails;
        existingRule.WebhookUrl = request.WebhookUrl;
        existingRule.CooldownMinutes = request.CooldownMinutes;
        existingRule.UpdatedAt = DateTime.UtcNow;

        var updated = await _unitOfWork.AlertRule.UpdateAsync(existingRule);
        if (!updated)
            return null;

        return await GetRuleByIdAsync(id);
    }

    public async Task<bool> DeleteRuleAsync(long id)
    {
        return await _unitOfWork.AlertRule.DeleteAsync(id);
    }

    public async Task<bool> SetRuleEnabledAsync(long id, bool isEnabled)
    {
        return await _unitOfWork.AlertRule.SetEnabledAsync(id, isEnabled);
    }

    #endregion

    #region 告警历史管理

    public async Task<PagedData<AlertHistoryDto>> GetAlertHistoryAsync(
        AlertHistoryQueryParams queryParams
    )
    {
        var model = new AlertHistoryQueryModel
        {
            RuleId = queryParams.RuleId,
            MetricType = queryParams.MetricType,
            Severity = queryParams.Severity,
            Status = queryParams.Status,
            StartTime = queryParams.StartTime,
            EndTime = queryParams.EndTime,
        };

        var pagedResult = await _unitOfWork.AlertHistory.GetPagedListAsync(
            model,
            queryParams.Page,
            queryParams.PageSize
        );

        return new PagedData<AlertHistoryDto>
        {
            Items = pagedResult.Items.Select(MapHistoryToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            PageIndex = pagedResult.PageIndex,
            PageSize = pagedResult.PageSize,
        };
    }

    public async Task<AlertHistoryDto?> GetAlertHistoryByIdAsync(long id)
    {
        var history = await _unitOfWork.AlertHistory.GetByIdAsync(id);
        return history == null ? null : MapHistoryToDto(history);
    }

    public async Task<bool> UpdateAlertStatusAsync(long id, UpdateAlertStatusRequest request)
    {
        return request.Status switch
        {
            "acknowledged" => await _unitOfWork.AlertHistory.AcknowledgeAsync(id, request.ResolvedBy ?? ""),
            "resolved" => await _unitOfWork.AlertHistory.ResolveAsync(id, request.ResolvedBy ?? ""),
            _ => await _unitOfWork.AlertHistory.UpdateStatusAsync(id, request.Status),
        };
    }

    public async Task<IEnumerable<AlertHistoryDto>> GetUnresolvedAlertsAsync()
    {
        var histories = await _unitOfWork.AlertHistory.GetUnresolvedAsync();
        return histories.Select(MapHistoryToDto);
    }

    #endregion

    #region 告警检查与触发

    public async Task CheckAlertsAsync()
    {
        await TriggerAlertCheckAsync();
    }

    public async Task<AlertCheckResult> TriggerAlertCheckAsync()
    {
        var result = new AlertCheckResult { CheckedAt = DateTime.UtcNow };

        try
        {
            var enabledRules = await _unitOfWork.AlertRule.GetEnabledAsync();
            var rulesList = enabledRules.ToList();
            result.RulesChecked = rulesList.Count;

            // 收集当前系统指标
            var metrics = await CollectCurrentMetricsAsync();

            foreach (var rule in rulesList)
            {
                try
                {
                    if (!metrics.TryGetValue(rule.MetricType, out var currentValue))
                        continue;

                    // 检查是否在冷却期
                    if (
                        rule.LastTriggeredAt.HasValue
                        && DateTime.UtcNow - rule.LastTriggeredAt.Value
                            < TimeSpan.FromMinutes(rule.CooldownMinutes)
                    )
                    {
                        continue;
                    }

                    // 检查是否满足告警条件
                    if (ShouldTriggerAlert(rule, currentValue))
                    {
                        // 检查持续时间
                        if (HasExceededDuration(rule.MetricType, rule.Threshold, rule.Operator, rule.DurationSeconds))
                        {
                            var alert = await CreateAlertHistoryAsync(rule, currentValue);
                            result.AlertsTriggered++;
                            result.TriggeredAlerts.Add(
                                $"{rule.Name}: {currentValue:F2} {rule.Operator} {rule.Threshold}"
                            );

                            // 发送通知
                            await SendAlertNotificationAsync(rule, alert, currentValue);
                            result.NotificationsSent++;

                            // 更新规则最后触发时间
                            await _unitOfWork.AlertRule.UpdateLastTriggeredAsync(rule.Id, DateTime.UtcNow);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Rule {rule.Name}: {ex.Message}");
                    _logger.LogError(ex, "Error checking alert rule: {RuleName}", rule.Name);
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Alert check failed: {ex.Message}");
            _logger.LogError(ex, "Error during alert check");
        }

        return result;
    }

    public async Task<AlertStatisticsDto> GetAlertStatisticsAsync()
    {
        var statistics = await _unitOfWork.AlertHistory.GetStatisticsAsync();

        return new AlertStatisticsDto
        {
            TotalRules = statistics.TotalRules,
            EnabledRules = statistics.EnabledRules,
            TotalAlertsToday = statistics.TotalAlertsToday,
            UnresolvedAlerts = statistics.UnresolvedAlerts,
            CriticalAlerts = statistics.CriticalAlerts,
            WarningAlerts = statistics.WarningAlerts,
            AlertsByMetricType = statistics.AlertsByMetricType,
            AlertsBySeverity = statistics.AlertsBySeverity,
        };
    }

    #endregion

    #region Private Methods

    private async Task<Dictionary<string, decimal>> CollectCurrentMetricsAsync()
    {
        var metrics = new Dictionary<string, decimal>();

        // CPU 使用率 (模拟 - 实际应从系统获取)
        var process = Process.GetCurrentProcess();
        // 简单计算 CPU 使用率
        metrics["cpu"] = (decimal)Math.Min(100, process.TotalProcessorTime.TotalSeconds % 100);

        // 内存使用率
        var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var usedMemory = process.WorkingSet64;
        metrics["memory"] = totalMemory > 0 ? (decimal)usedMemory / totalMemory * 100 : 0;

        // 响应时间和错误率默认返回0（实际应从请求日志仓储获取）
        metrics["response_time"] = 0;
        metrics["error_rate"] = 0;

        // 记录指标历史
        RecordMetricHistory(metrics);

        return await Task.FromResult(metrics);
    }

    private static void RecordMetricHistory(Dictionary<string, decimal> metrics)
    {
        lock (_metricLock)
        {
            var now = DateTime.UtcNow;
            foreach (var (metricType, value) in metrics)
            {
                if (!_metricHistory.ContainsKey(metricType))
                {
                    _metricHistory[metricType] = new List<MetricSample>();
                }

                _metricHistory[metricType].Add(new MetricSample { Timestamp = now, Value = value });

                // 只保留最近10分钟的数据
                _metricHistory[metricType] = _metricHistory[metricType]
                    .Where(s => now - s.Timestamp < TimeSpan.FromMinutes(10))
                    .ToList();
            }
        }
    }

    private static bool HasExceededDuration(
        string metricType,
        decimal threshold,
        string op,
        int durationSeconds
    )
    {
        lock (_metricLock)
        {
            if (!_metricHistory.TryGetValue(metricType, out var samples))
                return true; // 没有历史数据，直接触发

            var cutoff = DateTime.UtcNow.AddSeconds(-durationSeconds);
            var relevantSamples = samples.Where(s => s.Timestamp >= cutoff).ToList();

            if (relevantSamples.Count == 0)
                return true;

            // 检查是否所有样本都满足条件
            return relevantSamples.All(s => CompareValue(s.Value, threshold, op));
        }
    }

    private static bool ShouldTriggerAlert(AlertRule rule, decimal currentValue)
    {
        return CompareValue(currentValue, rule.Threshold, rule.Operator);
    }

    private static bool CompareValue(decimal value, decimal threshold, string op)
    {
        return op switch
        {
            "gt" => value > threshold,
            "gte" => value >= threshold,
            "lt" => value < threshold,
            "lte" => value <= threshold,
            "eq" => value == threshold,
            _ => false,
        };
    }

    private async Task<AlertHistory> CreateAlertHistoryAsync(AlertRule rule, decimal currentValue)
    {
        var message =
            $"告警规则 [{rule.Name}] 触发: {GetMetricTypeDisplay(rule.MetricType)} 当前值 {currentValue:F2} {GetOperatorDisplay(rule.Operator)} 阈值 {rule.Threshold}";

        var history = new AlertHistory
        {
            RuleId = rule.Id,
            RuleName = rule.Name,
            MetricType = rule.MetricType,
            MetricValue = currentValue,
            Threshold = rule.Threshold,
            Severity = rule.Severity,
            Message = message,
            Status = "triggered",
            TriggeredAt = DateTime.UtcNow,
        };

        history.Id = await _unitOfWork.AlertHistory.CreateAsync(history);

        _logger.LogWarning(
            "Alert triggered: {RuleName} - {Message}",
            rule.Name,
            message
        );

        return history;
    }

    private async Task SendAlertNotificationAsync(
        AlertRule rule,
        AlertHistory history,
        decimal currentValue
    )
    {
        var channels = new List<string>();
        if (!string.IsNullOrEmpty(rule.NotifyChannels))
        {
            try
            {
                channels = JsonSerializer.Deserialize<List<string>>(rule.NotifyChannels) ?? new();
            }
            catch
            {
                // ignore
            }
        }

        var context = new AlertNotificationContext
        {
            AlertId = history.Id,
            RuleName = rule.Name,
            MetricType = rule.MetricType,
            MetricValue = currentValue,
            Threshold = rule.Threshold,
            Operator = rule.Operator,
            Severity = rule.Severity,
            Message = history.Message ?? "",
            TriggeredAt = history.TriggeredAt,
            NotifyChannels = channels,
            NotifyEmails = rule.NotifyEmails,
            WebhookUrl = rule.WebhookUrl,
        };

        var result = await _notificationService.SendNotificationsAsync(context);

        // 更新通知发送状态
        await _unitOfWork.AlertHistory.UpdateNotifyResultAsync(
            history.Id,
            result.Success,
            result.ErrorMessage ?? "OK"
        );
    }

    private static AlertRuleDto MapToDto(AlertRule rule)
    {
        var channels = new List<string>();
        if (!string.IsNullOrEmpty(rule.NotifyChannels))
        {
            try
            {
                channels = JsonSerializer.Deserialize<List<string>>(rule.NotifyChannels) ?? new();
            }
            catch
            {
                // ignore
            }
        }

        return new AlertRuleDto
        {
            Id = rule.Id,
            Name = rule.Name,
            Description = rule.Description,
            MetricType = rule.MetricType,
            Operator = rule.Operator,
            Threshold = rule.Threshold,
            DurationSeconds = rule.DurationSeconds,
            Severity = rule.Severity,
            IsEnabled = rule.IsEnabled,
            NotifyChannels = channels,
            NotifyEmails = rule.NotifyEmails,
            WebhookUrl = rule.WebhookUrl,
            CooldownMinutes = rule.CooldownMinutes,
            LastTriggeredAt = rule.LastTriggeredAt,
            CreatedAt = rule.CreatedAt,
        };
    }

    private static AlertHistoryDto MapHistoryToDto(AlertHistory history)
    {
        return new AlertHistoryDto
        {
            Id = history.Id,
            RuleId = history.RuleId,
            RuleName = history.RuleName,
            MetricType = history.MetricType,
            MetricValue = history.MetricValue,
            Threshold = history.Threshold,
            Severity = history.Severity,
            Message = history.Message,
            Status = history.Status,
            TriggeredAt = history.TriggeredAt,
            AcknowledgedAt = history.AcknowledgedAt,
            AcknowledgedBy = history.AcknowledgedBy,
            ResolvedAt = history.ResolvedAt,
            ResolvedBy = history.ResolvedBy,
            NotifySent = history.NotifySent,
        };
    }

    private static string GetMetricTypeDisplay(string metricType) =>
        metricType switch
        {
            "cpu" => "CPU 使用率",
            "memory" => "内存使用率",
            "response_time" => "响应时间",
            "error_rate" => "错误率",
            "request_count" => "请求数量",
            _ => metricType,
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

    #endregion

    private class MetricSample
    {
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
    }
}
