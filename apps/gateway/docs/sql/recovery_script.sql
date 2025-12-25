-- 完整恢复脚本：恢复角色数据并添加 is_system 字段到 users 表
-- 请在 dawning_identity 数据库中执行此脚本

USE dawning_identity;

-- ============================================
-- 第一部分：为 users 表添加 is_system 字段
-- ============================================

-- 检查 is_system 列是否存在，如果不存在则添加
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dawning_identity' 
    AND TABLE_NAME = 'users' 
    AND COLUMN_NAME = 'is_system'
);

SET @sql = IF(@column_exists = 0, 
    'ALTER TABLE `users` ADD COLUMN `is_system` TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''是否为系统用户（系统用户不可删除/禁用）'' AFTER `is_active`',
    'SELECT ''is_system column already exists'' AS message'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 添加索引（如果不存在）
SET @index_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'dawning_identity' 
    AND TABLE_NAME = 'users' 
    AND INDEX_NAME = 'idx_users_is_system'
);

SET @sql_idx = IF(@index_exists = 0, 
    'CREATE INDEX `idx_users_is_system` ON `users` (`is_system`)',
    'SELECT ''idx_users_is_system index already exists'' AS message'
);

PREPARE stmt2 FROM @sql_idx;
EXECUTE stmt2;
DEALLOCATE PREPARE stmt2;

-- 将 admin 用户标记为系统用户
UPDATE `users` SET `is_system` = 1 WHERE `username` = 'admin';

SELECT 'users 表 is_system 字段处理完成！' AS message;

-- ============================================
-- 第二部分：恢复系统角色数据
-- ============================================

-- 检查并插入 super_admin 角色
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`)
SELECT UUID(), 'super_admin', '超级管理员', '系统最高权限角色，拥有所有权限', 1, 1, JSON_ARRAY('*:*:*'), NOW()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'super_admin');

-- 检查并插入 admin 角色
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`)
SELECT UUID(), 'admin', '系统管理员', '系统管理员角色，可以管理用户、角色、应用等', 1, 1, 
  JSON_ARRAY('user:*:*', 'role:read:*', 'application:*:*', 'scope:*:*', 'claim-type:*:*', 'system-config:*:*'), NOW()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'admin');

-- 检查并插入 user_manager 角色
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`)
SELECT UUID(), 'user_manager', '用户管理员', '负责用户账号管理', 1, 1, 
  JSON_ARRAY('user:read:*', 'user:create:*', 'user:update:*', 'user:reset-password:*'), NOW()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'user_manager');

-- 检查并插入 auditor 角色
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`)
SELECT UUID(), 'auditor', '审计员', '只读权限，可查看所有数据但不能修改', 1, 1, 
  JSON_ARRAY('user:read:*', 'role:read:*', 'application:read:*', 'scope:read:*', 'audit-log:read:*'), NOW()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'auditor');

-- 检查并插入 user 角色
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`)
SELECT UUID(), 'user', '普通用户', '普通用户角色，只能管理自己的信息', 1, 1, 
  JSON_ARRAY('user:read:own', 'user:update:own'), NOW()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'user');

SELECT '角色数据恢复完成！' AS message;

-- ============================================
-- 第三部分：恢复 admin 用户的角色分配
-- ============================================

-- 获取 admin 用户 ID 和 super_admin 角色 ID
SET @admin_user_id = (SELECT id FROM users WHERE username = 'admin' LIMIT 1);
SET @super_admin_role_id = (SELECT id FROM roles WHERE name = 'super_admin' LIMIT 1);

-- 如果 admin 用户存在且 super_admin 角色存在，确保分配关系存在
INSERT INTO `user_roles` (`user_id`, `role_id`, `assigned_at`)
SELECT @admin_user_id, @super_admin_role_id, NOW()
WHERE @admin_user_id IS NOT NULL 
  AND @super_admin_role_id IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM `user_roles` 
    WHERE `user_id` = @admin_user_id AND `role_id` = @super_admin_role_id
  );

SELECT 'admin 用户角色分配完成！' AS message;

-- ============================================
-- 显示最终结果
-- ============================================

SELECT '=== 恢复结果 ===' AS section;
SELECT id, name, display_name, is_system, is_active FROM roles ORDER BY name;

SELECT '=== admin 用户信息 ===' AS section;
SELECT id, username, display_name, is_active, is_system FROM users WHERE username = 'admin';

SELECT '=== admin 用户角色 ===' AS section;
SELECT u.username, r.name as role_name, r.display_name as role_display_name
FROM user_roles ur
JOIN users u ON ur.user_id = u.id
JOIN roles r ON ur.role_id = r.id
WHERE u.username = 'admin';
