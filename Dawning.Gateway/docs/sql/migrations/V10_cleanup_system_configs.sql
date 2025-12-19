-- =============================================
-- 清理旧的空 system_configs 表 - 迁移脚本
-- 版本: V10_cleanup_system_configs.sql
-- 日期: 2025-12-19
-- 说明: 删除未使用的空 system_configs 表（之前创建但未使用）
--       注意：V11 会将 system_configs 重命名为 system_configs
-- =============================================

-- =====================================================
-- 1. 删除旧的空 system_configs 表（如果存在）
-- =====================================================

-- 检查 system_configs 表是否存在，如果存在则删除
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables 
                     WHERE table_schema = DATABASE() AND table_name = 'system_configs');

-- 如果表存在，执行删除
-- 注意：由于 MySQL 不支持条件 DDL，需要手动检查

-- =====================================================
-- 2. 删除旧的 system_configs 表（如果存在）
-- =====================================================
DROP TABLE IF EXISTS system_configs;

-- =====================================================
-- 3. 验证
-- =====================================================
SELECT 'Migration V10 completed: old system_configs table removed (if existed)' AS status;

-- 显示 system_configs 中的配置（V11 会将此表重命名为 system_configs）
SELECT name AS config_group, `key` AS config_key, value, description 
FROM system_configs 
ORDER BY name, `key`;
