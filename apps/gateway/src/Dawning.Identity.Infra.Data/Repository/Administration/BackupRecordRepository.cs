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
/// Backup record repository implementation
/// </summary>
public class BackupRecordRepository : IBackupRecordRepository
{
    private readonly DbContext _context;

    public BackupRecordRepository(DbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get backup history list
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
    /// Get backup record by ID
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
    /// Create backup record
    /// </summary>
    public async Task<bool> CreateAsync(BackupRecord record)
    {
        var entity = record.ToEntity();
        var result = await _context.Connection.InsertAsync(entity, _context.Transaction);
        return result > 0;
    }

    /// <summary>
    /// Update backup record
    /// </summary>
    public async Task<bool> UpdateAsync(BackupRecord record)
    {
        var entity = record.ToEntity();
        return await _context.Connection.UpdateAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// Delete backup record
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = new BackupRecordEntity { Id = id.ToString() };
        return await _context.Connection.DeleteAsync(entity, _context.Transaction);
    }

    /// <summary>
    /// Get expired backup records
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
    /// Batch delete expired backup records
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
