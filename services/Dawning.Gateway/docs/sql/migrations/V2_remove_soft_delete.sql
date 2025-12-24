-- =====================================================
-- Migration: Remove Soft Delete (is_deleted column)
-- Version: V2
-- Date: 2025-12-08
-- Description: 移除软删除功能，删除 users 表的 is_deleted 字段
-- =====================================================

-- 注意：执行此迁移前请确保已备份数据库
-- 如果需要保留已标记为删除的数据，请先执行以下查询导出：
-- SELECT * FROM users WHERE is_deleted = true;

-- 1. 删除已标记为软删除的用户数据（可选）
-- 如果不需要保留已标记删除的数据，取消下面的注释
-- DELETE FROM users WHERE is_deleted = true;

-- 2. 移除 is_deleted 列
-- MySQL 语法不支持 IF EXISTS，需要先检查列是否存在
SET @dbname = DATABASE();
SET @tablename = 'users';
SET @columnname = 'is_deleted';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE
      (table_name = @tablename)
      AND (table_schema = @dbname)
      AND (column_name = @columnname)
  ) > 0,
  'ALTER TABLE users DROP COLUMN is_deleted;',
  'SELECT ''Column is_deleted does not exist, skipping...'' AS message;'
));
PREPARE alterIfExists FROM @preparedStatement;
EXECUTE alterIfExists;
DEALLOCATE PREPARE alterIfExists;

-- 验证迁移
-- SELECT column_name, data_type 
-- FROM information_schema.columns 
-- WHERE table_name = 'users' 
-- ORDER BY ordinal_position;

-- 完成
SELECT 'Migration V2 completed: is_deleted column removed from users table' AS status;
