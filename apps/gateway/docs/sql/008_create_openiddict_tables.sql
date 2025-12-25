-- ====================================================
-- OpenIddict Tables for MySQL 8.0
-- Date: 2025-11-18
-- Description: OpenIddict 核心表结构
--   - timestamp: BIGINT 毫秒级Unix时间戳（UTC），用于索引和分页
--   - created_at: DATETIME 存储UTC时间，由应用层设置
-- ====================================================

-- 1. 应用程序表 (Applications)
CREATE TABLE IF NOT EXISTS `openiddict_applications` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `client_id` VARCHAR(100) NOT NULL COMMENT '客户端ID',
    `client_secret` VARCHAR(500) NULL COMMENT '客户端密钥（哈希后）',
    `display_name` VARCHAR(200) NULL COMMENT '显示名称',
    `type` VARCHAR(50) NULL COMMENT '客户端类型（confidential, public）',
    `consent_type` VARCHAR(50) NULL COMMENT '同意类型（explicit, implicit, systematic）',
    `permissions` TEXT NULL COMMENT '权限列表（JSON格式）',
    `redirect_uris` TEXT NULL COMMENT '重定向URI列表（JSON格式）',
    `post_logout_redirect_uris` TEXT NULL COMMENT '登出后重定向URI列表（JSON格式）',
    `requirements` TEXT NULL COMMENT '要求列表（JSON格式）',
    `properties` TEXT NULL COMMENT '扩展属性（JSON格式）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    `updated_at` DATETIME NULL COMMENT '更新时间（UTC）',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_client_id` (`client_id`),
    INDEX `idx_type` (`type`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict应用程序表';

-- 2. 作用域表 (Scopes)
CREATE TABLE IF NOT EXISTS `openiddict_scopes` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `name` VARCHAR(200) NOT NULL COMMENT '作用域名称',
    `display_name` VARCHAR(200) NULL COMMENT '显示名称',
    `description` VARCHAR(500) NULL COMMENT '描述',
    `resources` TEXT NULL COMMENT '资源列表（JSON格式）',
    `properties` TEXT NULL COMMENT '扩展属性（JSON格式）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict作用域表';

-- 3. 授权表 (Authorizations)
CREATE TABLE IF NOT EXISTS `openiddict_authorizations` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `application_id` CHAR(36) NULL COMMENT '关联的应用程序ID',
    `subject` VARCHAR(200) NULL COMMENT '用户标识',
    `type` VARCHAR(50) NULL COMMENT '授权类型',
    `status` VARCHAR(50) NULL COMMENT '授权状态（valid, revoked）',
    `scopes` TEXT NULL COMMENT '授权的作用域列表（JSON格式）',
    `properties` TEXT NULL COMMENT '扩展属性（JSON格式）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    PRIMARY KEY (`id`),
    INDEX `idx_application_id` (`application_id`),
    INDEX `idx_subject` (`subject`),
    INDEX `idx_status` (`status`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict授权表';

-- 4. 令牌表 (Tokens)
CREATE TABLE IF NOT EXISTS `openiddict_tokens` (
    `id` CHAR(36) NOT NULL COMMENT '主键 (GUID)',
    `application_id` CHAR(36) NULL COMMENT '关联的应用程序ID',
    `authorization_id` CHAR(36) NULL COMMENT '关联的授权ID',
    `subject` VARCHAR(200) NULL COMMENT '用户标识',
    `type` VARCHAR(50) NULL COMMENT '令牌类型（access_token, refresh_token, id_token）',
    `status` VARCHAR(50) NULL COMMENT '令牌状态（valid, revoked, redeemed）',
    `payload` TEXT NULL COMMENT '令牌负载（JWT）',
    `reference_id` VARCHAR(200) NULL COMMENT '引用ID（用于令牌内省）',
    `expires_at` DATETIME NULL COMMENT '过期时间（UTC）',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix时间戳-毫秒（UTC，用于索引和分页）',
    `created_at` DATETIME NOT NULL COMMENT '创建时间（UTC）',
    PRIMARY KEY (`id`),
    INDEX `idx_application_id` (`application_id`),
    INDEX `idx_authorization_id` (`authorization_id`),
    INDEX `idx_subject` (`subject`),
    INDEX `idx_reference_id` (`reference_id`),
    INDEX `idx_status` (`status`),
    INDEX `idx_expires_at` (`expires_at`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict令牌表';
