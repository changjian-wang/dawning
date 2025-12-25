-- V11: Rename system_metadatas table to system_configs
-- 执行时间: 2024
-- 说明: 将 system_metadatas 表重命名为 system_configs，保持与代码命名一致

-- 1. 重命名表
RENAME TABLE `system_metadatas` TO `system_configs`;

-- 2. 更新索引名称（可选，MySQL会自动更新索引的表引用）
-- 如果需要更清晰的索引命名，可以删除旧索引并创建新索引
-- 但这不是必需的，因为索引仍然有效

-- 3. 验证重命名
SELECT TABLE_NAME, TABLE_ROWS 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = DATABASE() 
AND TABLE_NAME = 'system_configs';

-- 4. 显示表结构
DESCRIBE system_configs;

-- 5. 显示配置数据
SELECT * FROM system_configs LIMIT 10;
