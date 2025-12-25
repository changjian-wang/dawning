-- 007_create_audit_logs_table.sql
-- 创建审计日志表

USE dawning_identity;

-- 创建审计日志表
CREATE TABLE IF NOT EXISTS `audit_logs` (
  `id` CHAR(36) NOT NULL COMMENT '审计日志ID (GUID)',
  `user_id` CHAR(36) DEFAULT NULL COMMENT '操作用户ID',
  `username` VARCHAR(100) DEFAULT NULL COMMENT '操作用户名',
  `action` VARCHAR(100) NOT NULL COMMENT '操作类型 (Create, Update, Delete, Login, Logout等)',
  `entity_type` VARCHAR(100) DEFAULT NULL COMMENT '实体类型 (User, Role, Application等)',
  `entity_id` CHAR(36) DEFAULT NULL COMMENT '实体ID',
  `description` VARCHAR(500) DEFAULT NULL COMMENT '操作描述',
  `ip_address` VARCHAR(45) DEFAULT NULL COMMENT 'IP地址 (支持IPv6)',
  `user_agent` VARCHAR(500) DEFAULT NULL COMMENT '用户代理',
  `request_path` VARCHAR(500) DEFAULT NULL COMMENT '请求路径',
  `request_method` VARCHAR(10) DEFAULT NULL COMMENT '请求方法 (GET, POST, PUT, DELETE等)',
  `status_code` INT DEFAULT NULL COMMENT 'HTTP状态码',
  `old_values` JSON DEFAULT NULL COMMENT '修改前的值 (JSON)',
  `new_values` JSON DEFAULT NULL COMMENT '修改后的值 (JSON)',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_audit_logs_user_id` (`user_id`),
  KEY `idx_audit_logs_action` (`action`),
  KEY `idx_audit_logs_entity_type` (`entity_type`),
  KEY `idx_audit_logs_entity_id` (`entity_id`),
  KEY `idx_audit_logs_created_at` (`created_at`),
  KEY `idx_audit_logs_ip_address` (`ip_address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='审计日志表';

-- 显示创建结果
SELECT '审计日志表创建成功！' AS message;
SELECT COUNT(*) AS total_records FROM audit_logs;
