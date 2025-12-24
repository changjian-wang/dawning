using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.Monitoring;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Monitoring;

/// <summary>
/// 告警历史仓储实现
/// </summary>
public class AlertHistoryRepository : IAlertHistoryRepository
{
    private readonly DbContext _context;

    public AlertHistoryRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 分页查询告警历史
    /// </summary>
    public async Task<PagedData<AlertHistory>> GetPagedListAsync(
        AlertHistoryQueryModel model,
        int page,
        int pageSize
    )
    {
        var builder = _context.Connection.Builder<AlertHistoryEntity>(_context.Transaction);

        // 应用过滤条件
        builder = builder
            .WhereIf(model.RuleId.HasValue, x => x.RuleId == model.RuleId!.Value)
            .WhereIf(!string.IsNullOrEmpty(model.MetricType), x => x.MetricType == model.MetricType)
            .WhereIf(!string.IsNullOrEmpty(model.Severity), x => x.Severity == model.Severity)
            .WhereIf(!string.IsNullOrEmpty(model.Status), x => x.Status == model.Status)
            .WhereIf(model.StartTime.HasValue, x => x.TriggeredAt >= model.StartTime!.Value)
            .WhereIf(model.EndTime.HasValue, x => x.TriggeredAt <= model.EndTime!.Value);

        // 按触发时间降序排序
        var result = await builder
            .OrderByDescending(x => x.TriggeredAt)
            .AsPagedListAsync(page, pageSize);

        return new PagedData<AlertHistory>
        {
            PageIndex = page,
            PageSize = pageSize,
            TotalCount = result.TotalItems,
            Items = result.Values.ToModels(),
        };
    }

    /// <summary>
    /// 根据ID获取告警历史
    /// </summary>
    public async Task<AlertHistory?> GetByIdAsync(long id)
    {
        var entity = await _context.Connection.GetAsync<AlertHistoryEntity>(
            id,
            _context.Transaction
        );
        return entity?.ToModel();
    }

    /// <summary>
    /// 创建告警历史
    /// </summary>
    public async Task<long> CreateAsync(AlertHistory history)
    {
        var entity = history.ToEntity();
        entity.TriggeredAt = DateTime.UtcNow;

        var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
        return result;
    }

    /// <summary>
    /// 更新告警状态为已确认
    /// </summary>
    public async Task<bool> AcknowledgeAsync(long id, string acknowledgedBy)
    {
        var entity = await _context.Connection.GetAsync<AlertHistoryEntity>(
            id,
            _context.Transaction
        );
        if (entity == null)
            return false;

        entity.Status = "acknowledged";
        entity.AcknowledgedAt = DateTime.UtcNow;
        entity.AcknowledgedBy = acknowledgedBy;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 更新告警状态为已解决
    /// </summary>
    public async Task<bool> ResolveAsync(long id, string resolvedBy)
    {
        var entity = await _context.Connection.GetAsync<AlertHistoryEntity>(
            id,
            _context.Transaction
        );
        if (entity == null)
            return false;

        entity.Status = "resolved";
        entity.ResolvedAt = DateTime.UtcNow;
        entity.ResolvedBy = resolvedBy;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 更新告警状态
    /// </summary>
    public async Task<bool> UpdateStatusAsync(long id, string status)
    {
        var entity = await _context.Connection.GetAsync<AlertHistoryEntity>(
            id,
            _context.Transaction
        );
        if (entity == null)
            return false;

        entity.Status = status;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 获取未解决的告警
    /// </summary>
    public async Task<IEnumerable<AlertHistory>> GetUnresolvedAsync()
    {
        // 使用两次查询合并结果，因为 Builder 模式不支持 OR 条件
        var triggeredEntities = await _context
            .Connection.Builder<AlertHistoryEntity>(_context.Transaction)
            .Where(x => x.Status == "triggered")
            .OrderByDescending(x => x.TriggeredAt)
            .AsListAsync();

        var acknowledgedEntities = await _context
            .Connection.Builder<AlertHistoryEntity>(_context.Transaction)
            .Where(x => x.Status == "acknowledged")
            .OrderByDescending(x => x.TriggeredAt)
            .AsListAsync();

        var combined = triggeredEntities
            .Concat(acknowledgedEntities)
            .OrderByDescending(x => x.TriggeredAt);

        return combined.ToModels();
    }

    /// <summary>
    /// 更新通知发送状态
    /// </summary>
    public async Task<bool> UpdateNotifyResultAsync(long id, bool sent, string? result)
    {
        var entity = await _context.Connection.GetAsync<AlertHistoryEntity>(
            id,
            _context.Transaction
        );
        if (entity == null)
            return false;

        entity.NotifySent = sent;
        entity.NotifyResult = result;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 获取告警统计
    /// </summary>
    public async Task<AlertStatistics> GetStatisticsAsync()
    {
        var stats = new AlertStatistics();
        var today = DateTime.UtcNow.Date;

        // 获取所有规则统计
        var allRules = await _context
            .Connection.Builder<AlertRuleEntity>(_context.Transaction)
            .AsListAsync();
        stats.TotalRules = allRules.Count();
        stats.EnabledRules = allRules.Count(r => r.IsEnabled);

        // 获取今日告警
        var todayAlerts = await _context
            .Connection.Builder<AlertHistoryEntity>(_context.Transaction)
            .Where(x => x.TriggeredAt >= today)
            .AsListAsync();
        var todayList = todayAlerts.ToList();
        stats.TotalAlertsToday = todayList.Count;

        // 按指标类型统计
        stats.AlertsByMetricType = todayList
            .GroupBy(x => x.MetricType)
            .ToDictionary(g => g.Key, g => g.Count());

        // 按严重程度统计
        stats.AlertsBySeverity = todayList
            .GroupBy(x => x.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        // 未解决告警统计 - 分两次查询
        var triggeredAlerts = await _context
            .Connection.Builder<AlertHistoryEntity>(_context.Transaction)
            .Where(x => x.Status == "triggered")
            .AsListAsync();
        var acknowledgedAlerts = await _context
            .Connection.Builder<AlertHistoryEntity>(_context.Transaction)
            .Where(x => x.Status == "acknowledged")
            .AsListAsync();

        var unresolvedList = triggeredAlerts.Concat(acknowledgedAlerts).ToList();
        stats.UnresolvedAlerts = unresolvedList.Count;
        stats.CriticalAlerts = unresolvedList.Count(x => x.Severity == "critical");
        stats.WarningAlerts = unresolvedList.Count(x => x.Severity == "warning");

        return stats;
    }
}
