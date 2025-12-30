-- ============================================
-- V8: Multi-tenancy Support
-- Created: 2025-12-23
-- Description: Add multi-tenancy support with tenants table
-- Note: tenant_id columns have been added directly to each table's CREATE statement
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

-- 2. Create default tenant (primary tenant for localhost)
INSERT INTO `tenants` (`id`, `code`, `name`, `description`, `domain`, `is_active`, `plan`, `timestamp`, `created_at`)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'default',
    'Default Tenant',
    'System default tenant for host administrators and public data',
    'localhost',
    1,
    'enterprise',
    UNIX_TIMESTAMP() * 1000,
    NOW()
);

-- 3. Add tenant-related permissions
INSERT INTO `permissions` (`id`, `code`, `name`, `description`, `resource`, `action`, `category`, `is_system`, `display_order`)
VALUES 
    (UUID(), 'tenant:create', 'Create Tenant', 'Allows creating new tenants', 'tenant', 'create', 'multitenancy', 1, 700),
    (UUID(), 'tenant:read', 'View Tenant', 'Allows viewing tenant list and details', 'tenant', 'read', 'multitenancy', 1, 701),
    (UUID(), 'tenant:update', 'Update Tenant', 'Allows updating tenant information', 'tenant', 'update', 'multitenancy', 1, 702),
    (UUID(), 'tenant:delete', 'Delete Tenant', 'Allows deleting tenants', 'tenant', 'delete', 'multitenancy', 1, 703),
    (UUID(), 'tenant:switch', 'Switch Tenant', 'Allows switching between tenants', 'tenant', 'switch', 'multitenancy', 1, 704);

-- ============================================
-- Rollback script (execute these statements if rollback needed)
-- ============================================
-- DELETE FROM `permissions` WHERE `category` = 'multitenancy';
-- DROP TABLE IF EXISTS `tenants`;
