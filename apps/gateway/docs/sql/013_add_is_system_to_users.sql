-- 013_add_is_system_to_users.sql
-- 为用户表添加is_system字段，用于标记系统初始化数据

USE dawning_identity;

-- 添加is_system字段
ALTER TABLE `users`
ADD COLUMN `is_system` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否为系统用户（系统用户不可删除/禁用）'
AFTER `is_active`;

-- 创建索引以便快速查询系统用户
CREATE INDEX `idx_users_is_system` ON `users` (`is_system`);

-- 显示结果
SELECT '用户表is_system字段添加成功！' AS message;
DESCRIBE users;
