-- =============================================
-- 用户会话管理 - 数据库迁移脚本
-- 版本: V4_add_user_sessions.sql
-- 日期: 2025-12-16
-- =============================================

-- 创建用户会话表
CREATE TABLE IF NOT EXISTS `user_sessions` (
    `id` CHAR(36) NOT NULL COMMENT '会话ID',
    `user_id` CHAR(36) NOT NULL COMMENT '用户ID',
    `token_id` CHAR(36) NOT NULL COMMENT '关联的令牌ID',
    `device_id` VARCHAR(64) NOT NULL COMMENT '设备标识',
    `device_type` VARCHAR(32) NOT NULL DEFAULT 'web' COMMENT '设备类型(web, mobile, desktop)',
    `device_name` VARCHAR(256) NULL COMMENT '设备名称/用户代理',
    `ip_address` VARCHAR(45) NULL COMMENT 'IP地址(支持IPv6)',
    `login_time` DATETIME NOT NULL COMMENT '登录时间',
    `last_active_time` DATETIME NOT NULL COMMENT '最后活跃时间',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    INDEX `idx_user_sessions_user_id` (`user_id`),
    INDEX `idx_user_sessions_device_id` (`user_id`, `device_id`),
    INDEX `idx_user_sessions_token_id` (`token_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='用户会话表';

-- =============================================
-- 登录策略配置 - 添加到系统元数据表
-- =============================================

INSERT INTO `system_configs` (`id`, `name`, `key`, `value`, `description`, `non_editable`, `timestamp`, `created_at`) VALUES
(UUID(), 'Security', 'AllowMultipleDevices', 'true', '是否允许多设备同时登录', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'MaxDevices', '5', '最大允许设备数(0表示不限制)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'NewDevicePolicy', 'allow', '新设备登录策略(allow/deny/kick_oldest)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'RefreshTokenLifetimeDays', '30', '刷新令牌有效期(天)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Security', 'AccessTokenLifetimeMinutes', '60', '访问令牌有效期(分钟)', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW())
ON DUPLICATE KEY UPDATE `value` = VALUES(`value`);

-- =============================================
-- 回滚脚本（如需回滚请手动执行）
-- =============================================
-- DROP TABLE IF EXISTS `user_sessions`;
-- DELETE FROM `system_configs` WHERE `key` IN ('AllowMultipleDevices', 'MaxDevices', 'NewDevicePolicy', 'RefreshTokenLifetimeDays', 'AccessTokenLifetimeMinutes');
