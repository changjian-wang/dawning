using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.Administration;
using Dawning.Identity.Infra.Data.Context;
using Dawning.Identity.Infra.Data.Mapping.Administration;
using Dawning.Identity.Infra.Data.PersistentObjects.Administration;
using Dawning.ORM.Dapper;

namespace Dawning.Identity.Infra.Data.Repository.Administration;

/// <summary>
/// 备份记录仓储实现
/// </summary>
public class BackupRecordRepository : IBackupRecordRepository
{
    private readonly DbContext _context;

    public BackupRecordRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取备份历史列表
    /// </summary>
    public async Task<List<BackupRecord>> GetHistoryAsync(int count = 20)
    {
        var entities = await _context
            .Connection.Builder<BackupRecordEntity>(_context.Transaction)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .AsListAsync();

        return entities.ToModels().ToList();
    }

    /// <summary>
    /// 根据ID获取备份记录
    /// </summary>
    public async Task<BackupRecord?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Connection.GetAsync<BackupRecordEntity>(
            id.ToString(),
            _context.Transaction
        );
        return entity?.ToModel();
    }

    /// <summary>
    /// 创建备份记录
    /// </summary>
    public async Task<bool> CreateAsync(BackupRecord record)
    {
        var entity = record.ToEntity();
        var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
        return result > 0;
    }

    /// <summary>
    /// 更新备份记录
    /// </summary>
    public async Task<bool> UpdateAsync(BackupRecord record)
    {
        var entity = record.ToEntity();
        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 删除备份记录
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = new BackupRecordEntity { Id = id.ToString() };
        return await _context.Connection.DeleteAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// 获取过期备份记录
    /// </summary>
    public async Task<List<BackupRecord>> GetExpiredAsync(DateTime cutoffDate)
    {
        var entities = await _context
            .Connection.Builder<BackupRecordEntity>(_context.Transaction)
            .Where(r => r.CreatedAt < cutoffDate)
            .AsListAsync();

        return entities.ToModels().ToList();
    }

    /// <summary>
    /// 批量删除过期备份记录
    /// </summary>
    public async Task<int> DeleteExpiredAsync(DateTime cutoffDate)
    {
        var expiredRecords = await GetExpiredAsync(cutoffDate);
        var count = 0;

        foreach (var record in expiredRecords)
        {
            if (await DeleteAsync(record.Id))
            {
                count++;
            }
        }

        return count;
    }
}
