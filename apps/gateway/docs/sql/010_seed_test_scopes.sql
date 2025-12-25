-- 010_seed_test_scopes.sql
-- Purpose: Seed standard OAuth2/OpenID Connect scopes for testing
-- Author: System
-- Date: 2025-01-XX
-- Note: OpenIddict tables are created by EF Core. Run this script after first app startup.

USE dawning_identity;

DELIMITER //

DROP PROCEDURE IF EXISTS seed_openiddict_scopes//

CREATE PROCEDURE seed_openiddict_scopes()
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'openiddict_scopes') THEN
        -- Delete existing test scopes (if any)
        DELETE FROM openiddict_scopes 
        WHERE name IN ('openid', 'profile', 'email', 'roles', 'phone', 'address', 'api.read', 'api.write', 'offline_access');

        -- Insert standard OpenID Connect scopes
        INSERT INTO openiddict_scopes (id, name, display_name, description, resources, properties, timestamp, created_at)
        VALUES 
        (UUID(), 'openid', 'OpenID', 'OpenID Connect scope - allows authentication and access to subject identifier', '[]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'profile', 'User Profile', 'Access to user profile information (name, birthdate, gender, etc.)', '["user-api"]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'email', 'Email Address', 'Access to user email address and email verification status', '["user-api"]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'roles', 'User Roles', 'Access to user roles and permissions', '["user-api", "admin-api"]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'phone', 'Phone Number', 'Access to user phone number and phone number verification status', '["user-api"]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'address', 'Address Information', 'Access to user address information', '["user-api"]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'offline_access', 'Offline Access', 'Allows the application to access resources on behalf of the user when the user is not present', '[]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'api.read', 'API Read Access', 'Read-only access to API resources', '["api"]', '{}', UNIX_TIMESTAMP(), NOW()),
        (UUID(), 'api.write', 'API Write Access', 'Write access to API resources (create, update, delete)', '["api"]', '{}', UNIX_TIMESTAMP(), NOW());

        SELECT 'OpenIddict scopes创建成功！' AS message;
        SELECT name, display_name, description FROM openiddict_scopes ORDER BY name;
    ELSE
        SELECT 'OpenIddict表尚未创建，请先启动应用程序以初始化EF Core迁移。' AS message;
    END IF;
END//

DELIMITER ;

CALL seed_openiddict_scopes();
DROP PROCEDURE IF EXISTS seed_openiddict_scopes;
