-- ================================================
-- 创建权限表
-- 文件: 008_create_permissions_table.sql
-- 日期: 2025-12-10
-- 说明: 创建权限表，用于细粒度权限控制
-- ================================================

-- 创建权限表
CREATE TABLE IF NOT EXISTS permissions (
    id CHAR(36) PRIMARY KEY,
    code VARCHAR(100) NOT NULL UNIQUE,           -- 权限代码，格式：resource:action (如：user:create, role:update)
    name VARCHAR(100) NOT NULL,                   -- 权限名称
    description TEXT,                             -- 权限描述
    resource VARCHAR(50) NOT NULL,                -- 资源类型 (user, role, audit-log等)
    action VARCHAR(50) NOT NULL,                  -- 操作类型 (create, read, update, delete, export等)
    category VARCHAR(50),                         -- 权限分类 (administration, system, business等)
    is_system TINYINT(1) DEFAULT 0,               -- 是否为系统权限（不可删除）
    is_active TINYINT(1) DEFAULT 1,               -- 是否启用
    display_order INT DEFAULT 0,                  -- 显示顺序
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by CHAR(36),
    updated_at TIMESTAMP NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
    updated_by CHAR(36),
    timestamp BIGINT NOT NULL DEFAULT (UNIX_TIMESTAMP() * 1000)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建索引
CREATE INDEX idx_permissions_code ON permissions(code);
CREATE INDEX idx_permissions_resource ON permissions(resource);
CREATE INDEX idx_permissions_category ON permissions(category);
CREATE INDEX idx_permissions_is_active ON permissions(is_active);

-- 创建角色权限关联表
CREATE TABLE IF NOT EXISTS role_permissions (
    id CHAR(36) PRIMARY KEY,
    role_id CHAR(36) NOT NULL,
    permission_id CHAR(36) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by CHAR(36),
    CONSTRAINT fk_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE,
    CONSTRAINT uk_role_permission UNIQUE (role_id, permission_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建索引
CREATE INDEX idx_role_permissions_role_id ON role_permissions(role_id);
CREATE INDEX idx_role_permissions_permission_id ON role_permissions(permission_id);

-- 插入系统默认权限（使用 UUID()）
INSERT INTO permissions (id, code, name, description, resource, action, category, is_system, display_order) VALUES
-- 用户管理权限
(UUID(), 'user:create', '创建用户', '允许创建新用户', 'user', 'create', 'administration', 1, 100),
(UUID(), 'user:read', '查看用户', '允许查看用户列表和详情', 'user', 'read', 'administration', 1, 101),
(UUID(), 'user:update', '更新用户', '允许更新用户信息', 'user', 'update', 'administration', 1, 102),
(UUID(), 'user:delete', '删除用户', '允许删除用户', 'user', 'delete', 'administration', 1, 103),
(UUID(), 'user:export', '导出用户', '允许导出用户数据', 'user', 'export', 'administration', 1, 104),
(UUID(), 'user:import', '导入用户', '允许导入用户数据', 'user', 'import', 'administration', 1, 105),
(UUID(), 'user:reset_password', '重置密码', '允许重置用户密码', 'user', 'reset_password', 'administration', 1, 106),

-- 角色管理权限
(UUID(), 'role:create', '创建角色', '允许创建新角色', 'role', 'create', 'administration', 1, 200),
(UUID(), 'role:read', '查看角色', '允许查看角色列表和详情', 'role', 'read', 'administration', 1, 201),
(UUID(), 'role:update', '更新角色', '允许更新角色信息', 'role', 'update', 'administration', 1, 202),
(UUID(), 'role:delete', '删除角色', '允许删除角色', 'role', 'delete', 'administration', 1, 203),
(UUID(), 'role:assign_permissions', '分配权限', '允许为角色分配权限', 'role', 'assign_permissions', 'administration', 1, 204),

-- 权限管理权限
(UUID(), 'permission:create', '创建权限', '允许创建新权限', 'permission', 'create', 'administration', 1, 300),
(UUID(), 'permission:read', '查看权限', '允许查看权限列表和详情', 'permission', 'read', 'administration', 1, 301),
(UUID(), 'permission:update', '更新权限', '允许更新权限信息', 'permission', 'update', 'administration', 1, 302),
(UUID(), 'permission:delete', '删除权限', '允许删除权限', 'permission', 'delete', 'administration', 1, 303),

-- 审计日志权限
(UUID(), 'audit-log:read', '查看审计日志', '允许查看审计日志', 'audit-log', 'read', 'administration', 1, 400),
(UUID(), 'audit-log:export', '导出审计日志', '允许导出审计日志', 'audit-log', 'export', 'administration', 1, 401),
(UUID(), 'audit-log:cleanup', '清理审计日志', '允许清理旧的审计日志', 'audit-log', 'cleanup', 'administration', 1, 402),

-- 系统配置权限 (原 system-metadata)
(UUID(), 'system-config:create', '创建配置', '允许创建系统配置', 'system-config', 'create', 'administration', 1, 500),
(UUID(), 'system-config:read', '查看配置', '允许查看系统配置', 'system-config', 'read', 'administration', 1, 501),
(UUID(), 'system-config:update', '更新配置', '允许更新系统配置', 'system-config', 'update', 'administration', 1, 502),
(UUID(), 'system-config:delete', '删除配置', '允许删除系统配置', 'system-config', 'delete', 'administration', 1, 503),

-- OpenIddict客户端权限
(UUID(), 'client:create', '创建客户端', '允许创建OpenIddict客户端', 'client', 'create', 'openiddict', 1, 600),
(UUID(), 'client:read', '查看客户端', '允许查看客户端列表和详情', 'client', 'read', 'openiddict', 1, 601),
(UUID(), 'client:update', '更新客户端', '允许更新客户端信息', 'client', 'update', 'openiddict', 1, 602),
(UUID(), 'client:delete', '删除客户端', '允许删除客户端', 'client', 'delete', 'openiddict', 1, 603),

-- 系统配置权限
(UUID(), 'system:settings', '系统设置', '允许修改系统设置', 'system', 'settings', 'system', 1, 700),
(UUID(), 'system:monitoring', '系统监控', '允许查看系统监控信息', 'system', 'monitoring', 'system', 1, 701),
(UUID(), 'system:logs', '系统日志', '允许查看系统日志', 'system', 'logs', 'system', 1, 702);

-- 为admin角色分配所有权限
INSERT INTO role_permissions (id, role_id, permission_id)
SELECT UUID(), r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'admin';

-- 为user角色分配基本读取权限
INSERT INTO role_permissions (id, role_id, permission_id)
SELECT UUID(), r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'user'
AND p.code IN ('user:read', 'role:read', 'audit-log:read');

-- 为auditor角色分配审计相关权限
INSERT INTO role_permissions (id, role_id, permission_id)
SELECT UUID(), r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'auditor'
AND p.code IN ('audit-log:read', 'audit-log:export', 'user:read', 'role:read');
