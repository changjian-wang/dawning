-- 011_create_api_identity_resources.sql
-- Purpose: Create API Resources and Identity Resources tables for OAuth2/OpenID Connect
-- Author: System
-- Date: 2025-12-10

USE dawning_identity;

-- ==================== API Resources Table ====================
-- API Resources represent protected APIs that can be accessed using tokens
CREATE TABLE IF NOT EXISTS `api_resources` (
    `id` CHAR(36) NOT NULL COMMENT 'API resource unique identifier',
    `name` VARCHAR(200) NOT NULL COMMENT 'API resource name (unique identifier)',
    `display_name` VARCHAR(200) NULL COMMENT 'Display name',
    `description` VARCHAR(500) NULL COMMENT 'Description',
    `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Is enabled',
    `allowed_access_token_signing_algorithms` TEXT NULL COMMENT 'Allowed access token signing algorithms (JSON array)',
    `show_in_discovery_document` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Show in discovery document',
    `properties` TEXT NULL COMMENT 'Extended properties (JSON object)',
    `timestamp` BIGINT NOT NULL COMMENT 'Timestamp',
    `created_at` DATETIME NOT NULL COMMENT 'Created time',
    `updated_at` DATETIME NULL COMMENT 'Updated time',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_api_resources_name` (`name`),
    KEY `idx_api_resources_enabled` (`enabled`),
    KEY `idx_api_resources_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='API resources table';

-- ==================== API Resource Scopes Table ====================
-- Associates scopes with API resources
CREATE TABLE IF NOT EXISTS `api_resource_scopes` (
    `id` CHAR(36) NOT NULL COMMENT 'Unique identifier',
    `api_resource_id` CHAR(36) NOT NULL COMMENT 'API resource ID',
    `scope` VARCHAR(200) NOT NULL COMMENT 'Scope name',
    `created_at` DATETIME NOT NULL COMMENT 'Created time',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_api_resource_scopes` (`api_resource_id`, `scope`),
    KEY `idx_api_resource_scopes_resource_id` (`api_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='API resource scopes association table';

-- ==================== API Resource Claims Table ====================
-- Defines claims that should be included in access tokens for the API
CREATE TABLE IF NOT EXISTS `api_resource_claims` (
    `id` CHAR(36) NOT NULL COMMENT 'Unique identifier',
    `api_resource_id` CHAR(36) NOT NULL COMMENT 'API resource ID',
    `type` VARCHAR(200) NOT NULL COMMENT 'Claim type',
    `created_at` DATETIME NOT NULL COMMENT 'Created time',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_api_resource_claims` (`api_resource_id`, `type`),
    KEY `idx_api_resource_claims_resource_id` (`api_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='API resource claims table';

-- ==================== Identity Resources Table ====================
-- Identity Resources represent user identity information (OpenID Connect)
CREATE TABLE IF NOT EXISTS `identity_resources` (
    `id` CHAR(36) NOT NULL COMMENT 'Identity resource unique identifier',
    `name` VARCHAR(200) NOT NULL COMMENT 'Identity resource name (unique identifier)',
    `display_name` VARCHAR(200) NULL COMMENT 'Display name',
    `description` VARCHAR(500) NULL COMMENT 'Description',
    `enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Is enabled',
    `required` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'User must consent',
    `emphasize` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'Emphasize in consent screen',
    `show_in_discovery_document` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Show in discovery document',
    `properties` TEXT NULL COMMENT 'Extended properties (JSON object)',
    `timestamp` BIGINT NOT NULL COMMENT 'Timestamp',
    `created_at` DATETIME NOT NULL COMMENT 'Created time',
    `updated_at` DATETIME NULL COMMENT 'Updated time',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_identity_resources_name` (`name`),
    KEY `idx_identity_resources_enabled` (`enabled`),
    KEY `idx_identity_resources_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Identity resources table';

-- ==================== Identity Resource Claims Table ====================
-- Defines claims that are associated with an identity resource
CREATE TABLE IF NOT EXISTS `identity_resource_claims` (
    `id` CHAR(36) NOT NULL COMMENT 'Unique identifier',
    `identity_resource_id` CHAR(36) NOT NULL COMMENT 'Identity resource ID',
    `type` VARCHAR(200) NOT NULL COMMENT 'Claim type',
    `created_at` DATETIME NOT NULL COMMENT 'Created time',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_identity_resource_claims` (`identity_resource_id`, `type`),
    KEY `idx_identity_resource_claims_resource_id` (`identity_resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Identity resource claims table';

-- ==================== Verification ====================
-- Show created tables
SHOW TABLES LIKE '%resource%';

-- Show table structures
DESCRIBE api_resources;
DESCRIBE api_resource_scopes;
DESCRIBE api_resource_claims;
DESCRIBE identity_resources;
DESCRIBE identity_resource_claims;
