-- 010_seed_test_scopes.sql
-- Purpose: Seed standard OAuth2/OpenID Connect scopes for testing
-- Author: System
-- Date: 2025-01-XX

USE dawning_identity;

-- Delete existing test scopes (if any)
DELETE FROM openiddict_scopes 
WHERE name IN ('openid', 'profile', 'email', 'roles', 'phone', 'address', 'api.read', 'api.write', 'offline_access');

-- Insert standard OpenID Connect scopes
INSERT INTO openiddict_scopes (id, name, display_name, description, resources, properties, timestamp, created_at)
VALUES 
(
    UUID(),
    'openid',
    'OpenID',
    'OpenID Connect scope - allows authentication and access to subject identifier',
    '[]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'profile',
    'User Profile',
    'Access to user profile information (name, birthdate, gender, etc.)',
    '["user-api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'email',
    'Email Address',
    'Access to user email address and email verification status',
    '["user-api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'roles',
    'User Roles',
    'Access to user roles and permissions',
    '["user-api", "admin-api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'phone',
    'Phone Number',
    'Access to user phone number and phone number verification status',
    '["user-api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'address',
    'Address Information',
    'Access to user address information',
    '["user-api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'offline_access',
    'Offline Access',
    'Allows the application to access resources on behalf of the user when the user is not present',
    '[]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'api.read',
    'API Read Access',
    'Read-only access to API resources',
    '["api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
),
(
    UUID(),
    'api.write',
    'API Write Access',
    'Write access to API resources (create, update, delete)',
    '["api"]',
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

-- Verify inserted scopes
SELECT 
    name,
    display_name,
    description,
    JSON_LENGTH(resources) AS resource_count,
    created_at
FROM openiddict_scopes
ORDER BY 
    CASE 
        WHEN name = 'openid' THEN 1
        WHEN name = 'profile' THEN 2
        WHEN name = 'email' THEN 3
        WHEN name = 'roles' THEN 4
        WHEN name = 'phone' THEN 5
        WHEN name = 'address' THEN 6
        WHEN name = 'offline_access' THEN 7
        WHEN name = 'api.read' THEN 8
        WHEN name = 'api.write' THEN 9
        ELSE 10
    END;

-- Count total scopes
SELECT COUNT(*) AS total_scopes FROM openiddict_scopes;

-- Show scope distribution by resource
SELECT 
    TRIM(BOTH '"' FROM JSON_EXTRACT(resources, CONCAT('$[', idx, ']'))) AS resource,
    COUNT(*) AS scope_count
FROM 
    openiddict_scopes
    CROSS JOIN (
        SELECT 0 AS idx UNION SELECT 1 UNION SELECT 2 UNION SELECT 3
    ) AS indices
WHERE 
    JSON_LENGTH(resources) > 0 
    AND idx < JSON_LENGTH(resources)
GROUP BY resource
ORDER BY scope_count DESC;
