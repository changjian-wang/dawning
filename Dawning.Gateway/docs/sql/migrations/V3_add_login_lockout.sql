-- =============================================
-- 用户登录锁定功能 - 数据库迁移脚本
-- 版本: V3_add_login_lockout.sql
-- 日期: 2025-12-16
-- =============================================

-- 添加登录锁定相关字段到 users 表
ALTER TABLE `users`
ADD COLUMN `failed_login_count` INT NOT NULL DEFAULT 0 COMMENT '连续登录失败次数' AFTER `last_login_at`,
ADD COLUMN `lockout_end` DATETIME NULL COMMENT '锁定结束时间' AFTER `failed_login_count`,
ADD COLUMN `lockout_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用锁定功能' AFTER `lockout_end`;

-- 添加索引以优化锁定检查查询
CREATE INDEX `idx_users_lockout` ON `users` (`username`, `lockout_end`);

-- =============================================
-- 密码策略配置 - 添加到系统配置表
-- =============================================

-- 插入密码策略相关配置
INSERT INTO `system_configs` (`id`, `config_group`, `config_key`, `config_value`, `description`, `created_at`) VALUES
(UUID(), 'Security', 'PasswordMinLength', '8', '密码最小长度', NOW()),
(UUID(), 'Security', 'PasswordRequireUppercase', 'true', '密码是否要求包含大写字母', NOW()),
(UUID(), 'Security', 'PasswordRequireLowercase', 'true', '密码是否要求包含小写字母', NOW()),
(UUID(), 'Security', 'PasswordRequireDigit', 'true', '密码是否要求包含数字', NOW()),
(UUID(), 'Security', 'PasswordRequireSpecialChar', 'false', '密码是否要求包含特殊字符', NOW()),
(UUID(), 'Security', 'MaxFailedLoginAttempts', '5', '最大登录失败次数', NOW()),
(UUID(), 'Security', 'LockoutDurationMinutes', '15', '账户锁定时长(分钟)', NOW()),
(UUID(), 'Security', 'EnableLoginLockout', 'true', '是否启用登录锁定功能', NOW())
ON DUPLICATE KEY UPDATE `config_value` = VALUES(`config_value`);
