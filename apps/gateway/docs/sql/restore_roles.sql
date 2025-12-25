-- 恢复系统角色数据
-- 如果角色被意外删除，运行此脚本恢复

USE dawning_identity;

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

-- 显示恢复结果
SELECT '角色数据恢复完成！' AS message;
SELECT id, name, display_name, is_system, is_active FROM roles ORDER BY name;
