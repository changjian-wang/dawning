-- ============================================
-- V8: 多租户支持
-- 创建日期: 2025-12-23
-- 描述: 添加多租户支持，包括租户表和现有表的tenant_id字段
-- ============================================

-- 1. 创建租户表
CREATE TABLE IF NOT EXISTS `tenants` (
    `id` CHAR(36) NOT NULL COMMENT '租户ID',
    `code` VARCHAR(50) NOT NULL COMMENT '租户代码（唯一标识）',
    `name` VARCHAR(100) NOT NULL COMMENT '租户名称',
    `description` VARCHAR(500) NULL COMMENT '租户描述',
    `domain` VARCHAR(255) NULL COMMENT '绑定域名',
    `email` VARCHAR(255) NULL COMMENT '联系邮箱',
    `phone` VARCHAR(50) NULL COMMENT '联系电话',
    `logo_url` VARCHAR(500) NULL COMMENT '租户Logo URL',
    `settings` JSON NULL COMMENT '租户配置（JSON格式）',
    `connection_string` VARCHAR(500) NULL COMMENT '独立数据库连接字符串',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
    `plan` VARCHAR(50) NOT NULL DEFAULT 'free' COMMENT '订阅计划',
    `subscription_expires_at` DATETIME NULL COMMENT '订阅到期时间',
    `max_users` INT NULL COMMENT '最大用户数限制',
    `max_storage_mb` INT NULL COMMENT '最大存储空间（MB）',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `updated_at` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    `created_by` CHAR(36) NULL COMMENT '创建者ID',
    `updated_by` CHAR(36) NULL COMMENT '更新者ID',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_tenants_code` (`code`),
    UNIQUE KEY `uk_tenants_domain` (`domain`),
    KEY `idx_tenants_is_active` (`is_active`),
    KEY `idx_tenants_plan` (`plan`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='租户表';

-- 2. 为现有表添加 tenant_id 字段（可选，根据需要启用）
-- 注意：以下操作可能需要较长时间，请在维护窗口执行

-- 2.1 用户表
ALTER TABLE `users` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_users_tenant_id` (`tenant_id`);

-- 2.2 角色表
ALTER TABLE `roles` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_roles_tenant_id` (`tenant_id`);

-- 2.3 权限表
ALTER TABLE `permissions` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_permissions_tenant_id` (`tenant_id`);

-- 2.4 系统配置表
ALTER TABLE `system_configs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_system_configs_tenant_id` (`tenant_id`);

-- 2.5 审计日志表
ALTER TABLE `audit_logs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_audit_logs_tenant_id` (`tenant_id`);

-- 2.6 系统日志表
ALTER TABLE `system_logs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_system_logs_tenant_id` (`tenant_id`);

-- 2.7 告警规则表
ALTER TABLE `alert_rules` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_alert_rules_tenant_id` (`tenant_id`);

-- 2.8 告警历史表
ALTER TABLE `alert_history` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_alert_history_tenant_id` (`tenant_id`);

-- 2.9 请求日志表
ALTER TABLE `request_logs` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_request_logs_tenant_id` (`tenant_id`);

-- 2.10 网关路由表
ALTER TABLE `gateway_routes` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_gateway_routes_tenant_id` (`tenant_id`);

-- 2.11 网关集群表
ALTER TABLE `gateway_clusters` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_gateway_clusters_tenant_id` (`tenant_id`);

-- 2.12 限流策略表
ALTER TABLE `rate_limit_policies` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_rate_limit_policies_tenant_id` (`tenant_id`);

-- 2.13 IP访问规则表
ALTER TABLE `ip_access_rules` 
ADD COLUMN `tenant_id` CHAR(36) NULL COMMENT '租户ID' AFTER `id`,
ADD INDEX `idx_ip_access_rules_tenant_id` (`tenant_id`);

-- 3. 创建默认租户（主租户）
INSERT INTO `tenants` (`id`, `code`, `name`, `description`, `is_active`, `plan`, `created_at`)
VALUES (
    UUID(),
    'default',
    '默认租户',
    '系统默认租户，用于主机管理员和公共数据',
    1,
    'enterprise',
    NOW()
);

-- 4. 将现有数据关联到默认租户（可选）
-- 取消注释以下语句以将现有数据迁移到默认租户
-- SET @default_tenant_id = (SELECT id FROM tenants WHERE code = 'default');
-- UPDATE users SET tenant_id = @default_tenant_id WHERE tenant_id IS NULL;
-- UPDATE roles SET tenant_id = @default_tenant_id WHERE tenant_id IS NULL;
-- UPDATE permissions SET tenant_id = @default_tenant_id WHERE tenant_id IS NULL;
-- ... 其他表类似

-- 5. 添加租户相关权限
INSERT INTO `permissions` (`id`, `code`, `name`, `description`, `resource`, `action`, `category`, `is_active`, `sort_order`)
VALUES 
    (UUID(), 'tenant:create', '创建租户', '允许创建新租户', 'tenant', 'create', 'multitenancy', 1, 700),
    (UUID(), 'tenant:read', '查看租户', '允许查看租户列表和详情', 'tenant', 'read', 'multitenancy', 1, 701),
    (UUID(), 'tenant:update', '更新租户', '允许更新租户信息', 'tenant', 'update', 'multitenancy', 1, 702),
    (UUID(), 'tenant:delete', '删除租户', '允许删除租户', 'tenant', 'delete', 'multitenancy', 1, 703),
    (UUID(), 'tenant:switch', '切换租户', '允许在租户之间切换', 'tenant', 'switch', 'multitenancy', 1, 704);

-- ============================================
-- 回滚脚本（如需回滚，请执行以下语句）
-- ============================================
-- DROP TABLE IF EXISTS `tenants`;
-- ALTER TABLE `users` DROP COLUMN `tenant_id`, DROP INDEX `idx_users_tenant_id`;
-- ALTER TABLE `roles` DROP COLUMN `tenant_id`, DROP INDEX `idx_roles_tenant_id`;
-- ALTER TABLE `permissions` DROP COLUMN `tenant_id`, DROP INDEX `idx_permissions_tenant_id`;
-- ALTER TABLE `system_configs` DROP COLUMN `tenant_id`, DROP INDEX `idx_system_configs_tenant_id`;
-- ALTER TABLE `audit_logs` DROP COLUMN `tenant_id`, DROP INDEX `idx_audit_logs_tenant_id`;
-- ALTER TABLE `system_logs` DROP COLUMN `tenant_id`, DROP INDEX `idx_system_logs_tenant_id`;
-- ALTER TABLE `alert_rules` DROP COLUMN `tenant_id`, DROP INDEX `idx_alert_rules_tenant_id`;
-- ALTER TABLE `alert_history` DROP COLUMN `tenant_id`, DROP INDEX `idx_alert_history_tenant_id`;
-- ALTER TABLE `request_logs` DROP COLUMN `tenant_id`, DROP INDEX `idx_request_logs_tenant_id`;
-- ALTER TABLE `gateway_routes` DROP COLUMN `tenant_id`, DROP INDEX `idx_gateway_routes_tenant_id`;
-- ALTER TABLE `gateway_clusters` DROP COLUMN `tenant_id`, DROP INDEX `idx_gateway_clusters_tenant_id`;
-- ALTER TABLE `rate_limit_policies` DROP COLUMN `tenant_id`, DROP INDEX `idx_rate_limit_policies_tenant_id`;
-- ALTER TABLE `ip_access_rules` DROP COLUMN `tenant_id`, DROP INDEX `idx_ip_access_rules_tenant_id`;
-- DELETE FROM `permissions` WHERE `category` = 'multitenancy';
