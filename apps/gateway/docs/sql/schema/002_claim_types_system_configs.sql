-- ====================================================
-- Create claim_types and system_configs Tables
-- Date: 2025-12-09 (Updated: 2025-12-19)
-- Description: Create claim types and system config tables
-- ====================================================

USE `dawning_identity`;

-- ====================================================
-- 1. Claim Types Table
-- ====================================================
DROP TABLE IF EXISTS `claim_types`;

CREATE TABLE `claim_types` (
    `id` CHAR(36) NOT NULL COMMENT 'Primary key (GUID)',
    `name` VARCHAR(200) NOT NULL COMMENT 'Name',
    `display_name` VARCHAR(200) NOT NULL COMMENT 'Display name',
    `type` VARCHAR(50) NOT NULL COMMENT 'Type (String, Int, DateTime, Boolean, Enum)',
    `description` VARCHAR(500) NULL COMMENT 'Description',
    `required` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Is required',
    `non_editable` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'User editable (0=editable, 1=non-editable)',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix timestamp in milliseconds (UTC, for indexing and pagination)',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time (UTC)',
    `updated` DATETIME NULL COMMENT 'Updated time (UTC)',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`),
    INDEX `idx_type` (`type`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created` (`created`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Claim types table';

-- ====================================================
-- 2. System Config Table
-- ====================================================
DROP TABLE IF EXISTS `system_configs`;

CREATE TABLE `system_configs` (
    `id` CHAR(36) NOT NULL COMMENT 'Primary key (GUID)',
    `name` VARCHAR(200) NOT NULL COMMENT 'Config group name',
    `key` VARCHAR(200) NOT NULL COMMENT 'Config key',
    `value` TEXT NULL COMMENT 'Config value',
    `description` VARCHAR(500) NULL COMMENT 'Description',
    `non_editable` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Is read-only (0=editable, 1=non-editable)',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix timestamp in milliseconds (UTC, for indexing and pagination)',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time (UTC)',
    `updated_at` DATETIME NULL COMMENT 'Updated time (UTC)',
    PRIMARY KEY (`id`),
    INDEX `idx_name` (`name`),
    INDEX `idx_key` (`key`),
    INDEX `idx_name_key` (`name`, `key`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='System config table';

-- ====================================================
-- 3. Insert Initial Data (Optional)
-- ====================================================

-- Insert basic claim types
INSERT INTO `claim_types` (`id`, `name`, `display_name`, `type`, `description`, `required`, `non_editable`, `timestamp`, `created`) VALUES
(UUID(), 'sub', 'Subject', 'String', 'User unique identifier', 1, 1, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'name', 'Name', 'String', 'User name', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'email', 'Email', 'String', 'Email address', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'email_verified', 'Email Verified', 'Boolean', 'Email confirmed', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'phone_number', 'Phone Number', 'String', 'Phone number', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'phone_number_verified', 'Phone Number Verified', 'Boolean', 'Phone number confirmed', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'role', 'Role', 'String', 'User role', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'birthdate', 'Birthdate', 'DateTime', 'Date of birth', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'gender', 'Gender', 'Enum', 'Gender', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'locale', 'Locale', 'String', 'Language locale', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW());

-- Insert system config examples
INSERT INTO `system_configs` (`id`, `name`, `key`, `value`, `description`, `non_editable`, `timestamp`, `created_at`) VALUES
(UUID(), 'System', 'Version', '1.0.0', 'System version number', 1, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'System', 'Environment', 'Development', 'System runtime environment', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Client', 'DefaultRedirectUri', 'http://localhost:5173/callback', 'Default redirect URI', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Client', 'DefaultLogoutUri', 'http://localhost:5173/', 'Default logout URI', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Token', 'AccessTokenLifetime', '3600', 'Access token lifetime (seconds)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Token', 'RefreshTokenLifetime', '86400', 'Refresh token lifetime (seconds)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW());

-- ====================================================
-- 4. Verify Table Creation
-- ====================================================
SELECT 
    TABLE_NAME AS 'Table Name',
    TABLE_ROWS AS 'Row Count',
    TABLE_COMMENT AS 'Comment'
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'dawning_identity' 
AND TABLE_NAME IN ('claim_types', 'system_configs');
