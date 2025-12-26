-- ====================================================
-- OpenIddict Tables for MySQL 8.0
-- Date: 2025-11-18
-- Description: OpenIddict core table structure
--   - timestamp: BIGINT millisecond Unix timestamp (UTC), for indexing and pagination
--   - created_at: DATETIME stores UTC time, set by application layer
-- ====================================================

-- 1. Applications Table
CREATE TABLE IF NOT EXISTS `openiddict_applications` (
    `id` CHAR(36) NOT NULL COMMENT 'Primary key (GUID)',
    `client_id` VARCHAR(100) NOT NULL COMMENT 'Client ID',
    `client_secret` VARCHAR(500) NULL COMMENT 'Client secret (hashed)',
    `display_name` VARCHAR(200) NULL COMMENT 'Display name',
    `type` VARCHAR(50) NULL COMMENT 'Client type (confidential, public)',
    `consent_type` VARCHAR(50) NULL COMMENT 'Consent type (explicit, implicit, systematic)',
    `permissions` TEXT NULL COMMENT 'Permissions list (JSON format)',
    `redirect_uris` TEXT NULL COMMENT 'Redirect URIs list (JSON format)',
    `post_logout_redirect_uris` TEXT NULL COMMENT 'Post-logout redirect URIs list (JSON format)',
    `requirements` TEXT NULL COMMENT 'Requirements list (JSON format)',
    `properties` TEXT NULL COMMENT 'Extended properties (JSON format)',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix timestamp in milliseconds (UTC, for indexing and pagination)',
    `created_at` DATETIME NOT NULL COMMENT 'Created time (UTC)',
    `updated_at` DATETIME NULL COMMENT 'Updated time (UTC)',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_client_id` (`client_id`),
    INDEX `idx_type` (`type`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict applications table';

-- 2. Scopes Table
CREATE TABLE IF NOT EXISTS `openiddict_scopes` (
    `id` CHAR(36) NOT NULL COMMENT 'Primary key (GUID)',
    `name` VARCHAR(200) NOT NULL COMMENT 'Scope name',
    `display_name` VARCHAR(200) NULL COMMENT 'Display name',
    `description` VARCHAR(500) NULL COMMENT 'Description',
    `resources` TEXT NULL COMMENT 'Resources list (JSON format)',
    `properties` TEXT NULL COMMENT 'Extended properties (JSON format)',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix timestamp in milliseconds (UTC, for indexing and pagination)',
    `created_at` DATETIME NOT NULL COMMENT 'Created time (UTC)',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict scopes table';

-- 3. Authorizations Table
CREATE TABLE IF NOT EXISTS `openiddict_authorizations` (
    `id` CHAR(36) NOT NULL COMMENT 'Primary key (GUID)',
    `application_id` CHAR(36) NULL COMMENT 'Associated application ID',
    `subject` VARCHAR(200) NULL COMMENT 'User identifier',
    `type` VARCHAR(50) NULL COMMENT 'Authorization type',
    `status` VARCHAR(50) NULL COMMENT 'Authorization status (valid, revoked)',
    `scopes` TEXT NULL COMMENT 'Authorized scopes list (JSON format)',
    `properties` TEXT NULL COMMENT 'Extended properties (JSON format)',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix timestamp in milliseconds (UTC, for indexing and pagination)',
    `created_at` DATETIME NOT NULL COMMENT 'Created time (UTC)',
    PRIMARY KEY (`id`),
    INDEX `idx_application_id` (`application_id`),
    INDEX `idx_subject` (`subject`),
    INDEX `idx_status` (`status`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict authorizations table';

-- 4. Tokens Table
CREATE TABLE IF NOT EXISTS `openiddict_tokens` (
    `id` CHAR(36) NOT NULL COMMENT 'Primary key (GUID)',
    `application_id` CHAR(36) NULL COMMENT 'Associated application ID',
    `authorization_id` CHAR(36) NULL COMMENT 'Associated authorization ID',
    `subject` VARCHAR(200) NULL COMMENT 'User identifier',
    `type` VARCHAR(50) NULL COMMENT 'Token type (access_token, refresh_token, id_token)',
    `status` VARCHAR(50) NULL COMMENT 'Token status (valid, revoked, redeemed)',
    `payload` TEXT NULL COMMENT 'Token payload (JWT)',
    `reference_id` VARCHAR(200) NULL COMMENT 'Reference ID (for token introspection)',
    `expires_at` DATETIME NULL COMMENT 'Expiration time (UTC)',
    `timestamp` BIGINT NOT NULL COMMENT 'Unix timestamp in milliseconds (UTC, for indexing and pagination)',
    `created_at` DATETIME NOT NULL COMMENT 'Created time (UTC)',
    PRIMARY KEY (`id`),
    INDEX `idx_application_id` (`application_id`),
    INDEX `idx_authorization_id` (`authorization_id`),
    INDEX `idx_subject` (`subject`),
    INDEX `idx_reference_id` (`reference_id`),
    INDEX `idx_status` (`status`),
    INDEX `idx_expires_at` (`expires_at`),
    INDEX `idx_timestamp` (`timestamp`),
    INDEX `idx_created_at` (`created_at`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='OpenIddict tokens table';
