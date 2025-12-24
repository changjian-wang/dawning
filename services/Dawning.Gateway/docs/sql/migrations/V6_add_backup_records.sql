-- =====================================================
-- V6_add_backup_records.sql
-- 数据库备份记录表创建脚本
-- 创建日期: 2025-12-16
-- 描述: 用于存储数据库备份历史记录
-- =====================================================

-- 备份记录表
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
    
    -- 索引
    INDEX idx_backup_records_created_at (created_at),
    INDEX idx_backup_records_status (status),
    INDEX idx_backup_records_type (backup_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 添加表注释
ALTER TABLE backup_records COMMENT = '数据库备份记录表';

-- =====================================================
-- 备份配置项
-- =====================================================

-- 添加备份配置到系统元数据表
INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'AutoBackupEnabled', 'true', '是否启用自动备份', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'AutoBackupEnabled'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'AutoBackupIntervalHours', '24', '自动备份间隔（小时）', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'AutoBackupIntervalHours'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'RetentionDays', '30', '备份保留天数', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'RetentionDays'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'MaxBackupCount', '50', '最大备份数量', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'MaxBackupCount'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'BackupPath', 'backups', '备份存储路径', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'BackupPath'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'CompressBackups', 'true', '是否压缩备份文件', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'CompressBackups'
);

INSERT INTO system_configs (id, name, `key`, value, description, non_editable, timestamp, created_at)
SELECT UUID(), 'Backup', 'AutoBackupIncludeLogs', 'false', '自动备份是否包含日志表', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM system_configs 
    WHERE name = 'Backup' AND `key` = 'AutoBackupIncludeLogs'
);

-- =====================================================
-- 验证
-- =====================================================

-- 显示表结构
DESCRIBE backup_records;

-- 显示配置
SELECT * FROM system_configs WHERE name = 'Backup';
