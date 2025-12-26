-- ============================================
-- V8: Alert Rules and Alert History Tables
-- Date: 2025-12-17
-- Description: Add alert rules management and alert history tracking
-- ============================================

-- Alert rules table
CREATE TABLE IF NOT EXISTS `alert_rules` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL COMMENT 'Rule name',
    `description` VARCHAR(500) NULL COMMENT 'Rule description',
    `metric_type` VARCHAR(50) NOT NULL COMMENT 'Metric type: cpu, memory, response_time, error_rate, request_count',
    `operator` VARCHAR(20) NOT NULL COMMENT 'Comparison operator: gt, gte, lt, lte, eq',
    `threshold` DECIMAL(18,4) NOT NULL COMMENT 'Threshold value',
    `duration_seconds` INT NOT NULL DEFAULT 60 COMMENT 'Duration (seconds), alert triggers only after exceeding this time',
    `severity` VARCHAR(20) NOT NULL DEFAULT 'warning' COMMENT 'Severity level: info, warning, error, critical',
    `is_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Whether enabled',
    `notify_channels` VARCHAR(500) NULL COMMENT 'Notification channels, JSON array: ["email", "webhook"]',
    `notify_emails` VARCHAR(1000) NULL COMMENT 'Notification emails, comma-separated',
    `webhook_url` VARCHAR(500) NULL COMMENT 'Webhook URL',
    `cooldown_minutes` INT NOT NULL DEFAULT 5 COMMENT 'Cooldown time (minutes) to avoid duplicate alerts',
    `last_triggered_at` DATETIME NULL COMMENT 'Last triggered time',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_alert_rules_metric_type` (`metric_type`),
    INDEX `idx_alert_rules_is_enabled` (`is_enabled`),
    INDEX `idx_alert_rules_severity` (`severity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Alert rules table';

-- Alert history table
CREATE TABLE IF NOT EXISTS `alert_history` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `rule_id` BIGINT UNSIGNED NOT NULL COMMENT 'Alert rule ID',
    `rule_name` VARCHAR(100) NOT NULL COMMENT 'Rule name (denormalized)',
    `metric_type` VARCHAR(50) NOT NULL COMMENT 'Metric type',
    `metric_value` DECIMAL(18,4) NOT NULL COMMENT 'Metric value at trigger time',
    `threshold` DECIMAL(18,4) NOT NULL COMMENT 'Threshold value',
    `severity` VARCHAR(20) NOT NULL COMMENT 'Severity level',
    `message` VARCHAR(1000) NULL COMMENT 'Alert message',
    `status` VARCHAR(20) NOT NULL DEFAULT 'triggered' COMMENT 'Status: triggered, acknowledged, resolved',
    `triggered_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Trigger time',
    `acknowledged_at` DATETIME NULL COMMENT 'Acknowledgment time',
    `acknowledged_by` VARCHAR(100) NULL COMMENT 'Acknowledged by',
    `resolved_at` DATETIME NULL COMMENT 'Resolution time',
    `resolved_by` VARCHAR(100) NULL COMMENT 'Resolved by',
    `notify_sent` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Whether notification was sent',
    `notify_result` VARCHAR(500) NULL COMMENT 'Notification send result',
    INDEX `idx_alert_history_rule_id` (`rule_id`),
    INDEX `idx_alert_history_status` (`status`),
    INDEX `idx_alert_history_severity` (`severity`),
    INDEX `idx_alert_history_triggered_at` (`triggered_at`),
    CONSTRAINT `fk_alert_history_rule` FOREIGN KEY (`rule_id`) REFERENCES `alert_rules`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Alert history table';

-- Insert default alert rules
INSERT INTO `alert_rules` (`name`, `description`, `metric_type`, `operator`, `threshold`, `duration_seconds`, `severity`, `is_enabled`, `notify_channels`, `cooldown_minutes`) VALUES
('High CPU Usage', 'Alert when CPU usage exceeds 80%', 'cpu', 'gt', 80.0000, 60, 'warning', 1, '["email"]', 5),
('High Memory Usage', 'Alert when memory usage exceeds 85%', 'memory', 'gt', 85.0000, 60, 'warning', 1, '["email"]', 5),
('Critical Memory Usage', 'Alert when memory usage exceeds 95%', 'memory', 'gt', 95.0000, 30, 'critical', 1, '["email", "webhook"]', 5),
('High API Response Time', 'Alert when average response time exceeds 2000ms', 'response_time', 'gt', 2000.0000, 120, 'warning', 1, '["email"]', 10),
('High Error Rate', 'Alert when error rate exceeds 5%', 'error_rate', 'gt', 5.0000, 60, 'error', 1, '["email", "webhook"]', 5);

-- Rollback script
-- DROP TABLE IF EXISTS `alert_history`;
-- DROP TABLE IF EXISTS `alert_rules`;
