-- ====================================================
-- Create claim_types and system_metadatas Tables
-- Date: 2025-12-09
-- Description: 创建声明类型和系统元数据表
-- ====================================================

USE `dawning_identity`;

-- ====================================================
-- 1. 声明类型表 (Claim Types)
-- ====================================================
DROP TABLE IF EXISTS `claim_types`;

CREATE TABLE `claim_types` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `name` VARCHAR(200) NOT NULL COMMENT '名称',
    `display_name` VARCHAR(200) NOT NULL COMMENT '显示名称',
    `type` VARCHAR(50) NOT NULL COMMENT '类型（String, Int, DateTime, Boolean, Enum）',
    `description` VARCHAR(500) NULL COMMENT '描述说明',
    `required` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否必须项',
    `non_editable` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '用户是否可编辑（0=可编辑，1=不可编辑）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
    `created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间（UTC）',
    `updated` DATETIME NULL COMMENT '更新时间（UTC）',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`),
    INDEX `idx_type` (`type`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created` (`created`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='声明类型表';

-- ====================================================
-- 2. 系统元数据表 (System Metadata)
-- ====================================================
DROP TABLE IF EXISTS `system_metadatas`;

CREATE TABLE `system_metadatas` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `name` VARCHAR(200) NOT NULL COMMENT '类型名称（Client，IdentityResource，ApiResource，ApiScope等）',
    `key` VARCHAR(200) NOT NULL COMMENT '键',
    `value` TEXT NULL COMMENT '值',
    `description` VARCHAR(500) NULL COMMENT '描述说明',
    `non_editable` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '用户是否可编辑（0=可编辑，1=不可编辑）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间（UTC）',
    `updated_at` DATETIME NULL COMMENT '更新时间（UTC）',
    PRIMARY KEY (`id`),
    INDEX `idx_name` (`name`),
    INDEX `idx_key` (`key`),
    INDEX `idx_name_key` (`name`, `key`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='系统元数据表';

-- ====================================================
-- 3. 插入初始数据 (Optional)
-- ====================================================

-- 插入基本的声明类型
INSERT INTO `claim_types` (`id`, `name`, `display_name`, `type`, `description`, `required`, `non_editable`, `timestamp`, `created`) VALUES
(UUID(), 'sub', 'Subject', 'String', '用户唯一标识符', 1, 1, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'name', 'Name', 'String', '用户姓名', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'email', 'Email', 'String', '电子邮箱', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'email_verified', 'Email Verified', 'Boolean', '邮箱是否已验证', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'phone_number', 'Phone Number', 'String', '手机号码', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'phone_number_verified', 'Phone Number Verified', 'Boolean', '手机号码是否已验证', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'role', 'Role', 'String', '用户角色', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'birthdate', 'Birthdate', 'DateTime', '出生日期', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'gender', 'Gender', 'Enum', '性别', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'locale', 'Locale', 'String', '语言区域', 0, 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW());

-- 插入系统元数据示例
INSERT INTO `system_metadatas` (`id`, `name`, `key`, `value`, `description`, `non_editable`, `timestamp`, `created_at`) VALUES
(UUID(), 'System', 'Version', '1.0.0', '系统版本号', 1, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'System', 'Environment', 'Development', '系统运行环境', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Client', 'DefaultRedirectUri', 'http://localhost:5173/callback', '默认重定向URI', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Client', 'DefaultLogoutUri', 'http://localhost:5173/', '默认登出URI', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Token', 'AccessTokenLifetime', '3600', '访问令牌生命周期（秒）', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW()),
(UUID(), 'Token', 'RefreshTokenLifetime', '86400', '刷新令牌生命周期（秒）', 0, UNIX_TIMESTAMP(NOW(3)) * 1000, NOW());

-- ====================================================
-- 4. 验证表创建
-- ====================================================
SELECT 
    TABLE_NAME AS '表名',
    TABLE_ROWS AS '行数',
    TABLE_COMMENT AS '注释'
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'dawning_identity' 
AND TABLE_NAME IN ('claim_types', 'system_metadatas');
