-- 创建系统日志表
CREATE TABLE IF NOT EXISTS `system_logs` (
    `id` CHAR(36) NOT NULL COMMENT '日志唯一标识(GUID)',
    `level` VARCHAR(20) NOT NULL COMMENT '日志级别（Info, Warning, Error）',
    `message` TEXT NOT NULL COMMENT '日志消息',
    `exception` TEXT NULL COMMENT '异常信息（异常类型和消息）',
    `stack_trace` TEXT NULL COMMENT '异常堆栈跟踪',
    `source` VARCHAR(255) NULL COMMENT '异常来源',
    `user_id` CHAR(36) NULL COMMENT '操作用户ID',
    `username` VARCHAR(256) NULL COMMENT '操作用户名',
    `ip_address` VARCHAR(50) NULL COMMENT 'IP地址',
    `user_agent` TEXT NULL COMMENT '用户代理',
    `request_path` VARCHAR(500) NULL COMMENT '请求路径',
    `request_method` VARCHAR(10) NULL COMMENT '请求方法（GET, POST等）',
    `status_code` INT NULL COMMENT 'HTTP状态码',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `timestamp` BIGINT NOT NULL COMMENT '时间戳（用于排序和查询优化）',
    PRIMARY KEY (`id`),
    INDEX `idx_level` (`level` ASC),
    INDEX `idx_created_at` (`created_at` DESC),
    INDEX `idx_timestamp` (`timestamp` DESC),
    INDEX `idx_user_id` (`user_id` ASC),
    INDEX `idx_username` (`username` ASC),
    INDEX `idx_level_created` (`level`, `created_at` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='系统日志表（应用日志和异常）';

-- 创建按月分区（示例：2025年1-12月）
-- 注意：需要在数据库支持分区时使用
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
