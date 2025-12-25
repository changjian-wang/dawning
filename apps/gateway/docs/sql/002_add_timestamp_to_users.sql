-- 添加timestamp字段到users表
-- 用于分页查询优化

USE dawning_identity;

-- 添加timestamp列（bigint类型）
ALTER TABLE `users` 
ADD COLUMN `timestamp` BIGINT NOT NULL DEFAULT 0 COMMENT '时间戳（用于分页查询）' AFTER `remark`;

-- 创建索引以优化分页查询
CREATE INDEX `idx_timestamp` ON `users`(`timestamp`);

-- 使用created_at初始化timestamp值（转换为Unix时间戳毫秒）
UPDATE `users` 
SET `timestamp` = UNIX_TIMESTAMP(created_at) * 1000 
WHERE `timestamp` = 0;

-- 验证
SELECT id, username, created_at, timestamp FROM users LIMIT 5;
