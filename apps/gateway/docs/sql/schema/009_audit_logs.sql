-- 007_create_audit_logs_table.sql
-- Create audit logs table

USE dawning_identity;

-- Create audit logs table
CREATE TABLE IF NOT EXISTS `audit_logs` (
  `id` CHAR(36) NOT NULL COMMENT 'Audit log ID (GUID)',
  `tenant_id` CHAR(36) DEFAULT NULL COMMENT 'Tenant ID',
  `user_id` CHAR(36) DEFAULT NULL COMMENT 'Operating user ID',
  `username` VARCHAR(100) DEFAULT NULL COMMENT 'Operating username',
  `action` VARCHAR(100) NOT NULL COMMENT 'Action type (Create, Update, Delete, Login, Logout, etc.)',
  `entity_type` VARCHAR(100) DEFAULT NULL COMMENT 'Entity type (User, Role, Application, etc.)',
  `entity_id` CHAR(36) DEFAULT NULL COMMENT 'Entity ID',
  `description` VARCHAR(500) DEFAULT NULL COMMENT 'Action description',
  `ip_address` VARCHAR(45) DEFAULT NULL COMMENT 'IP address (supports IPv6)',
  `user_agent` VARCHAR(500) DEFAULT NULL COMMENT 'User agent',
  `request_path` VARCHAR(500) DEFAULT NULL COMMENT 'Request path',
  `request_method` VARCHAR(10) DEFAULT NULL COMMENT 'Request method (GET, POST, PUT, DELETE, etc.)',
  `status_code` INT DEFAULT NULL COMMENT 'HTTP status code',
  `old_values` JSON DEFAULT NULL COMMENT 'Values before modification (JSON)',
  `new_values` JSON DEFAULT NULL COMMENT 'Values after modification (JSON)',
  `timestamp` BIGINT NOT NULL DEFAULT 0 COMMENT 'Timestamp (millisecond Unix timestamp)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Created time',
  PRIMARY KEY (`id`),
  KEY `idx_audit_logs_user_id` (`user_id`),
  KEY `idx_audit_logs_action` (`action`),
  KEY `idx_audit_logs_entity_type` (`entity_type`),
  KEY `idx_audit_logs_entity_id` (`entity_id`),
  KEY `idx_audit_logs_created_at` (`created_at`),
  KEY `idx_audit_logs_timestamp` (`timestamp`),
  KEY `idx_audit_logs_ip_address` (`ip_address`),
  KEY `idx_audit_logs_tenant_id` (`tenant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Audit logs table';

-- Display creation results
SELECT 'Audit logs table created successfully!' AS message;
SELECT COUNT(*) AS total_records FROM audit_logs;
