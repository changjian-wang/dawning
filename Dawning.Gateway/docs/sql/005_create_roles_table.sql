-- 005_create_roles_table.sql
-- 创建角色表和初始化数据

USE dawning_identity;

-- 创建角色表
CREATE TABLE IF NOT EXISTS `roles` (
  `id` CHAR(36) NOT NULL COMMENT '角色ID (GUID)',
  `name` VARCHAR(50) NOT NULL COMMENT '角色名称（唯一标识，如admin、user、manager）',
  `display_name` VARCHAR(100) NOT NULL COMMENT '角色显示名称',
  `description` VARCHAR(500) DEFAULT NULL COMMENT '角色描述',
  `is_system` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否为系统角色（系统角色不可删除）',
  `is_active` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
  `permissions` JSON DEFAULT NULL COMMENT '角色权限列表（JSON数组）',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `created_by` CHAR(36) DEFAULT NULL COMMENT '创建者ID',
  `updated_at` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `updated_by` CHAR(36) DEFAULT NULL COMMENT '更新者ID',
  `deleted_at` DATETIME DEFAULT NULL COMMENT '软删除时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_roles_name` (`name`),
  KEY `idx_roles_is_active` (`is_active`),
  KEY `idx_roles_deleted_at` (`deleted_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='角色表';

-- 插入系统预定义角色
INSERT INTO `roles` (`id`, `name`, `display_name`, `description`, `is_system`, `is_active`, `permissions`, `created_at`) VALUES
(UUID(), 'super_admin', '超级管理员', '系统最高权限角色，拥有所有权限', 1, 1, 
 JSON_ARRAY('*:*:*'), NOW()),
 
(UUID(), 'admin', '系统管理员', '系统管理员角色，可以管理用户、角色、应用等', 1, 1, 
 JSON_ARRAY(
   'user:*:*',
   'role:read:*',
   'application:*:*',
   'scope:*:*',
   'claim-type:*:*',
   'system-metadata:*:*'
 ), NOW()),
 
(UUID(), 'user_manager', '用户管理员', '负责用户账号管理', 1, 1, 
 JSON_ARRAY(
   'user:read:*',
   'user:create:*',
   'user:update:*',
   'user:reset-password:*'
 ), NOW()),
 
(UUID(), 'auditor', '审计员', '只读权限，可查看所有数据但不能修改', 1, 1, 
 JSON_ARRAY(
   'user:read:*',
   'role:read:*',
   'application:read:*',
   'scope:read:*',
   'audit-log:read:*'
 ), NOW()),
 
(UUID(), 'user', '普通用户', '普通用户角色，只能管理自己的信息', 1, 1, 
 JSON_ARRAY(
   'user:read:own',
   'user:update:own'
 ), NOW());

-- 显示创建结果
SELECT '角色表创建成功！' AS message;
SELECT id, name, display_name, description, is_system, is_active FROM roles;
