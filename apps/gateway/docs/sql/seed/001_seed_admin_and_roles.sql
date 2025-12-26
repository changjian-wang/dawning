-- ============================================
-- Seed Initial Admin User and System Roles
-- Execute after all schema scripts
-- ============================================

USE dawning_identity;

-- ============================================
-- 1. Seed System Roles
-- ============================================

-- Super Admin role
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `timestamp`, `created_at`)
SELECT UUID(), 'super_admin', 'Super Administrator', 'System highest privilege role with all permissions', 1, 1, 
  JSON_ARRAY('*:*:*'), UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'super_admin');

-- Admin role
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `timestamp`, `created_at`)
SELECT UUID(), 'admin', 'System Administrator', 'System administrator role, can manage users, roles, applications, etc.', 1, 1, 
  JSON_ARRAY('user:*:*', 'role:read:*', 'application:*:*', 'scope:*:*', 'claim-type:*:*', 'system-config:*:*'), 
  UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'admin');

-- User Manager role
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `timestamp`, `created_at`)
SELECT UUID(), 'user_manager', 'User Manager', 'Responsible for user account management', 1, 1, 
  JSON_ARRAY('user:read:*', 'user:create:*', 'user:update:*', 'user:reset-password:*'), 
  UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'user_manager');

-- Auditor role
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `timestamp`, `created_at`)
SELECT UUID(), 'auditor', 'Auditor', 'Read-only permission, can view all data but cannot modify', 1, 1, 
  JSON_ARRAY('user:read:*', 'role:read:*', 'application:read:*', 'scope:read:*', 'audit-log:read:*'), 
  UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'auditor');

-- User role
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `timestamp`, `created_at`)
SELECT UUID(), 'user', 'Regular User', 'Regular user with basic permissions', 1, 1, 
  JSON_ARRAY('profile:read:self', 'profile:update:self'), 
  UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `roles` WHERE `name` = 'user');

-- ============================================
-- 2. Assign Admin User to Super Admin Role
-- ============================================

-- Get admin user id and super_admin role id, then create association
INSERT INTO `user_roles` (`id`, `user_id`, `role_id`, `timestamp`, `created_at`)
SELECT UUID(), u.id, r.id, UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
FROM `users` u, `roles` r
WHERE u.username = 'admin' AND r.name = 'super_admin'
AND NOT EXISTS (
    SELECT 1 FROM `user_roles` ur 
    WHERE ur.user_id = u.id AND ur.role_id = r.id
);

-- ============================================
-- 3. Seed Default Tenant (if multitenancy enabled)
-- ============================================

INSERT INTO `tenants` (`id`, `code`, `name`, `description`, `is_active`, `plan`, `timestamp`, `created_at`)
SELECT UUID(), 'default', 'Default Tenant', 'System default tenant', 1, 'enterprise', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `tenants` WHERE `code` = 'default');

-- ============================================
-- Verification
-- ============================================

SELECT 'Seed data inserted successfully!' AS message;
SELECT name, display_name, is_system FROM roles;
SELECT username, display_name, is_system FROM users WHERE username = 'admin';
