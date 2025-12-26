-- =============================================
-- User Session Management - Database Migration Script
-- Version: V4_add_user_sessions.sql
-- Date: 2025-12-16
-- =============================================

-- Create user sessions table
CREATE TABLE IF NOT EXISTS `user_sessions` (
    `id` CHAR(36) NOT NULL COMMENT 'Session ID',
    `user_id` CHAR(36) NOT NULL COMMENT 'User ID',
    `token_id` CHAR(36) NOT NULL COMMENT 'Associated token ID',
    `device_id` VARCHAR(64) NOT NULL COMMENT 'Device identifier',
    `device_type` VARCHAR(32) NOT NULL DEFAULT 'web' COMMENT 'Device type (web, mobile, desktop)',
    `device_name` VARCHAR(256) NULL COMMENT 'Device name/user agent',
    `ip_address` VARCHAR(45) NULL COMMENT 'IP address (supports IPv6)',
    `login_time` DATETIME NOT NULL COMMENT 'Login time',
    `last_active_time` DATETIME NOT NULL COMMENT 'Last active time',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_user_sessions_user_id` (`user_id`),
    INDEX `idx_user_sessions_device_id` (`user_id`, `device_id`),
    INDEX `idx_user_sessions_token_id` (`token_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='User sessions table';

-- =============================================
-- Login Policy Configuration - Add to system configs table
-- =============================================

INSERT INTO `system_configs` (`id`, `name`, `key`, `value`, `description`, `non_editable`, `timestamp`, `created_at`) VALUES
(UUID(), 'Security', 'AllowMultipleDevices', 'true', 'Whether to allow multiple devices to login simultaneously', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'MaxDevices', '5', 'Maximum allowed devices (0 means unlimited)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'NewDevicePolicy', 'allow', 'New device login policy (allow/deny/kick_oldest)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'RefreshTokenLifetimeDays', '30', 'Refresh token lifetime (days)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'AccessTokenLifetimeMinutes', '60', 'Access token lifetime (minutes)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW())
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);

-- =============================================
-- Rollback script (execute manually if rollback is needed)
-- =============================================
-- DROP TABLE IF EXISTS `user_sessions`;
-- DELETE FROM `system_configs` WHERE `key` IN ('AllowMultipleDevices', 'MaxDevices', 'NewDevicePolicy', 'RefreshTokenLifetimeDays', 'AccessTokenLifetimeMinutes');
