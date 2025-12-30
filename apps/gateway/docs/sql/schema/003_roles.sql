-- 005_create_roles_table.sql
-- Create roles table and initial data

USE dawning_identity;

-- Create roles table
CREATE TABLE IF NOT EXISTS `roles` (
  `id` CHAR(36) NOT NULL COMMENT 'Role ID (GUID)',
  `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID',
  `name` VARCHAR(50) NOT NULL COMMENT 'Role name (unique identifier, e.g., admin, user, manager)',
  `display_name` VARCHAR(100) NOT NULL COMMENT 'Role display name',
  `description` VARCHAR(500) DEFAULT NULL COMMENT 'Role description',
  `is_system` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Is system role (system roles cannot be deleted)',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Is active',
  `permissions` JSON DEFAULT NULL COMMENT 'Role permissions list (JSON array)',
  `timestamp` BIGINT NOT NULL DEFAULT 0 COMMENT 'Timestamp (millisecond Unix timestamp)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Creator ID',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Updated time',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT 'Updater ID',
  `deleted_at` DATETIME DEFAULT NULL COMMENT 'Soft delete time',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_roles_name` (`name`),
  KEY `idx_roles_is_active` (`is_active`),
  KEY `idx_roles_timestamp` (`timestamp`),
  KEY `idx_roles_deleted_at` (`deleted_at`),
  KEY `idx_roles_tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Roles table';

-- Insert system predefined roles
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`) VALUES
(UUID(), 'super_admin', 'Super Administrator', 'System highest privilege role, has all permissions', 1, 1, 
 JSON_ARRAY('*:*:*'), NOW()),
 
(UUID(), 'admin', 'System Administrator', 'System administrator role, can manage users, roles, applications, etc.', 1, 1, 
 JSON_ARRAY(
   'user:*:*',
   'role:read:*',
   'application:*:*',
   'scope:*:*',
   'claim-type:*:*',
   'system-config:*:*'
 ), NOW()),
 
(UUID(), 'user_manager', 'User Manager', 'Responsible for user account management', 1, 1, 
 JSON_ARRAY(
   'user:read:*',
   'user:create:*',
   'user:update:*',
   'user:reset-password:*'
 ), NOW()),
 
(UUID(), 'auditor', 'Auditor', 'Read-only access, can view all data but cannot modify', 1, 1, 
 JSON_ARRAY(
   'user:read:*',
   'role:read:*',
   'application:read:*',
   'scope:read:*',
   'audit-log:read:*'
 ), NOW()),
 
(UUID(), 'user', 'User', 'Regular user role, can only manage own information', 1, 1, 
 JSON_ARRAY(
   'user:read:own',
   'user:update:own'
 ), NOW());

-- Display creation results
SELECT 'Roles table created successfully!' AS message;
SELECT id, name, display_name, description, is_system, is_active FROM roles;
