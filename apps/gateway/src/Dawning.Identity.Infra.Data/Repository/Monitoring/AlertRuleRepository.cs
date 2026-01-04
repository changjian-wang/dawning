using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Interfaces.Monitoring;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Monitoring;

/// <summary>
/// Alert rule repository implementation
/// </summary>
public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly DbContext _context;

    public AlertRuleRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all alert rules
    /// </summary>
    public async Task<IEnumerable<AlertRule>> GetAllAsync()
    {
        var entities = await _context
            .Connection.Builder<AlertRuleEntity>(_context.Transaction)
            .OrderByDescending(r => r.CreatedAt)
            .AsListAsync();

        return entities.ToModels();
    }

    /// <summary>
    /// Get enabled alert rules
    /// </summary>
    public async Task<IEnumerable<AlertRule>> GetEnabledAsync()
    {
        var entities = await _context
            .Connection.Builder<AlertRuleEntity>(_context.Transaction)
            .Where(r => r.IsEnabled == true)
            .OrderByDescending(r => r.Severity)
            .OrderBy(r => r.Name)
            .AsListAsync();

        return entities.ToModels();
    }

    /// <summary>
    /// Get alert rule by ID
    /// </summary>
    public async Task<AlertRule?> GetByIdAsync(long id)
    {
        var entity = await _context.Connection.GetAsync<AlertRuleEntity>(id, _context.Transaction);
        return entity?.ToModel();
    }

    /// <summary>
    /// Create alert rule
    /// </summary>
    public async Task<long> CreateAsync(AlertRule rule)
    {
        var entity = rule.ToEntity();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
        return result;
    }

    /// <summary>
    /// Update alert rule
    /// </summary>
    public async Task<bool> UpdateAsync(AlertRule rule)
    {
        var entity = rule.ToEntity();
        entity.UpdatedAt = DateTime.UtcNow;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// Delete alert rule
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Connection.GetAsync<AlertRuleEntity>(id, _context.Transaction);
        if (entity == null)
            return false;

        return await _context.Connection.DeleteAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// Update enabled status
    /// </summary>
    public async Task<bool> SetEnabledAsync(long id, bool isEnabled)
    {
        var entity = await _context.Connection.GetAsync<AlertRuleEntity>(id, _context.Transaction);
        if (entity == null)
            return false;

        entity.IsEnabled = isEnabled;
        entity.UpdatedAt = DateTime.UtcNow;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// Update last triggered time
    /// </summary>
    public async Task<bool> UpdateLastTriggeredAsync(long id, DateTime triggeredAt)
    {
        var entity = await _context.Connection.GetAsync<AlertRuleEntity>(id, _context.Transaction);
        if (entity == null)
            return false;

        entity.LastTriggeredAt = triggeredAt;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }
}
