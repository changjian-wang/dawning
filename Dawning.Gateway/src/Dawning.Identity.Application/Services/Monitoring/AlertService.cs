using System.Diagnostics;
using System.Text.Json;
using Dapper;
using Dawning.Identity.Application.Dtos.Monitoring;
using Dawning.Identity.Application.Interfaces.Monitoring;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models;
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
        var sql = "SELECT * FROM alert_rules ORDER BY created_at DESC";
        var rules = await _unitOfWork.Connection.QueryAsync<AlertRule>(sql);
        return rules.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertRuleDto>> GetEnabledRulesAsync()
    {
        var sql = "SELECT * FROM alert_rules WHERE is_enabled = 1 ORDER BY severity DESC, name";
        var rules = await _unitOfWork.Connection.QueryAsync<AlertRule>(sql);
        return rules.Select(MapToDto);
    }

    public async Task<AlertRuleDto?> GetRuleByIdAsync(long id)
    {
        var sql = "SELECT * FROM alert_rules WHERE id = @Id";
        var rule = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AlertRule>(
            sql,
            new { Id = id }
        );
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

        var sql =
            @"INSERT INTO alert_rules 
            (name, description, metric_type, operator, threshold, duration_seconds, 
             severity, is_enabled, notify_channels, notify_emails, webhook_url, 
             cooldown_minutes, created_at, updated_at)
            VALUES 
            (@Name, @Description, @MetricType, @Operator, @Threshold, @DurationSeconds,
             @Severity, @IsEnabled, @NotifyChannels, @NotifyEmails, @WebhookUrl,
             @CooldownMinutes, @CreatedAt, @UpdatedAt);
            SELECT LAST_INSERT_ID();";

        rule.Id = await _unitOfWork.Connection.ExecuteScalarAsync<long>(sql, rule);
        _logger.LogInformation("Created alert rule: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return MapToDto(rule);
    }

    public async Task<AlertRuleDto?> UpdateRuleAsync(long id, UpdateAlertRuleRequest request)
    {
        var sql =
            @"UPDATE alert_rules SET
            name = @Name,
            description = @Description,
            metric_type = @MetricType,
            operator = @Operator,
            threshold = @Threshold,
            duration_seconds = @DurationSeconds,
            severity = @Severity,
            is_enabled = @IsEnabled,
            notify_channels = @NotifyChannels,
            notify_emails = @NotifyEmails,
            webhook_url = @WebhookUrl,
            cooldown_minutes = @CooldownMinutes,
            updated_at = @UpdatedAt
            WHERE id = @Id";

        var affected = await _unitOfWork.Connection.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                request.Name,
                request.Description,
                request.MetricType,
                request.Operator,
                request.Threshold,
                request.DurationSeconds,
                request.Severity,
                request.IsEnabled,
                NotifyChannels = JsonSerializer.Serialize(request.NotifyChannels),
                request.NotifyEmails,
                request.WebhookUrl,
                request.CooldownMinutes,
                UpdatedAt = DateTime.UtcNow,
            }
        );

        if (affected == 0)
            return null;

        return await GetRuleByIdAsync(id);
    }

    public async Task<bool> DeleteRuleAsync(long id)
    {
        var sql = "DELETE FROM alert_rules WHERE id = @Id";
        var affected = await _unitOfWork.Connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<bool> SetRuleEnabledAsync(long id, bool isEnabled)
    {
        var sql =
            "UPDATE alert_rules SET is_enabled = @IsEnabled, updated_at = @UpdatedAt WHERE id = @Id";
        var affected = await _unitOfWork.Connection.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                IsEnabled = isEnabled,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        return affected > 0;
    }

    #endregion

    #region 告警历史管理

    public async Task<PagedData<AlertHistoryDto>> GetAlertHistoryAsync(
        AlertHistoryQueryParams queryParams
    )
    {
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (queryParams.RuleId.HasValue)
        {
            whereClause += " AND rule_id = @RuleId";
            parameters.Add("RuleId", queryParams.RuleId.Value);
        }

        if (!string.IsNullOrEmpty(queryParams.MetricType))
        {
            whereClause += " AND metric_type = @MetricType";
            parameters.Add("MetricType", queryParams.MetricType);
        }

        if (!string.IsNullOrEmpty(queryParams.Severity))
        {
            whereClause += " AND severity = @Severity";
            parameters.Add("Severity", queryParams.Severity);
        }

        if (!string.IsNullOrEmpty(queryParams.Status))
        {
            whereClause += " AND status = @Status";
            parameters.Add("Status", queryParams.Status);
        }

        if (queryParams.StartTime.HasValue)
        {
            whereClause += " AND triggered_at >= @StartTime";
            parameters.Add("StartTime", queryParams.StartTime.Value);
        }

        if (queryParams.EndTime.HasValue)
        {
            whereClause += " AND triggered_at <= @EndTime";
            parameters.Add("EndTime", queryParams.EndTime.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM alert_history {whereClause}";
        var totalCount = await _unitOfWork.Connection.ExecuteScalarAsync<int>(
            countSql,
            parameters
        );

        var offset = (queryParams.Page - 1) * queryParams.PageSize;
        var dataSql =
            $@"SELECT * FROM alert_history {whereClause} 
                       ORDER BY triggered_at DESC 
                       LIMIT @PageSize OFFSET @Offset";
        parameters.Add("PageSize", queryParams.PageSize);
        parameters.Add("Offset", offset);

        var histories = await _unitOfWork.Connection.QueryAsync<AlertHistory>(dataSql, parameters);
        var dtos = histories.Select(MapHistoryToDto).ToList();

        return new PagedData<AlertHistoryDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageIndex = queryParams.Page,
            PageSize = queryParams.PageSize,
        };
    }

    public async Task<AlertHistoryDto?> GetAlertHistoryByIdAsync(long id)
    {
        var sql = "SELECT * FROM alert_history WHERE id = @Id";
        var history = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AlertHistory>(
            sql,
            new { Id = id }
        );
        return history == null ? null : MapHistoryToDto(history);
    }

    public async Task<bool> UpdateAlertStatusAsync(long id, UpdateAlertStatusRequest request)
    {
        var sql = request.Status switch
        {
            "acknowledged"
                => @"UPDATE alert_history SET 
                    status = @Status, 
                    acknowledged_at = @Now, 
                    acknowledged_by = @ResolvedBy 
                    WHERE id = @Id",
            "resolved"
                => @"UPDATE alert_history SET 
                    status = @Status, 
                    resolved_at = @Now, 
                    resolved_by = @ResolvedBy 
                    WHERE id = @Id",
            _ => "UPDATE alert_history SET status = @Status WHERE id = @Id",
        };

        var affected = await _unitOfWork.Connection.ExecuteAsync(
            sql,
            new
            {
                Id = id,
                request.Status,
                Now = DateTime.UtcNow,
                request.ResolvedBy,
            }
        );
        return affected > 0;
    }

    public async Task<IEnumerable<AlertHistoryDto>> GetUnresolvedAlertsAsync()
    {
        var sql =
            @"SELECT * FROM alert_history 
                      WHERE status IN ('triggered', 'acknowledged') 
                      ORDER BY 
                        CASE severity 
                            WHEN 'critical' THEN 1 
                            WHEN 'error' THEN 2 
                            WHEN 'warning' THEN 3 
                            ELSE 4 
                        END,
                        triggered_at DESC";
        var histories = await _unitOfWork.Connection.QueryAsync<AlertHistory>(sql);
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
            var enabledRules = await GetEnabledRulesInternalAsync();
            result.RulesChecked = enabledRules.Count();

            // 收集当前系统指标
            var metrics = await CollectCurrentMetricsAsync();

            foreach (var rule in enabledRules)
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
                            await UpdateRuleLastTriggeredAsync(rule.Id);
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
        var stats = new AlertStatisticsDto();

        // 规则统计
        var rulesSql =
            @"SELECT 
            COUNT(*) as TotalRules,
            SUM(CASE WHEN is_enabled = 1 THEN 1 ELSE 0 END) as EnabledRules
            FROM alert_rules";
        var ruleStats = await _unitOfWork.Connection.QueryFirstAsync<dynamic>(rulesSql);
        stats.TotalRules = (int)ruleStats.TotalRules;
        stats.EnabledRules = (int)ruleStats.EnabledRules;

        // 今日告警统计
        var today = DateTime.UtcNow.Date;
        var todaySql =
            @"SELECT COUNT(*) FROM alert_history WHERE triggered_at >= @Today";
        stats.TotalAlertsToday = await _unitOfWork.Connection.ExecuteScalarAsync<int>(
            todaySql,
            new { Today = today }
        );

        // 未解决告警
        var unresolvedSql =
            @"SELECT 
            COUNT(*) as Total,
            SUM(CASE WHEN severity = 'critical' THEN 1 ELSE 0 END) as Critical,
            SUM(CASE WHEN severity = 'warning' THEN 1 ELSE 0 END) as Warning
            FROM alert_history 
            WHERE status IN ('triggered', 'acknowledged')";
        var unresolvedStats = await _unitOfWork.Connection.QueryFirstAsync<dynamic>(unresolvedSql);
        stats.UnresolvedAlerts = (int)unresolvedStats.Total;
        stats.CriticalAlerts = (int)(unresolvedStats.Critical ?? 0);
        stats.WarningAlerts = (int)(unresolvedStats.Warning ?? 0);

        // 按指标类型统计
        var byMetricSql =
            @"SELECT metric_type, COUNT(*) as Count 
                          FROM alert_history 
                          WHERE triggered_at >= @Today
                          GROUP BY metric_type";
        var byMetric = await _unitOfWork.Connection.QueryAsync<dynamic>(
            byMetricSql,
            new { Today = today }
        );
        stats.AlertsByMetricType = byMetric.ToDictionary(
            x => (string)x.metric_type,
            x => (int)x.Count
        );

        // 按严重程度统计
        var bySeveritySql =
            @"SELECT severity, COUNT(*) as Count 
                           FROM alert_history 
                           WHERE triggered_at >= @Today
                           GROUP BY severity";
        var bySeverity = await _unitOfWork.Connection.QueryAsync<dynamic>(
            bySeveritySql,
            new { Today = today }
        );
        stats.AlertsBySeverity = bySeverity.ToDictionary(
            x => (string)x.severity,
            x => (int)x.Count
        );

        return stats;
    }

    #endregion

    #region Private Methods

    private async Task<IEnumerable<AlertRule>> GetEnabledRulesInternalAsync()
    {
        var sql = "SELECT * FROM alert_rules WHERE is_enabled = 1";
        return await _unitOfWork.Connection.QueryAsync<AlertRule>(sql);
    }

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

        // 响应时间 (需要从请求日志获取)
        var avgResponseTime = await GetAverageResponseTimeAsync();
        metrics["response_time"] = avgResponseTime;

        // 错误率 (需要从请求日志获取)
        var errorRate = await GetErrorRateAsync();
        metrics["error_rate"] = errorRate;

        // 记录指标历史
        RecordMetricHistory(metrics);

        return metrics;
    }

    private async Task<decimal> GetAverageResponseTimeAsync()
    {
        try
        {
            var sql =
                @"SELECT AVG(response_time_ms) FROM request_logs 
                      WHERE timestamp >= DATE_SUB(NOW(), INTERVAL 5 MINUTE)";
            var result = await _unitOfWork.Connection.ExecuteScalarAsync<decimal?>(sql);
            return result ?? 0;
        }
        catch
        {
            return 0; // 表不存在时返回0
        }
    }

    private async Task<decimal> GetErrorRateAsync()
    {
        try
        {
            var sql =
                @"SELECT 
                (SUM(CASE WHEN status_code >= 500 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0))
                FROM request_logs 
                WHERE timestamp >= DATE_SUB(NOW(), INTERVAL 5 MINUTE)";
            var result = await _unitOfWork.Connection.ExecuteScalarAsync<decimal?>(sql);
            return result ?? 0;
        }
        catch
        {
            return 0;
        }
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

        var sql =
            @"INSERT INTO alert_history 
            (rule_id, rule_name, metric_type, metric_value, threshold, severity, message, status, triggered_at)
            VALUES 
            (@RuleId, @RuleName, @MetricType, @MetricValue, @Threshold, @Severity, @Message, @Status, @TriggeredAt);
            SELECT LAST_INSERT_ID();";

        history.Id = await _unitOfWork.Connection.ExecuteScalarAsync<long>(sql, history);

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
        var sql =
            @"UPDATE alert_history SET 
            notify_sent = @NotifySent, 
            notify_result = @NotifyResult 
            WHERE id = @Id";
        await _unitOfWork.Connection.ExecuteAsync(
            sql,
            new
            {
                Id = history.Id,
                NotifySent = result.Success,
                NotifyResult = result.ErrorMessage ?? "OK",
            }
        );
    }

    private async Task UpdateRuleLastTriggeredAsync(long ruleId)
    {
        var sql = "UPDATE alert_rules SET last_triggered_at = @Now WHERE id = @Id";
        await _unitOfWork.Connection.ExecuteAsync(
            sql,
            new { Id = ruleId, Now = DateTime.UtcNow }
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
