-- ============================================
-- V8: Multi-tenancy Support
-- Created: 2025-12-23
-- Description: Add multi-tenancy support, including tenant table and tenant_id field for existing tables
-- ============================================

-- 1. Create tenants table
CREATE TABLE IF NOT EXISTS `tenants` (
    `id` CHAR(36) NOT NULL COMMENT 'Tenant ID',
    `code` VARCHAR(50) NOT NULL COMMENT 'Tenant code (unique identifier)',
    `name` VARCHAR(100) NOT NULL COMMENT 'Tenant name',
    `description` VARCHAR(500) NULL COMMENT 'Tenant description',
    `domain` VARCHAR(255) NULL COMMENT 'Bound domain',
    `email` VARCHAR(255) NULL COMMENT 'Contact email',
    `phone` VARCHAR(50) NULL COMMENT 'Contact phone',
    `logo_url` VARCHAR(500) NULL COMMENT 'Tenant logo URL',
    `settings` JSON NULL COMMENT 'Tenant settings (JSON format)',
    `connection_string` VARCHAR(500) NULL COMMENT 'Separate database connection string',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Is active',
    `plan` VARCHAR(50) NOT NULL DEFAULT 'free' COMMENT 'Subscription plan',
    `subscription_expires_at` DATETIME NULL COMMENT 'Subscription expiration time',
    `max_users` INT NULL COMMENT 'Maximum users limit',
    `max_storage_mb` INT NULL COMMENT 'Maximum storage (MB)',
    `timestamp` BIGINT NOT NULL DEFAULT 0 COMMENT 'Timestamp (millisecond Unix timestamp)',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time',
    `updated_at` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP COMMENT 'Updated time',
    `created_by` CHAR(36) NULL COMMENT 'Creator ID',
    `updated_by` CHAR(36) NULL COMMENT 'Updater ID',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_tenants_code` (`code`),
    UNIQUE KEY `uk_tenants_domain` (`domain`),
    KEY `idx_tenants_is_active` (`is_active`),
    KEY `idx_tenants_plan` (`plan`),
    KEY `idx_tenants_timestamp` (`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tenants table';

-- 2. Add tenant_id field to existing tables (optional, enable as needed)
-- Note: Following operations may take a long time, please execute during maintenance window

-- 2.1 Users table
ALTER TABLE `users` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_users_tenant_id` (`tenant_id`);

-- 2.2 Roles table
ALTER TABLE `roles` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_roles_tenant_id` (`tenant_id`);

-- 2.3 Permissions table
ALTER TABLE `permissions` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_permissions_tenant_id` (`tenant_id`);

-- 2.4 System configs table
ALTER TABLE `system_configs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_system_configs_tenant_id` (`tenant_id`);

-- 2.5 Audit logs table
ALTER TABLE `audit_logs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_audit_logs_tenant_id` (`tenant_id`);

-- 2.6 System logs table
ALTER TABLE `system_logs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_system_logs_tenant_id` (`tenant_id`);

-- 2.7 Alert rules table
ALTER TABLE `alert_rules` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_alert_rules_tenant_id` (`tenant_id`);

-- 2.8 Alert history table
ALTER TABLE `alert_history` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_alert_history_tenant_id` (`tenant_id`);

-- 2.9 Request logs table
ALTER TABLE `request_logs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_request_logs_tenant_id` (`tenant_id`);

-- 2.10 Gateway routes table
ALTER TABLE `gateway_routes` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_gateway_routes_tenant_id` (`tenant_id`);

-- 2.11 Gateway clusters table
ALTER TABLE `gateway_clusters` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_gateway_clusters_tenant_id` (`tenant_id`);

-- 2.12 Rate limit policies table
ALTER TABLE `rate_limit_policies` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_rate_limit_policies_tenant_id` (`tenant_id`);

-- 2.13 IP access rules table
ALTER TABLE `ip_access_rules` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT 'Tenant ID' AFTER `id`,
ADD INDEX `idx_ip_access_rules_tenant_id` (`tenant_id`);

-- 3. Create default tenant (primary tenant)
INSERT INTO `tenants` (`id`, `code`, `name`, `description`, `is_active`, `plan`, `created_at`)
VALUES (
    UUID(),
    'default',
    'Default Tenant',
    'System default tenant for host administrators and public data',
    1,
    'enterprise',
    NOW()
);

-- 4. Associate existing data to default tenant (optional)
-- Uncomment the following statements to migrate existing data to default tenant
-- SET @default_tenant_id = (SELECT id FROM tenants WHERE code = 'default');
-- UPDATE users SET tenant_id = @default_tenant_id WHERE tenant_id IS NULL;
-- UPDATE roles SET tenant_id = @default_tenant_id WHERE tenant_id IS NULL;
-- UPDATE permissions SET tenant_id = @default_tenant_id WHERE tenant_id IS NULL;
-- ... similar for other tables

-- 5. Add tenant-related permissions
INSERT INTO `permissions` (`id`, `code`, `name`, `description`, `resource`, `action`, `category`, `is_active`, `sort_order`)
VALUES 
    (UUID(), 'tenant:create', 'Create Tenant', 'Allows creating new tenants', 'tenant', 'create', 'multitenancy', 1, 700),
    (UUID(), 'tenant:read', 'View Tenant', 'Allows viewing tenant list and details', 'tenant', 'read', 'multitenancy', 1, 701),
    (UUID(), 'tenant:update', 'Update Tenant', 'Allows updating tenant information', 'tenant', 'update', 'multitenancy', 1, 702),
    (UUID(), 'tenant:delete', 'Delete Tenant', 'Allows deleting tenants', 'tenant', 'delete', 'multitenancy', 1, 703),
    (UUID(), 'tenant:switch', 'Switch Tenant', 'Allows switching between tenants', 'tenant', 'switch', 'multitenancy', 1, 704);

-- ============================================
-- Rollback script (execute these statements if rollback needed)
-- ============================================
-- DROP TABLE IF EXISTS `tenants`;
-- ALTER TABLE `users` DROP COLUMN `tenant_id`, DROP INDEX `idx_users_tenant_id`;
-- ALTER TABLE `roles` DROP COLUMN `tenant_id`, DROP INDEX `idx_roles_tenant_id`;
-- ALTER TABLE `permissions` DROP COLUMN `tenant_id`, DROP INDEX `idx_permissions_tenant_id`;
-- ALTER TABLE `system_configs` DROP COLUMN `tenant_id`, DROP INDEX `idx_system_configs_tenant_id`;
-- ALTER TABLE `audit_logs` DROP COLUMN `tenant_id`, DROP INDEX `idx_audit_logs_tenant_id`;
-- ALTER TABLE `system_logs` DROP COLUMN `tenant_id`, DROP INDEX `idx_system_logs_tenant_id`;
-- ALTER TABLE `alert_rules` DROP COLUMN `tenant_id`, DROP INDEX `idx_alert_rules_tenant_id`;
-- ALTER TABLE `alert_history` DROP COLUMN `tenant_id`, DROP INDEX `idx_alert_history_tenant_id`;
-- ALTER TABLE `request_logs` DROP COLUMN `tenant_id`, DROP INDEX `idx_request_logs_tenant_id`;
-- ALTER TABLE `gateway_routes` DROP COLUMN `tenant_id`, DROP INDEX `idx_gateway_routes_tenant_id`;
-- ALTER TABLE `gateway_clusters` DROP COLUMN `tenant_id`, DROP INDEX `idx_gateway_clusters_tenant_id`;
-- ALTER TABLE `rate_limit_policies` DROP COLUMN `tenant_id`, DROP INDEX `idx_rate_limit_policies_tenant_id`;
-- ALTER TABLE `ip_access_rules` DROP COLUMN `tenant_id`, DROP INDEX `idx_ip_access_rules_tenant_id`;
-- DELETE FROM `permissions` WHERE `category` = 'multitenancy';
