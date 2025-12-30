-- =====================================================
-- V6_add_backup_records.sql
-- Database Backup Records Table Creation Script
-- Created: 2025-12-16
-- Description: Store database backup history records
-- =====================================================

-- Backup records table
CREATE TABLE IF NOT EXISTS backup_records (
    id CHAR(36) NOT NULL PRIMARY KEY,
    file_name VARCHAR(512) NOT NULL,
    file_path VARCHAR(1024) NOT NULL,
    file_size_bytes BIGINT NOT NULL DEFAULT 0,
    backup_type VARCHAR(50) NOT NULL DEFAULT 'full',
    created_at DATETIME NOT NULL,
    description VARCHAR(500) NULL,
    is_manual BOOLEAN NOT NULL DEFAULT TRUE,
    status VARCHAR(20) NOT NULL DEFAULT 'success',
    error_message TEXT NULL,
    
    -- Indexes
    INDEX idx_backup_records_created_at (created_at),
    INDEX idx_backup_records_status (status),
    INDEX idx_backup_records_type (backup_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add table comment
ALTER TABLE backup_records COMMENT = 'Database backup records table';

-- =====================================================
-- Backup Configuration Items
-- =====================================================

-- Add backup configuration to system configs table
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'AutoBackupEnabled', 'true', 'Whether to enable automatic backup', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'AutoBackupEnabled'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'AutoBackupIntervalHours', '24', 'Automatic backup interval (hours)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'AutoBackupIntervalHours'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'RetentionDays', '30', 'Backup retention days', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'RetentionDays'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'MaxBackupCount', '50', 'Maximum backup count', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'MaxBackupCount'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'BackupPath', 'backups', 'Backup storage path', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'BackupPath'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'CompressBackups', 'true', 'Whether to compress backup files', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'CompressBackups'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'AutoBackupIncludeLogs', 'false', 'Whether automatic backup includes log tables', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'AutoBackupIncludeLogs'
);

-- =====================================================
-- Verification
-- =====================================================

-- Show table structure
DESCRIBE backup_records;

-- Show configuration
SELECT * FROM system_configs WHERE name = 'Backup';
