-- ================================================
-- 插入测试OpenIddict客户端
-- 文件: 009_seed_test_openiddict_clients.sql
-- 日期: 2025-12-10
-- 说明: 为测试创建示例OpenIddict应用程序
-- ================================================

-- 清理现有测试数据
DELETE FROM openiddict_applications WHERE client_id IN ('admin-portal', 'mobile-app', 'api-client');

-- 1. Admin Portal - SPA应用（使用PKCE，不需要客户端密钥）
INSERT INTO openiddict_applications (
    id,
    client_id,
    client_secret,
    display_name,
    type,
    consent_type,
    permissions,
    redirect_uris,
    post_logout_redirect_uris,
    requirements,
    properties,
    timestamp,
    created_at
) VALUES (
    UUID(),
    'admin-portal',
    NULL,  -- Public客户端不需要密钥
    'Admin Portal',
    'public',
    'explicit',
    '["ept:authorization", "ept:token", "ept:logout", "gt:authorization_code", "gt:refresh_token", "rst:code", "scp:openid", "scp:profile", "scp:email", "scp:roles"]',
    '["http://localhost:5173/callback", "http://localhost:5173/silent-renew"]',
    '["http://localhost:5173", "http://localhost:5173/logout"]',
    '["pkce"]',
    '{"application_type": "spa", "framework": "vue3"}',
    UNIX_TIMESTAMP() * 1000,
    NOW()
);

-- 2. Mobile App - 移动应用（使用PKCE，Public类型）
INSERT INTO openiddict_applications (
    id,
    client_id,
    client_secret,
    display_name,
    type,
    consent_type,
    permissions,
    redirect_uris,
    post_logout_redirect_uris,
    requirements,
    properties,
    timestamp,
    created_at
) VALUES (
    UUID(),
    'mobile-app',
    NULL,  -- Public客户端
    'Mobile Application',
    'public',
    'explicit',
    '["ept:authorization", "ept:token", "ept:logout", "gt:authorization_code", "gt:refresh_token", "rst:code", "scp:openid", "scp:profile", "scp:offline_access"]',
    '["com.dawning.app://callback"]',
    '["com.dawning.app://logout"]',
    '["pkce"]',
    '{"application_type": "mobile", "platform": "ios_android"}',
    UNIX_TIMESTAMP() * 1000,
    NOW()
);

-- 3. API Client - 机器对机器（Confidential，需要客户端密钥）
-- 注意：密钥 "api_secret_2024" 已使用PBKDF2哈希（需要在应用层完成）
-- 这里使用临时明文，实际生产环境应该通过API创建
INSERT INTO openiddict_applications (
    id,
    client_id,
    client_secret,
    display_name,
    type,
    consent_type,
    permissions,
    redirect_uris,
    post_logout_redirect_uris,
    requirements,
    properties,
    timestamp,
    created_at
) VALUES (
    UUID(),
    'api-client',
    'temp_secret_need_hash',  -- 需要通过API更新为哈希值
    'API Service Client',
    'confidential',
    'implicit',  -- 服务器到服务器不需要用户同意
    '["ept:token", "gt:client_credentials", "scp:api.read", "scp:api.write"]',
    '[]',  -- 客户端凭证流不需要重定向URI
    '[]',
    '[]',
    '{"application_type": "service", "environment": "development"}',
    UNIX_TIMESTAMP() * 1000,
    NOW()
);

-- 验证插入结果
SELECT 
    client_id,
    display_name,
    type,
    consent_type,
    created_at
FROM openiddict_applications
WHERE client_id IN ('admin-portal', 'mobile-app', 'api-client')
ORDER BY created_at;

-- 显示权限统计
SELECT 
    type AS client_type,
    COUNT(*) AS count,
    GROUP_CONCAT(client_id) AS clients
FROM openiddict_applications
GROUP BY type;
