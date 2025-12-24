-- =============================================
-- 限流策略和 IP 访问规则表
-- =============================================

-- 限流策略表
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

-- IP 访问规则表
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

-- 插入默认限流策略
INSERT INTO `rate_limit_policies` (`id`, `name`, `display_name`, `policy_type`, `permit_limit`, `window_seconds`, `description`) VALUES
(UUID(), 'default', '默认策略', 'fixed-window', 100, 60, '每分钟 100 次请求'),
(UUID(), 'strict', '严格限制', 'fixed-window', 30, 60, '每分钟 30 次请求'),
(UUID(), 'relaxed', '宽松限制', 'fixed-window', 500, 60, '每分钟 500 次请求'),
(UUID(), 'api-standard', 'API 标准', 'sliding-window', 100, 60, '滑动窗口，每分钟 100 次'),
(UUID(), 'burst-allow', '允许突发', 'token-bucket', 100, 60, '令牌桶，允许短期突发请求');
