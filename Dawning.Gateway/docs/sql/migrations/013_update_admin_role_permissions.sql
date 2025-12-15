-- 013_update_admin_role_permissions.sql
-- 更新 admin 角色权限，增加角色和权限管理能力

USE dawning_identity;

-- 更新 admin 角色的权限
UPDATE `roles` 
SET 
    `description` = '系统管理员角色，可以管理用户、角色、权限、应用等',
    `permissions` = JSON_ARRAY(
        'user:*:*',
        'role:*:*',
        'permission:*:*',
        'application:*:*',
        'scope:*:*',
        'claim-type:*:*',
        'system-metadata:*:*',
        'audit-log:read:*'
    ),
    `updated_at` = NOW()
WHERE `name` = 'admin';

-- 显示更新结果
SELECT '已更新 admin 角色权限' AS message;
SELECT name, display_name, description, permissions FROM roles WHERE name = 'admin';
