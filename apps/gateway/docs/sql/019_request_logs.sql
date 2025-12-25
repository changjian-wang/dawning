-- =====================================================
-- V5_add_request_logs.sql
-- 请求日志表创建脚本
-- 创建日期: 2025-12-16
-- 描述: 用于存储API请求日志，支持监控和审计
-- =====================================================

-- 请求日志表
CREATE TABLE IF NOT EXISTS request_logs (
    id CHAR(36) NOT NULL PRIMARY KEY,
    request_id VARCHAR(100) NULL,
    method VARCHAR(10) NOT NULL,
    path VARCHAR(2048) NOT NULL,
    query_string VARCHAR(2048) NULL,
    status_code INT NOT NULL,
    response_time_ms BIGINT NOT NULL,
    client_ip VARCHAR(45) NULL,
    user_agent VARCHAR(1024) NULL,
    user_id CHAR(36) NULL,
    user_name VARCHAR(256) NULL,
    request_time DATETIME NOT NULL,
    request_body_size BIGINT NULL,
    response_body_size BIGINT NULL,
    exception TEXT NULL,
    additional_info JSON NULL,
    
    -- 索引
    INDEX idx_request_logs_request_time (request_time),
    INDEX idx_request_logs_status_code (status_code),
    INDEX idx_request_logs_user_id (user_id),
    INDEX idx_request_logs_client_ip (client_ip),
    INDEX idx_request_logs_path (path(255)),
    INDEX idx_request_logs_method (method),
    
    -- 复合索引用于常见查询
    INDEX idx_request_logs_time_status (request_time, status_code),
    INDEX idx_request_logs_time_path (request_time, path(255))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 添加表注释
ALTER TABLE request_logs COMMENT = 'API请求日志表';

-- =====================================================
-- 日志保留策略配置
-- =====================================================

-- 添加日志保留天数配置
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Logging', 'RequestLogRetentionDays', '30', '请求日志保留天数', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Logging' AND `key` = 'RequestLogRetentionDays'
);

-- 添加慢请求阈值配置
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Logging', 'SlowRequestThresholdMs', '1000', '慢请求阈值（毫秒）', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Logging' AND `key` = 'SlowRequestThresholdMs'
);

-- 添加请求日志启用配置
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Logging', 'RequestLoggingEnabled', 'true', '是否启用请求日志记录', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Logging' AND `key` = 'RequestLoggingEnabled'
);

-- =====================================================
-- 定期清理事件（可选，需要MySQL事件调度器）
-- =====================================================

-- 启用事件调度器（如果尚未启用）
-- SET GLOBAL event_scheduler = ON;

-- 创建日志清理事件（每天凌晨3点执行）
-- DELIMITER //
-- CREATE EVENT IF NOT EXISTS cleanup_request_logs
-- ON SCHEDULE EVERY 1 DAY
-- STARTS TIMESTAMP(CURRENT_DATE + INTERVAL 1 DAY, '03:00:00')
-- DO
-- BEGIN
--     DECLARE retention_days INT DEFAULT 30;
--     
--     -- 从配置表获取保留天数
--     SELECT CAST(value AS UNSIGNED) INTO retention_days
--     FROM system_configs 
--     WHERE name = 'Logging' AND `key` = 'RequestLogRetentionDays'
--     LIMIT 1;
--     
--     -- 删除过期日志
--     DELETE FROM request_logs 
--     WHERE request_time < DATE_SUB(NOW(), INTERVAL retention_days DAY);
-- END //
-- DELIMITER ;

-- =====================================================
-- 验证
-- =====================================================

-- 显示表结构
DESCRIBE request_logs;

-- 显示配置
SELECT * FROM system_configs WHERE name = 'Logging';
