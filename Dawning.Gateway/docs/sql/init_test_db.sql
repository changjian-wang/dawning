-- ====================================================
-- Dawning Identity Test Database Initialization
-- Date: 2025-12-08
-- ====================================================

USE `dawning_identity_test`;

-- 1. 应用程序表 (Applications)
CREATE TABLE IF NOT EXISTS `openiddict_applications` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `client_id` VARCHAR(100) NOT NULL COMMENT '客户端ID',
    `client_secret` VARCHAR(500) NULL COMMENT '客户端密钥（哈希后）',
    `display_name` VARCHAR(200) NULL COMMENT '显示名称',
    `type` VARCHAR(50) NULL COMMENT '客户端类型',
    `consent_type` VARCHAR(50) NULL COMMENT '同意类型',
    `permissions` TEXT NULL COMMENT '权限列表（JSON）',
    `redirect_uris` TEXT NULL COMMENT '重定向URI（JSON）',
    `post_logout_redirect_uris` TEXT NULL COMMENT '登出URI（JSON）',
    `requirements` TEXT NULL COMMENT '要求列表（JSON）',
    `properties` TEXT NULL COMMENT '扩展属性（JSON）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    `updated_at` DATETIME NULL COMMENT '更新时间（UTC）',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_client_id` (`client_id`),
    INDEX `idx_timestamp` (`timestamp`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. 作用域表 (Scopes)
CREATE TABLE IF NOT EXISTS `openiddict_scopes` (
    `id` CHAR(36) NOT NULL COMMENT '主键',
    `name` VARCHAR(200) NOT NULL COMMENT '名称',
    `display_name` VARCHAR(200) NULL COMMENT '显示名称',
    `description` VARCHAR(500) NULL COMMENT '描述',
    `resources` TEXT NULL COMMENT '资源列表（JSON）',
    `properties` TEXT NULL COMMENT '扩展属性（JSON）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`),
    INDEX `idx_timestamp` (`timestamp`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. 授权表 (Authorizations)
CREATE TABLE IF NOT EXISTS `openiddict_authorizations` (
    `id` CHAR(36) NOT NULL COMMENT '主键',
    `application_id` CHAR(36) NULL COMMENT '应用ID',
    `subject` VARCHAR(200) NULL COMMENT '用户标识',
    `type` VARCHAR(50) NULL COMMENT '授权类型',
    `status` VARCHAR(50) NULL COMMENT '状态',
    `scopes` TEXT NULL COMMENT '作用域（JSON）',
    `properties` TEXT NULL COMMENT '扩展属性（JSON）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    PRIMARY KEY (`id`),
    INDEX `idx_application_id` (`application_id`),
    INDEX `idx_subject` (`subject`),
    INDEX `idx_timestamp` (`timestamp`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. 令牌表 (Tokens)
CREATE TABLE IF NOT EXISTS `openiddict_tokens` (
    `id` CHAR(36) NOT NULL COMMENT '主键',
    `application_id` CHAR(36) NULL COMMENT '应用ID',
    `authorization_id` CHAR(36) NULL COMMENT '授权ID',
    `subject` VARCHAR(200) NULL COMMENT '用户标识',
    `type` VARCHAR(50) NULL COMMENT '令牌类型',
    `status` VARCHAR(50) NULL COMMENT '状态',
    `payload` TEXT NULL COMMENT '负载（JWT）',
    `reference_id` VARCHAR(200) NULL COMMENT '引用ID',
    `expires_at` DATETIME NULL COMMENT '过期时间（UTC）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    PRIMARY KEY (`id`),
    INDEX `idx_application_id` (`application_id`),
    INDEX `idx_authorization_id` (`authorization_id`),
    INDEX `idx_reference_id` (`reference_id`),
    INDEX `idx_timestamp` (`timestamp`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT 'Test database initialized successfully' AS status;
