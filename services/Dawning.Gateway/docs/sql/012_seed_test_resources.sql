-- 012_seed_test_resources.sql
-- Purpose: Seed standard API Resources and Identity Resources for testing
-- Author: System
-- Date: 2025-12-10

USE dawning_identity;

-- ==================== Clean up existing test data ====================
DELETE FROM api_resource_claims WHERE api_resource_id IN (
    SELECT id FROM api_resources WHERE name IN ('dawning-api', 'user-api', 'admin-api')
);

DELETE FROM api_resource_scopes WHERE api_resource_id IN (
    SELECT id FROM api_resources WHERE name IN ('dawning-api', 'user-api', 'admin-api')
);

DELETE FROM api_resources WHERE name IN ('dawning-api', 'user-api', 'admin-api');

DELETE FROM identity_resource_claims WHERE identity_resource_id IN (
    SELECT id FROM identity_resources WHERE name IN ('openid', 'profile', 'email', 'phone', 'address')
);

DELETE FROM identity_resources WHERE name IN ('openid', 'profile', 'email', 'phone', 'address');

-- ==================== Insert API Resources ====================
-- Dawning API (main backend API)
SET @dawning_api_id = UUID();
INSERT INTO api_resources (id, name, display_name, description, enabled, allowed_access_token_signing_algorithms, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @dawning_api_id,
    'dawning-api',
    'Dawning Backend API',
    'Main backend API for the Dawning system',
    TRUE,
    '["RS256", "ES256"]',
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

-- Insert scopes for Dawning API
INSERT INTO api_resource_scopes (id, api_resource_id, scope, created_at) VALUES
(UUID(), @dawning_api_id, 'api.read', NOW()),
(UUID(), @dawning_api_id, 'api.write', NOW()),
(UUID(), @dawning_api_id, 'api.delete', NOW());

-- Insert claims for Dawning API
INSERT INTO api_resource_claims (id, api_resource_id, type, created_at) VALUES
(UUID(), @dawning_api_id, 'name', NOW()),
(UUID(), @dawning_api_id, 'email', NOW()),
(UUID(), @dawning_api_id, 'role', NOW()),
(UUID(), @dawning_api_id, 'sub', NOW());

-- User API (user management)
SET @user_api_id = UUID();
INSERT INTO api_resources (id, name, display_name, description, enabled, allowed_access_token_signing_algorithms, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @user_api_id,
    'user-api',
    'User Management API',
    'API for user account management and profiles',
    TRUE,
    '["RS256"]',
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

-- Insert scopes for User API
INSERT INTO api_resource_scopes (id, api_resource_id, scope, created_at) VALUES
(UUID(), @user_api_id, 'profile', NOW()),
(UUID(), @user_api_id, 'email', NOW());

-- Insert claims for User API
INSERT INTO api_resource_claims (id, api_resource_id, type, created_at) VALUES
(UUID(), @user_api_id, 'name', NOW()),
(UUID(), @user_api_id, 'given_name', NOW()),
(UUID(), @user_api_id, 'family_name', NOW()),
(UUID(), @user_api_id, 'email', NOW()),
(UUID(), @user_api_id, 'email_verified', NOW());

-- Admin API (administration)
SET @admin_api_id = UUID();
INSERT INTO api_resources (id, name, display_name, description, enabled, allowed_access_token_signing_algorithms, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @admin_api_id,
    'admin-api',
    'Administration API',
    'API for system administration and management',
    TRUE,
    '["RS256"]',
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

-- Insert scopes for Admin API
INSERT INTO api_resource_scopes (id, api_resource_id, scope, created_at) VALUES
(UUID(), @admin_api_id, 'roles', NOW());

-- Insert claims for Admin API
INSERT INTO api_resource_claims (id, api_resource_id, type, created_at) VALUES
(UUID(), @admin_api_id, 'role', NOW()),
(UUID(), @admin_api_id, 'permission', NOW());

-- ==================== Insert Identity Resources ====================
-- OpenID Connect standard identity resources

-- OpenID (required for OIDC)
SET @openid_id = UUID();
INSERT INTO identity_resources (id, name, display_name, description, enabled, required, emphasize, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @openid_id,
    'openid',
    'OpenID',
    'OpenID Connect subject identifier',
    TRUE,
    TRUE,
    FALSE,
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

INSERT INTO identity_resource_claims (id, identity_resource_id, type, created_at) VALUES
(UUID(), @openid_id, 'sub', NOW());

-- Profile
SET @profile_id = UUID();
INSERT INTO identity_resources (id, name, display_name, description, enabled, required, emphasize, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @profile_id,
    'profile',
    'User Profile',
    'User profile information (name, birthdate, etc.)',
    TRUE,
    FALSE,
    TRUE,
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

INSERT INTO identity_resource_claims (id, identity_resource_id, type, created_at) VALUES
(UUID(), @profile_id, 'name', NOW()),
(UUID(), @profile_id, 'family_name', NOW()),
(UUID(), @profile_id, 'given_name', NOW()),
(UUID(), @profile_id, 'middle_name', NOW()),
(UUID(), @profile_id, 'nickname', NOW()),
(UUID(), @profile_id, 'preferred_username', NOW()),
(UUID(), @profile_id, 'profile', NOW()),
(UUID(), @profile_id, 'picture', NOW()),
(UUID(), @profile_id, 'website', NOW()),
(UUID(), @profile_id, 'gender', NOW()),
(UUID(), @profile_id, 'birthdate', NOW()),
(UUID(), @profile_id, 'zoneinfo', NOW()),
(UUID(), @profile_id, 'locale', NOW()),
(UUID(), @profile_id, 'updated_at', NOW());

-- Email
SET @email_id = UUID();
INSERT INTO identity_resources (id, name, display_name, description, enabled, required, emphasize, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @email_id,
    'email',
    'Email Address',
    'User email address and verification status',
    TRUE,
    FALSE,
    TRUE,
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

INSERT INTO identity_resource_claims (id, identity_resource_id, type, created_at) VALUES
(UUID(), @email_id, 'email', NOW()),
(UUID(), @email_id, 'email_verified', NOW());

-- Phone
SET @phone_id = UUID();
INSERT INTO identity_resources (id, name, display_name, description, enabled, required, emphasize, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @phone_id,
    'phone',
    'Phone Number',
    'User phone number and verification status',
    TRUE,
    FALSE,
    FALSE,
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

INSERT INTO identity_resource_claims (id, identity_resource_id, type, created_at) VALUES
(UUID(), @phone_id, 'phone_number', NOW()),
(UUID(), @phone_id, 'phone_number_verified', NOW());

-- Address
SET @address_id = UUID();
INSERT INTO identity_resources (id, name, display_name, description, enabled, required, emphasize, show_in_discovery_document, properties, timestamp, created_at)
VALUES (
    @address_id,
    'address',
    'Address',
    'User postal address',
    TRUE,
    FALSE,
    FALSE,
    TRUE,
    '{}',
    UNIX_TIMESTAMP(),
    NOW()
);

INSERT INTO identity_resource_claims (id, identity_resource_id, type, created_at) VALUES
(UUID(), @address_id, 'address', NOW());

-- ==================== Verification ====================
-- Show API Resources
SELECT 
    ar.name,
    ar.display_name,
    ar.enabled,
    COUNT(DISTINCT ars.scope) AS scope_count,
    COUNT(DISTINCT arc.type) AS claim_count
FROM api_resources ar
LEFT JOIN api_resource_scopes ars ON ar.id = ars.api_resource_id
LEFT JOIN api_resource_claims arc ON ar.id = arc.api_resource_id
GROUP BY ar.id, ar.name, ar.display_name, ar.enabled
ORDER BY ar.created_at;

-- Show Identity Resources
SELECT 
    ir.name,
    ir.display_name,
    ir.enabled,
    ir.required,
    ir.emphasize,
    COUNT(irc.type) AS claim_count
FROM identity_resources ir
LEFT JOIN identity_resource_claims irc ON ir.id = irc.identity_resource_id
GROUP BY ir.id, ir.name, ir.display_name, ir.enabled, ir.required, ir.emphasize
ORDER BY ir.created_at;

-- Total counts
SELECT 
    (SELECT COUNT(*) FROM api_resources) AS total_api_resources,
    (SELECT COUNT(*) FROM api_resource_scopes) AS total_api_scopes,
    (SELECT COUNT(*) FROM api_resource_claims) AS total_api_claims,
    (SELECT COUNT(*) FROM identity_resources) AS total_identity_resources,
    (SELECT COUNT(*) FROM identity_resource_claims) AS total_identity_claims;
