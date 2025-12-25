-- ============================================
-- V8: 告警规则和告警历史表
-- 日期: 2025-12-17
-- 描述: 添加告警规则管理和告警历史记录功能
-- ============================================

-- 告警规则表
CREATE TABLE IF NOT EXISTS `alert_rules` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL COMMENT '规则名称',
    `description` VARCHAR(500) NULL COMMENT '规则描述',
    `metric_type` VARCHAR(50) NOT NULL COMMENT '指标类型: cpu, memory, response_time, error_rate, request_count',
    `operator` VARCHAR(20) NOT NULL COMMENT '比较操作符: gt, gte, lt, lte, eq',
    `threshold` DECIMAL(18,4) NOT NULL COMMENT '阈值',
    `duration_seconds` INT NOT NULL DEFAULT 60 COMMENT '持续时间(秒)，超过此时间才触发告警',
    `severity` VARCHAR(20) NOT NULL DEFAULT 'warning' COMMENT '严重程度: info, warning, error, critical',
    `is_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `notify_channels` VARCHAR(500) NULL COMMENT '通知渠道，JSON数组: ["email", "webhook"]',
    `notify_emails` VARCHAR(1000) NULL COMMENT '通知邮箱，逗号分隔',
    `webhook_url` VARCHAR(500) NULL COMMENT 'Webhook URL',
    `cooldown_minutes` INT NOT NULL DEFAULT 5 COMMENT '冷却时间(分钟)，避免重复告警',
    `last_triggered_at` DATETIME NULL COMMENT '上次触发时间',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_alert_rules_metric_type` (`metric_type`),
    INDEX `idx_alert_rules_is_enabled` (`is_enabled`),
    INDEX `idx_alert_rules_severity` (`severity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='告警规则表';

-- 告警历史表
CREATE TABLE IF NOT EXISTS `alert_history` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `rule_id` BIGINT UNSIGNED NOT NULL COMMENT '告警规则ID',
    `rule_name` VARCHAR(100) NOT NULL COMMENT '规则名称(冗余)',
    `metric_type` VARCHAR(50) NOT NULL COMMENT '指标类型',
    `metric_value` DECIMAL(18,4) NOT NULL COMMENT '触发时的指标值',
    `threshold` DECIMAL(18,4) NOT NULL COMMENT '阈值',
    `severity` VARCHAR(20) NOT NULL COMMENT '严重程度',
    `message` VARCHAR(1000) NULL COMMENT '告警消息',
    `status` VARCHAR(20) NOT NULL DEFAULT 'triggered' COMMENT '状态: triggered, acknowledged, resolved',
    `triggered_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '触发时间',
    `acknowledged_at` DATETIME NULL COMMENT '确认时间',
    `acknowledged_by` VARCHAR(100) NULL COMMENT '确认人',
    `resolved_at` DATETIME NULL COMMENT '解决时间',
    `resolved_by` VARCHAR(100) NULL COMMENT '解决人',
    `notify_sent` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否已发送通知',
    `notify_result` VARCHAR(500) NULL COMMENT '通知发送结果',
    INDEX `idx_alert_history_rule_id` (`rule_id`),
    INDEX `idx_alert_history_status` (`status`),
    INDEX `idx_alert_history_severity` (`severity`),
    INDEX `idx_alert_history_triggered_at` (`triggered_at`),
    CONSTRAINT `fk_alert_history_rule` FOREIGN KEY (`rule_id`) REFERENCES `alert_rules`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='告警历史表';

-- 插入默认告警规则
INSERT INTO `alert_rules` (`name`, `description`, `metric_type`, `operator`, `threshold`, `duration_seconds`, `severity`, `is_enabled`, `notify_channels`, `cooldown_minutes`) VALUES
('CPU 使用率过高', '当 CPU 使用率超过 80% 时告警', 'cpu', 'gt', 80.0000, 60, 'warning', 1, '["email"]', 5),
('内存使用率过高', '当内存使用率超过 85% 时告警', 'memory', 'gt', 85.0000, 60, 'warning', 1, '["email"]', 5),
('内存使用率严重', '当内存使用率超过 95% 时告警', 'memory', 'gt', 95.0000, 30, 'critical', 1, '["email", "webhook"]', 5),
('API 响应时间过长', '当平均响应时间超过 2000ms 时告警', 'response_time', 'gt', 2000.0000, 120, 'warning', 1, '["email"]', 10),
('错误率过高', '当错误率超过 5% 时告警', 'error_rate', 'gt', 5.0000, 60, 'error', 1, '["email", "webhook"]', 5);

-- 回滚脚本
-- DROP TABLE IF EXISTS `alert_history`;
-- DROP TABLE IF EXISTS `alert_rules`;
