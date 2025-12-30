-- ================================================
-- Create permissions table
-- File: 008_create_permissions_table.sql
-- Date: 2025-12-10
-- Description: Create permissions table for fine-grained access control
-- ================================================

-- Create permissions table
CREATE TABLE IF NOT EXISTS permissions (
    id CHAR(36) PRIMARY KEY,
    tenant_id CHAR(36) NULL,                      -- Tenant ID
    code VARCHAR(100) NOT NULL UNIQUE,           -- Permission code, format: resource:action (e.g., user:create, role:update)
    name VARCHAR(100) NOT NULL,                   -- Permission name
    description TEXT,                             -- Permission description
    resource VARCHAR(50) NOT NULL,                -- Resource type (user, role, audit-log, etc.)
    action VARCHAR(50) NOT NULL,                  -- Action type (create, read, update, delete, export, etc.)
    category VARCHAR(50),                         -- Permission category (administration, system, business, etc.)
    is_system TINYINT(1) DEFAULT 0,               -- Is system permission (cannot be deleted)
    is_active TINYINT(1) DEFAULT 1,               -- Is active
    display_order INT DEFAULT 0,                  -- Display order
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by CHAR(36),
    updated_at TIMESTAMP NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
    updated_by CHAR(36),
    timestamp BIGINT NOT NULL DEFAULT (UNIX_TIMESTAMP() * 1000),
    INDEX idx_permissions_code (code),
    INDEX idx_permissions_resource (resource),
    INDEX idx_permissions_category (category),
    INDEX idx_permissions_is_active (is_active),
    INDEX idx_permissions_tenant_id (tenant_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create role permissions association table
CREATE TABLE IF NOT EXISTS role_permissions (
    id CHAR(36) PRIMARY KEY,
    role_id CHAR(36) NOT NULL,
    permission_id CHAR(36) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by CHAR(36),
    CONSTRAINT uk_role_permission UNIQUE (role_id, permission_id),
    INDEX idx_role_permissions_role_id (role_id),
    INDEX idx_role_permissions_permission_id (permission_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert system default permissions (using UUID())
INSERT INTO permissions (id, code, name, description, resource, action, category, is_system, display_order) VALUES
-- User management permissions
(UUID(), 'user:create', 'Create User', 'Allows creating new users', 'user', 'create', 'administration', 1, 100),
(UUID(), 'user:read', 'View User', 'Allows viewing user list and details', 'user', 'read', 'administration', 1, 101),
(UUID(), 'user:update', 'Update User', 'Allows updating user information', 'user', 'update', 'administration', 1, 102),
(UUID(), 'user:delete', 'Delete User', 'Allows deleting users', 'user', 'delete', 'administration', 1, 103),
(UUID(), 'user:export', 'Export User', 'Allows exporting user data', 'user', 'export', 'administration', 1, 104),
(UUID(), 'user:import', 'Import User', 'Allows importing user data', 'user', 'import', 'administration', 1, 105),
(UUID(), 'user:reset_password', 'Reset Password', 'Allows resetting user passwords', 'user', 'reset_password', 'administration', 1, 106),

-- Role management permissions
(UUID(), 'role:create', 'Create Role', 'Allows creating new roles', 'role', 'create', 'administration', 1, 200),
(UUID(), 'role:read', 'View Role', 'Allows viewing role list and details', 'role', 'read', 'administration', 1, 201),
(UUID(), 'role:update', 'Update Role', 'Allows updating role information', 'role', 'update', 'administration', 1, 202),
(UUID(), 'role:delete', 'Delete Role', 'Allows deleting roles', 'role', 'delete', 'administration', 1, 203),
(UUID(), 'role:assign_permissions', 'Assign Permissions', 'Allows assigning permissions to roles', 'role', 'assign_permissions', 'administration', 1, 204),

-- Permission management permissions
(UUID(), 'permission:create', 'Create Permission', 'Allows creating new permissions', 'permission', 'create', 'administration', 1, 300),
(UUID(), 'permission:read', 'View Permission', 'Allows viewing permission list and details', 'permission', 'read', 'administration', 1, 301),
(UUID(), 'permission:update', 'Update Permission', 'Allows updating permission information', 'permission', 'update', 'administration', 1, 302),
(UUID(), 'permission:delete', 'Delete Permission', 'Allows deleting permissions', 'permission', 'delete', 'administration', 1, 303),

-- Audit log permissions
(UUID(), 'audit-log:read', 'View Audit Logs', 'Allows viewing audit logs', 'audit-log', 'read', 'administration', 1, 400),
(UUID(), 'audit-log:export', 'Export Audit Logs', 'Allows exporting audit logs', 'audit-log', 'export', 'administration', 1, 401),
(UUID(), 'audit-log:cleanup', 'Cleanup Audit Logs', 'Allows cleaning up old audit logs', 'audit-log', 'cleanup', 'administration', 1, 402),

-- System config permissions (formerly system-metadata)
(UUID(), 'system-config:create', 'Create Config', 'Allows creating system config', 'system-config', 'create', 'administration', 1, 500),
(UUID(), 'system-config:read', 'View Config', 'Allows viewing system config', 'system-config', 'read', 'administration', 1, 501),
(UUID(), 'system-config:update', 'Update Config', 'Allows updating system config', 'system-config', 'update', 'administration', 1, 502),
(UUID(), 'system-config:delete', 'Delete Config', 'Allows deleting system config', 'system-config', 'delete', 'administration', 1, 503),

-- OpenIddict client permissions
(UUID(), 'client:create', 'Create Client', 'Allows creating OpenIddict clients', 'client', 'create', 'openiddict', 1, 600),
(UUID(), 'client:read', 'View Client', 'Allows viewing client list and details', 'client', 'read', 'openiddict', 1, 601),
(UUID(), 'client:update', 'Update Client', 'Allows updating client information', 'client', 'update', 'openiddict', 1, 602),
(UUID(), 'client:delete', 'Delete Client', 'Allows deleting clients', 'client', 'delete', 'openiddict', 1, 603),

-- System settings permissions
(UUID(), 'system:settings', 'System Settings', 'Allows modifying system settings', 'system', 'settings', 'system', 1, 700),
(UUID(), 'system:monitoring', 'System Monitoring', 'Allows viewing system monitoring information', 'system', 'monitoring', 'system', 1, 701),
(UUID(), 'system:logs', 'System Logs', 'Allows viewing system logs', 'system', 'logs', 'system', 1, 702);

-- Assign all permissions to admin role
INSERT INTO role_permissions (id, role_id, permission_id)
SELECT UUID(), r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'admin';

-- Assign basic read permissions to user role
INSERT INTO role_permissions (id, role_id, permission_id)
SELECT UUID(), r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'user'
AND p.code IN ('user:read', 'role:read', 'audit-log:read');

-- Assign audit-related permissions to auditor role
INSERT INTO role_permissions (id, role_id, permission_id)
SELECT UUID(), r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'auditor'
AND p.code IN ('audit-log:read', 'audit-log:export', 'user:read', 'role:read');
