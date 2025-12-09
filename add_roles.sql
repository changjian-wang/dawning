-- 添加3个测试角色
SET @admin_id = (SELECT id FROM users WHERE username='admin');

INSERT INTO roles (id, name, display_name, description, is_system, is_active, permissions, created_at, created_by) VALUES
(UUID(), 'developer', '开发人员', '软件开发工程师角色，可访问开发相关资源', 0, 1, '["application:read:*","scope:read:*","user:read:self"]', NOW(), @admin_id),
(UUID(), 'tester', '测试人员', '软件测试工程师角色，可进行测试相关操作', 0, 1, '["application:read:*","user:read:*","test:execute:*"]', NOW(), @admin_id),
(UUID(), 'guest', '访客', '系统访客角色，仅有只读权限', 0, 1, '["*:read:public"]', NOW(), @admin_id);

-- 查看所有角色
SELECT name, display_name, description, is_system, is_active FROM roles ORDER BY is_system DESC, created_at;
