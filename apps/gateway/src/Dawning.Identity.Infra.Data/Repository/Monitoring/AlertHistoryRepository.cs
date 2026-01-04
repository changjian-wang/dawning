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
/// Alert history repository implementation
/// </summary>
public class AlertHistoryRepository : IAlertHistoryRepository
{
    private readonly DbContext _context;

    public AlertHistoryRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Paged query for alert history
    /// </summary>
    public async Task<PagedData<AlertHistory>> GetPagedListAsync(
        AlertHistoryQueryModel model,
        int page,
        int pageSize
    )
    {
        var builder = _context.Connection.Builder<AlertHistoryEntity>(_context.Transaction);

        // Apply filter conditions
        builder = builder
            .WhereIf(model.RuleId.HasValue, x => x.RuleId == model.RuleId!.Value)
            .WhereIf(!string.IsNullOrEmpty(model.MetricType), x => x.MetricType == model.MetricType)
            .WhereIf(!string.IsNullOrEmpty(model.Severity), x => x.Severity == model.Severity)
            .WhereIf(!string.IsNullOrEmpty(model.Status), x => x.Status == model.Status)
            .WhereIf(model.StartTime.HasValue, x => x.TriggeredAt >= model.StartTime!.Value)
            .WhereIf(model.EndTime.HasValue, x => x.TriggeredAt <= model.EndTime!.Value);

        // Sort by trigger time descending
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
    /// Get alert history by ID
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
    /// Create alert history
    /// </summary>
    public async Task<long> CreateAsync(AlertHistory history)
    {
        var entity = history.ToEntity();
        entity.TriggeredAt = DateTime.UtcNow;

        var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
        return result;
    }

    /// <summary>
    /// Update alert status to acknowledged
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
    /// Update alert status to resolved
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
    /// Update alert status
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
    /// Get unresolved alerts
    /// </summary>
    public async Task<IEnumerable<AlertHistory>> GetUnresolvedAsync()
    {
        // Use two queries and merge results, because Builder pattern does not support OR conditions
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
    /// Update notification send status
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
    /// Get alert statistics
    /// </summary>
    public async Task<AlertStatistics> GetStatisticsAsync()
    {
        var stats = new AlertStatistics();
        var today = DateTime.UtcNow.Date;

        // Get all rules statistics
        var allRules = await _context
            .Connection.Builder<AlertRuleEntity>(_context.Transaction)
            .AsListAsync();
        stats.TotalRules = allRules.Count();
        stats.EnabledRules = allRules.Count(r => r.IsEnabled);

        // Get today's alerts
        var todayAlerts = await _context
            .Connection.Builder<AlertHistoryEntity>(_context.Transaction)
            .Where(x => x.TriggeredAt >= today)
            .AsListAsync();
        var todayList = todayAlerts.ToList();
        stats.TotalAlertsToday = todayList.Count;

        // Group by metric type
        stats.AlertsByMetricType = todayList
            .GroupBy(x => x.MetricType)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by severity
        stats.AlertsBySeverity = todayList
            .GroupBy(x => x.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        // Unresolved alerts statistics - two separate queries
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
