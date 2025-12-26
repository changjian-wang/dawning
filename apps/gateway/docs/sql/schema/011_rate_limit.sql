-- =============================================
-- Rate Limit Policies and IP Access Rules Tables
-- =============================================

-- Rate limit policies table
CREATE TABLE IF NOT EXISTS `rate_limit_policies` (
    `id` CHAR(36) NOT NULL,
    `name` VARCHAR(100) NOT NULL,
    `display_name` VARCHAR(200),
    `policy_type` VARCHAR(50) NOT NULL DEFAULT 'fixed-window',
    `permit_limit` INT NOT NULL DEFAULT 100,
    `window_seconds` INT NOT NULL DEFAULT 60,
    `segments_per_window` INT NOT NULL DEFAULT 6,
    `queue_limit` INT NOT NULL DEFAULT 0,
    `tokens_per_period` INT NOT NULL DEFAULT 10,
    `replenishment_period_seconds` INT NOT NULL DEFAULT 1,
    `is_enabled` TINYINT(1) NOT NULL DEFAULT 1,
    `description` TEXT,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_rate_limit_policy_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- IP access rules table
CREATE TABLE IF NOT EXISTS `ip_access_rules` (
    `id` CHAR(36) NOT NULL,
    `ip_address` VARCHAR(50) NOT NULL,
    `rule_type` VARCHAR(20) NOT NULL DEFAULT 'blacklist',
    `description` TEXT,
    `is_enabled` TINYINT(1) NOT NULL DEFAULT 1,
    `expires_at` DATETIME,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME,
    `created_by` VARCHAR(100),
    PRIMARY KEY (`id`),
    KEY `idx_ip_access_rule_type` (`rule_type`),
    KEY `idx_ip_access_enabled` (`is_enabled`),
    KEY `idx_ip_access_expires` (`expires_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert default rate limit policies
INSERT INTO `rate_limit_policies` (`id`, `name`, `display_name`, `policy_type`, `permit_limit`, `window_seconds`, `description`) VALUES
(UUID(), 'default', 'Default Policy', 'fixed-window', 100, 60, '100 requests per minute'),
(UUID(), 'strict', 'Strict Limit', 'fixed-window', 30, 60, '30 requests per minute'),
(UUID(), 'relaxed', 'Relaxed Limit', 'fixed-window', 500, 60, '500 requests per minute'),
(UUID(), 'api-standard', 'API Standard', 'sliding-window', 100, 60, 'Sliding window, 100 requests per minute'),
(UUID(), 'burst-allow', 'Allow Burst', 'token-bucket', 100, 60, 'Token bucket, allows short-term burst requests');
