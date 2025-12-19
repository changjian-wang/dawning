-- 011_create_api_identity_resources.sql
-- Purpose: Create API Resources and Identity Resources tables for OAuth2/OpenID Connect
-- Author: System
-- Date: 2025-12-10

USE dawning_identity;

-- ==================== API Resources Table ====================
-- API Resources represent protected APIs that can be accessed using tokens
CREATE TABLE IF NOT EXISTS `api_resources` (
    `id` CHAR(36) NOT NULL COMMENT 'API资源唯一标识',
    `name` VARCHAR(200) NOT NULL COMMENT 'API资源名称(唯一标识符)',
    `display_name` VARCHAR(200) NULL COMMENT '显示名称',
    `description` VARCHAR(500) NULL COMMENT '描述信息',
    `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `allowed_access_token_signing_algorithms` TEXT NULL COMMENT '允许的访问令牌签名算法(JSON数组)',
    `show_in_discovery_document` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否在发现文档中显示',
    `properties` TEXT NULL COMMENT '扩展属性(JSON对象)',
    `timestamp` BIGINT NOT NULL COMMENT '时间戳',
    `created_at` DATETIME NOT NULL COMMENT '创建时间',
    `updated_at` DATETIME NULL COMMENT '更新时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_api_resources_name` (`name`),
    KEY `idx_api_resources_enabled` (`enabled`),
    KEY `idx_api_resources_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='API资源表';

-- ==================== API Resource Scopes Table ====================
-- Associates scopes with API resources
CREATE TABLE IF NOT EXISTS `api_resource_scopes` (
    `id` CHAR(36) NOT NULL COMMENT '唯一标识',
    `api_resource_id` CHAR(36) NOT NULL COMMENT 'API资源ID',
    `scope` VARCHAR(200) NOT NULL COMMENT 'Scope名称',
    `created_at` DATETIME NOT NULL COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_api_resource_scopes` (`api_resource_id`, `scope`),
    KEY `idx_api_resource_scopes_resource_id` (`api_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='API资源作用域关联表';

-- ==================== API Resource Claims Table ====================
-- Defines claims that should be included in access tokens for the API
CREATE TABLE IF NOT EXISTS `api_resource_claims` (
    `id` CHAR(36) NOT NULL COMMENT '唯一标识',
    `api_resource_id` CHAR(36) NOT NULL COMMENT 'API资源ID',
    `type` VARCHAR(200) NOT NULL COMMENT 'Claim类型',
    `created_at` DATETIME NOT NULL COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_api_resource_claims` (`api_resource_id`, `type`),
    KEY `idx_api_resource_claims_resource_id` (`api_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='API资源声明表';

-- ==================== Identity Resources Table ====================
-- Identity Resources represent user identity information (OpenID Connect)
CREATE TABLE IF NOT EXISTS `identity_resources` (
    `id` CHAR(36) NOT NULL COMMENT '身份资源唯一标识',
    `name` VARCHAR(200) NOT NULL COMMENT '身份资源名称(唯一标识符)',
    `display_name` VARCHAR(200) NULL COMMENT '显示名称',
    `description` VARCHAR(500) NULL COMMENT '描述信息',
    `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `required` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '用户是否必须同意',
    `emphasize` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否在同意界面中强调',
    `show_in_discovery_document` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否在发现文档中显示',
    `properties` TEXT NULL COMMENT '扩展属性(JSON对象)',
    `timestamp` BIGINT NOT NULL COMMENT '时间戳',
    `created_at` DATETIME NOT NULL COMMENT '创建时间',
    `updated_at` DATETIME NULL COMMENT '更新时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_identity_resources_name` (`name`),
    KEY `idx_identity_resources_enabled` (`enabled`),
    KEY `idx_identity_resources_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='身份资源表';

-- ==================== Identity Resource Claims Table ====================
-- Defines claims that are associated with an identity resource
CREATE TABLE IF NOT EXISTS `identity_resource_claims` (
    `id` CHAR(36) NOT NULL COMMENT '唯一标识',
    `identity_resource_id` CHAR(36) NOT NULL COMMENT '身份资源ID',
    `type` VARCHAR(200) NOT NULL COMMENT 'Claim类型',
    `created_at` DATETIME NOT NULL COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_identity_resource_claims` (`identity_resource_id`, `type`),
    KEY `idx_identity_resource_claims_resource_id` (`identity_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='身份资源声明表';

-- ==================== Verification ====================
-- Show created tables
SHOW TABLES LIKE '%resource%';

-- Show table structures
DESCRIBE api_resources;
DESCRIBE api_resource_scopes;
DESCRIBE api_resource_claims;
DESCRIBE identity_resources;
DESCRIBE identity_resource_claims;
