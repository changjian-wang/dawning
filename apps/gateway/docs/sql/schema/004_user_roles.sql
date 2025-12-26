-- 006_create_user_roles_table.sql
-- Create user-role association table

USE dawning_identity;

-- Create user roles association table
CREATE TABLE IF NOT EXISTS `user_roles` (
  `id` CHAR(36) NOT NULL COMMENT 'Association ID (GUID)',
  `user_id` CHAR(36) NOT NULL COMMENT 'User ID',
  `role_id` CHAR(36) NOT NULL COMMENT 'Role ID',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Assignment time',
  `created_by` CHAR(36) DEFAULT NULL COMMENT 'Assigner ID',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_roles` (`user_id`, `role_id`),
  KEY `idx_user_roles_user_id` (`user_id`),
  KEY `idx_user_roles_role_id` (`role_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='User roles association table';

-- Assign admin and super_admin roles to admin user
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

-- Assign user role to other test users
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

-- Display creation results
SELECT 'User roles association table created successfully!' AS message;
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
