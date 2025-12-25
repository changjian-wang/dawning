-- 006_create_user_roles_table.sql
-- 创建用户-角色关联表

USE dawning_identity;

-- 创建用户角色关联表
CREATE TABLE IF NOT EXISTS `user_roles` (
  `id` CHAR(36) NOT NULL COMMENT '关联ID (GUID)',
  `user_id` CHAR(36) NOT NULL COMMENT '用户ID',
  `role_id` CHAR(36) NOT NULL COMMENT '角色ID',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '分配时间',
  `created_by` CHAR(36) DEFAULT NULL COMMENT '分配者ID',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_roles` (`user_id`, `role_id`),
  KEY `idx_user_roles_user_id` (`user_id`),
  KEY `idx_user_roles_role_id` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='用户角色关联表';

-- 为admin用户分配 admin 和 super_admin 角色
INSERT INTO `user_roles` (`id`, `user_id`, `role_id`, `created_at`)
SELECT 
  UUID() AS id,
  u.id AS user_id,
  r.id AS role_id,
  NOW() AS created_at
FROM `users` u
CROSS JOIN `roles` r
WHERE u.username = 'admin' 
  AND r.name IN ('admin', 'super_admin')
  AND r.deleted_at IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM `user_roles` ur 
    WHERE ur.user_id = u.id AND ur.role_id = r.id
  );

-- 为其他测试用户分配user角色
INSERT INTO `user_roles` (`id`, `user_id`, `role_id`, `created_at`)
SELECT 
  UUID() AS id,
  u.id AS user_id,
  r.id AS role_id,
  NOW() AS created_at
FROM `users` u
CROSS JOIN `roles` r
WHERE u.username IN ('zhangsan', 'lisi', 'wangwu', 'zhaoliu', 'sunqi')
  AND r.name = 'user'
  AND r.deleted_at IS NULL
  AND NOT EXISTS (
    SELECT 1 FROM `user_roles` ur 
    WHERE ur.user_id = u.id AND ur.role_id = r.id
  );

-- 显示创建结果
SELECT '用户角色关联表创建成功！' AS message;
SELECT 
  ur.id,
  u.username,
  r.name AS role_name,
  r.display_name AS role_display_name,
  ur.created_at
FROM user_roles ur
JOIN users u ON ur.user_id = u.id
JOIN roles r ON ur.role_id = r.id
ORDER BY u.username, r.name;
