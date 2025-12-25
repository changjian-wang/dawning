using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Domain.Models;
using Dawning.Identity.Domain.Models.Monitoring;

namespace Dawning.Identity.Domain.Interfaces.Monitoring;

/// <summary>
/// 告警历史仓储接口
/// </summary>
public interface IAlertHistoryRepository
{
    /// <summary>
    /// 分页查询告警历史
    /// </summary>
    Task<PagedData<AlertHistory>> GetPagedListAsync(
        AlertHistoryQueryModel model,
        int page,
        int pageSize
    );

    /// <summary>
    /// 根据ID获取告警历史
    /// </summary>
    Task<AlertHistory?> GetByIdAsync(long id);

    /// <summary>
    /// 创建告警历史
    /// </summary>
    Task<long> CreateAsync(AlertHistory history);

    /// <summary>
    /// 更新告警状态为已确认
    /// </summary>
    Task<bool> AcknowledgeAsync(long id, string acknowledgedBy);

    /// <summary>
    /// 更新告警状态为已解决
    /// </summary>
    Task<bool> ResolveAsync(long id, string resolvedBy);

    /// <summary>
    /// 更新告警状态
    /// </summary>
    Task<bool> UpdateStatusAsync(long id, string status);

    /// <summary>
    /// 获取未解决的告警
    /// </summary>
    Task<IEnumerable<AlertHistory>> GetUnresolvedAsync();

    /// <summary>
    /// 更新通知发送状态
    /// </summary>
    Task<bool> UpdateNotifyResultAsync(long id, bool sent, string? result);

    /// <summary>
    /// 获取告警统计
    /// </summary>
    Task<AlertStatistics> GetStatisticsAsync();
}
