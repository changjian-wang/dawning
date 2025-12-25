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
/// 告警规则仓储实现
/// </summary>
public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly DbContext _context;

    public AlertRuleRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取所有告警规则
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
    /// 获取启用的告警规则
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
    /// 根据ID获取告警规则
    /// </summary>
    public async Task<AlertRule?> GetByIdAsync(long id)
    {
        var entity = await _context.Connection.GetAsync<AlertRuleEntity>(id, _context.Transaction);
        return entity?.ToModel();
    }

    /// <summary>
    /// 创建告警规则
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
    /// 更新告警规则
    /// </summary>
    public async Task<bool> UpdateAsync(AlertRule rule)
    {
        var entity = rule.ToEntity();
        entity.UpdatedAt = DateTime.UtcNow;

        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 删除告警规则
    /// </summary>
    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _context.Connection.GetAsync<AlertRuleEntity>(id, _context.Transaction);
        if (entity == null)
            return false;

        return await _context.Connection.DeleteAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 更新启用状态
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
    /// 更新最后触发时间
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
