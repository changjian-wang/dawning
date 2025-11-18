-- ====================================================
-- Dawning Identity Database Creation Script
-- Date: 2025-11-18
-- Description: 创建 Dawning Identity 数据库
-- ====================================================

-- 创建数据库
CREATE DATABASE IF NOT EXISTS `dawning_identity` 
DEFAULT CHARACTER SET utf8mb4 
COLLATE utf8mb4_general_ci

-- 使用数据库
USE `dawning_identity`;

-- 查看数据库信息
SELECT 
    SCHEMA_NAME AS '数据库名',
    DEFAULT_CHARACTER_SET_NAME AS '字符集',
    DEFAULT_COLLATION_NAME AS '排序规则'
FROM information_schema.SCHEMATA 
WHERE SCHEMA_NAME = 'dawning_identity';

-- ====================================================
-- OpenIddict Tables for MySQL 8.0
-- Date: 2025-11-18
-- Description: OpenIddict 核心表结构
--   - timestamp: BIGINT 毫秒级Unix时间戳（UTC），用于索引和分页
--   - created_at: DATETIME 存储UTC时间，由应用层设置
-- ====================================================

-- 1. 应用程序表 (Applications)
DROP TABLE IF EXISTS `openiddict_applications`;

CREATE TABLE `openiddict_applications` (
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
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='OpenIddict应用程序表';

-- 2. 作用域表 (Scopes)
DROP TABLE IF EXISTS `openiddict_scopes`;

CREATE TABLE `openiddict_scopes` (
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
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='OpenIddict作用域表';

-- 3. 授权表 (Authorizations)
DROP TABLE IF EXISTS `openiddict_authorizations`;

CREATE TABLE `openiddict_authorizations` (
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
    INDEX `idx_created_at` (`created_at`),
    CONSTRAINT `fk_authorization_application` 
        FOREIGN KEY (`application_id`) 
        REFERENCES `openiddict_applications` (`id`) 
        ON DELETE CASCADE
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='OpenIddict授权表';

-- 4. 令牌表 (Tokens)
DROP TABLE IF EXISTS `openiddict_tokens`;

CREATE TABLE `openiddict_tokens` (
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
    INDEX `idx_created_at` (`created_at`),
    CONSTRAINT `fk_token_application` 
        FOREIGN KEY (`application_id`) 
        REFERENCES `openiddict_applications` (`id`) 
        ON DELETE CASCADE,
    CONSTRAINT `fk_token_authorization` 
        FOREIGN KEY (`authorization_id`) 
        REFERENCES `openiddict_authorizations` (`id`) 
        ON DELETE CASCADE
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='OpenIddict令牌表';

-- ====================================================
-- 插入初始测试数据
-- ====================================================

-- 插入测试应用程序
INSERT INTO `openiddict_applications` (
    `id`, `client_id`, `client_secret`, `display_name`, `type`, `consent_type`,
    `permissions`, `redirect_uris`, `post_logout_redirect_uris`, 
    `requirements`, `properties`, `timestamp`, `created_at`
) VALUES (
    UUID(),
    'test-client',
    'test-secret-hash',
    'Test Application',
    'confidential',
    'explicit',
    '["ept:token", "ept:authorization"]',
    '["https://localhost:5001/callback"]',
    '["https://localhost:5001/signout-callback"]',
    '[]',
    '{}',
    UNIX_TIMESTAMP() * 1000,
    UTC_TIMESTAMP()  -- 使用 UTC_TIMESTAMP() 而不是 NOW()
);

-- 插入测试作用域
INSERT INTO `openiddict_scopes` (
    `id`, `name`, `display_name`, `description`, 
    `resources`, `properties`, `timestamp`, `created_at`
) VALUES 
(UUID(), 'openid', 'OpenID', 'OpenID Connect scope', '[]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()),
(UUID(), 'profile', 'Profile', 'User profile information', '[]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()),
(UUID(), 'email', 'Email', 'User email address', '[]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP()),
(UUID(), 'api', 'API', 'API access scope', '["api-resource"]', '{}', UNIX_TIMESTAMP() * 1000, UTC_TIMESTAMP());

-- ====================================================
-- 清理过期令牌的存储过程
-- ====================================================

DELIMITER //

DROP PROCEDURE IF EXISTS `sp_prune_expired_tokens`//

CREATE PROCEDURE `sp_prune_expired_tokens`()
BEGIN
    DELETE FROM `openiddict_tokens`
    WHERE `expires_at` IS NOT NULL 
      AND `expires_at` < UTC_TIMESTAMP()  -- 使用 UTC_TIMESTAMP()
      AND `status` != 'valid';
      
    SELECT ROW_COUNT() AS deleted_count;
END//

DELIMITER ;

-- ====================================================
-- 定时清理任务
-- ====================================================

DROP EVENT IF EXISTS `evt_cleanup_expired_tokens`;

CREATE EVENT `evt_cleanup_expired_tokens`
ON SCHEDULE EVERY 1 DAY
STARTS (TIMESTAMP(CURRENT_DATE) + INTERVAL 1 DAY + INTERVAL 2 HOUR)
DO
    CALL sp_prune_expired_tokens();

-- ====================================================
-- 查询验证示例
-- ====================================================

-- 查看当前 UTC 时间
-- SELECT UTC_TIMESTAMP();

-- 查看数据
-- SELECT 
--     `id`, 
--     `client_id`, 
--     `timestamp`,
--     `created_at`,
--     FROM_UNIXTIME(`timestamp` / 1000) as ts_converted
-- FROM `openiddict_applications`;

-- 分页查询
-- SELECT * FROM `openiddict_applications` 
-- ORDER BY `timestamp` DESC 
-- LIMIT 10 OFFSET 0;