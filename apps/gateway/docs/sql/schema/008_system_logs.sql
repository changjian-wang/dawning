-- Create system logs table
CREATE TABLE IF NOT EXISTS `system_logs` (
    `id` CHAR(36) NOT NULL COMMENT 'Log unique identifier (GUID)',
    `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID',
    `level` VARCHAR(20) NOT NULL COMMENT 'Log level (Info, Warning, Error)',
    `message` TEXT NOT NULL COMMENT 'Log message',
    `exception` TEXT NULL COMMENT 'Exception information (exception type and message)',
    `stack_trace` TEXT NULL COMMENT 'Exception stack trace',
    `source` VARCHAR(255) NULL COMMENT 'Exception source',
    `user_id` CHAR(36) NULL COMMENT 'Operating user ID',
    `username` VARCHAR(256) NULL COMMENT 'Operating username',
    `ip_address` VARCHAR(50) NULL COMMENT 'IP address',
    `user_agent` TEXT NULL COMMENT 'User agent',
    `request_path` VARCHAR(500) NULL COMMENT 'Request path',
    `request_method` VARCHAR(10) NULL COMMENT 'Request method (GET, POST, etc.)',
    `status_code` INT NULL COMMENT 'HTTP status code',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time',
    `timestamp` BIGINT NOT NULL COMMENT 'Timestamp (for sorting and query optimization)',
    PRIMARY KEY (`id`),
    INDEX `idx_level` (`level` ASC),
    INDEX `idx_created_at` (`created_at` DESC),
    INDEX `idx_timestamp` (`timestamp` DESC),
    INDEX `idx_user_id` (`user_id` ASC),
    INDEX `idx_username` (`username` ASC),
    INDEX `idx_level_created` (`level`, `created_at` DESC),
    INDEX `idx_system_logs_tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='System logs table (application logs and exceptions)';

-- Create monthly partitions (example: January-December 2025)
-- Note: Use when database supports partitioning
-- ALTER TABLE `system_logs` PARTITION BY RANGE (YEAR(created_at) * 100 + MONTH(created_at)) (
--     PARTITION p202501 VALUES LESS THAN (202502),
--     PARTITION p202502 VALUES LESS THAN (202503),
--     PARTITION p202503 VALUES LESS THAN (202504),
--     PARTITION p202504 VALUES LESS THAN (202505),
--     PARTITION p202505 VALUES LESS THAN (202506),
--     PARTITION p202506 VALUES LESS THAN (202507),
--     PARTITION p202507 VALUES LESS THAN (202508),
--     PARTITION p202508 VALUES LESS THAN (202509),
--     PARTITION p202509 VALUES LESS THAN (202510),
--     PARTITION p202510 VALUES LESS THAN (202511),
--     PARTITION p202511 VALUES LESS THAN (202512),
--     PARTITION p202512 VALUES LESS THAN (202513),
--     PARTITION p_future VALUES LESS THAN MAXVALUE
-- );
