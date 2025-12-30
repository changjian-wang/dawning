-- =====================================================
-- V5_add_request_logs.sql
-- Request Logs Table Creation Script
-- Created: 2025-12-16
-- Description: Store API request logs for monitoring and auditing
-- =====================================================

-- Request logs table
CREATE TABLE IF NOT EXISTS request_logs (
    id CHAR(36) NOT NULL PRIMARY KEY,
    tenant_id CHAR(36) NULL,
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
    
    -- Indexes
    INDEX idx_request_logs_request_time (request_time),
    INDEX idx_request_logs_status_code (status_code),
    INDEX idx_request_logs_user_id (user_id),
    INDEX idx_request_logs_client_ip (client_ip),
    INDEX idx_request_logs_path (path(255)),
    INDEX idx_request_logs_method (method),
    
    -- Composite indexes for common queries
    INDEX idx_request_logs_time_status (request_time, status_code),
    INDEX idx_request_logs_time_path (request_time, path(255)),
    INDEX idx_request_logs_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add table comment
ALTER TABLE request_logs COMMENT = 'API request logs table';

-- =====================================================
-- Log Retention Policy Configuration
-- =====================================================

-- Add log retention days configuration
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Logging', 'RequestLogRetentionDays', '30', 'Request log retention days', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Logging' AND `key` = 'RequestLogRetentionDays'
);

-- Add slow request threshold configuration
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Logging', 'SlowRequestThresholdMs', '1000', 'Slow request threshold (milliseconds)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Logging' AND `key` = 'SlowRequestThresholdMs'
);

-- Add request logging enabled configuration
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Logging', 'RequestLoggingEnabled', 'true', 'Whether to enable request logging', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Logging' AND `key` = 'RequestLoggingEnabled'
);

-- =====================================================
-- Scheduled Cleanup Event (Optional, requires MySQL event scheduler)
-- =====================================================

-- Enable event scheduler (if not already enabled)
-- SET GLOBAL event_scheduler = ON;

-- Create log cleanup event (runs daily at 3:00 AM)
-- DELIMITER //
-- CREATE EVENT IF NOT EXISTS cleanup_request_logs
-- ON SCHEDULE EVERY 1 DAY
-- STARTS TIMESTAMP(CURRENT_DATE + INTERVAL 1 DAY, '03:00:00')
-- DO
-- BEGIN
--     DECLARE retention_days INT DEFAULT 30;
--     
--     -- Get retention days from config table
--     SELECT CAST(value AS UNSIGNED) INTO retention_days
--     FROM system_configs 
--     WHERE name = 'Logging' AND `key` = 'RequestLogRetentionDays'
--     LIMIT 1;
--     
--     -- Delete expired logs
--     DELETE FROM request_logs 
--     WHERE request_time < DATE_SUB(NOW(), INTERVAL retention_days DAY);
-- END //
-- DELIMITER ;

-- =====================================================
-- Verification
-- =====================================================

-- Show table structure
DESCRIBE request_logs;

-- Show configuration
SELECT * FROM system_configs WHERE name = 'Logging';
