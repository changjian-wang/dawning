-- Create IP Access Rules table
CREATE TABLE IF NOT EXISTS ip_access_rules (
  id CHAR(36) NOT NULL DEFAULT (UUID()) PRIMARY KEY,
  ip_address VARCHAR(50) NOT NULL,
  rule_type VARCHAR(20) NOT NULL DEFAULT 'blacklist',
  description TEXT,
  is_enabled TINYINT(1) DEFAULT 1,
  expires_at DATETIME,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
  created_by VARCHAR(100),
  INDEX idx_ip_address (ip_address),
  INDEX idx_rule_type (rule_type),
  INDEX idx_is_enabled (is_enabled)
);

-- Create Rate Limit Policies table
CREATE TABLE IF NOT EXISTS rate_limit_policies (
  id CHAR(36) NOT NULL DEFAULT (UUID()) PRIMARY KEY,
  name VARCHAR(100) NOT NULL UNIQUE,
  display_name VARCHAR(200),
  policy_type VARCHAR(50) NOT NULL DEFAULT 'fixed-window',
  permit_limit INT NOT NULL DEFAULT 100,
  window_seconds INT NOT NULL DEFAULT 60,
  segments_per_window INT DEFAULT 6,
  queue_limit INT DEFAULT 0,
  tokens_per_period INT DEFAULT 10,
  replenishment_period_seconds INT DEFAULT 1,
  is_enabled TINYINT(1) DEFAULT 1,
  description TEXT,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_name (name),
  INDEX idx_is_enabled (is_enabled)
);
