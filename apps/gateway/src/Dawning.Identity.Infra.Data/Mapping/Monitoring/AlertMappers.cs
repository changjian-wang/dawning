using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

namespace Dawning.Identity.Infra.Data.Mapping.Monitoring
{
    /// <summary>
    /// 告警规则映射器
    /// </summary>
    public static class AlertRuleMappers
    {
        /// <summary>
        /// 将数据库实体转换为领域模型
        /// </summary>
        public static AlertRule ToModel(this AlertRuleEntity entity)
        {
            return new AlertRule
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                MetricType = entity.MetricType,
                Operator = entity.Operator,
                Threshold = entity.Threshold,
                DurationSeconds = entity.DurationSeconds,
                Severity = entity.Severity,
                IsEnabled = entity.IsEnabled,
                NotifyChannels = entity.NotifyChannels,
                NotifyEmails = entity.NotifyEmails,
                WebhookUrl = entity.WebhookUrl,
                CooldownMinutes = entity.CooldownMinutes,
                LastTriggeredAt = entity.LastTriggeredAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        /// <summary>
        /// 将领域模型转换为数据库实体
        /// </summary>
        public static AlertRuleEntity ToEntity(this AlertRule model)
        {
            return new AlertRuleEntity
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                MetricType = model.MetricType,
                Operator = model.Operator,
                Threshold = model.Threshold,
                DurationSeconds = model.DurationSeconds,
                Severity = model.Severity,
                IsEnabled = model.IsEnabled,
                NotifyChannels = model.NotifyChannels,
                NotifyEmails = model.NotifyEmails,
                WebhookUrl = model.WebhookUrl,
                CooldownMinutes = model.CooldownMinutes,
                LastTriggeredAt = model.LastTriggeredAt,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
            };
        }

        /// <summary>
        /// 批量转换实体到模型
        /// </summary>
        public static IEnumerable<AlertRule> ToModels(this IEnumerable<AlertRuleEntity> entities)
        {
            return entities.Select(e => e.ToModel());
        }
    }

    /// <summary>
    /// 告警历史映射器
    /// </summary>
    public static class AlertHistoryMappers
    {
        /// <summary>
        /// 将数据库实体转换为领域模型
        /// </summary>
        public static AlertHistory ToModel(this AlertHistoryEntity entity)
        {
            return new AlertHistory
            {
                Id = entity.Id,
                RuleId = entity.RuleId,
                RuleName = entity.RuleName,
                MetricType = entity.MetricType,
                MetricValue = entity.MetricValue,
                Threshold = entity.Threshold,
                Severity = entity.Severity,
                Message = entity.Message,
                Status = entity.Status,
                TriggeredAt = entity.TriggeredAt,
                AcknowledgedAt = entity.AcknowledgedAt,
                AcknowledgedBy = entity.AcknowledgedBy,
                ResolvedAt = entity.ResolvedAt,
                ResolvedBy = entity.ResolvedBy,
                NotifySent = entity.NotifySent,
                NotifyResult = entity.NotifyResult,
            };
        }

        /// <summary>
        /// 将领域模型转换为数据库实体
        /// </summary>
        public static AlertHistoryEntity ToEntity(this AlertHistory model)
        {
            return new AlertHistoryEntity
            {
                Id = model.Id,
                RuleId = model.RuleId,
                RuleName = model.RuleName,
                MetricType = model.MetricType,
                MetricValue = model.MetricValue,
                Threshold = model.Threshold,
                Severity = model.Severity,
                Message = model.Message,
                Status = model.Status,
                TriggeredAt = model.TriggeredAt,
                AcknowledgedAt = model.AcknowledgedAt,
                AcknowledgedBy = model.AcknowledgedBy,
                ResolvedAt = model.ResolvedAt,
                ResolvedBy = model.ResolvedBy,
                NotifySent = model.NotifySent,
                NotifyResult = model.NotifyResult,
            };
        }

        /// <summary>
        /// 批量转换实体到模型
        /// </summary>
        public static IEnumerable<AlertHistory> ToModels(
            this IEnumerable<AlertHistoryEntity> entities
        )
        {
            return entities.Select(e => e.ToModel());
        }
    }
}
