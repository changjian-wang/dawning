-- ================================================
-- 插入测试OpenIddict客户端
-- 文件: 009_seed_test_openiddict_clients.sql
-- 日期: 2025-12-10
-- 说明: 为测试创建示例OpenIddict应用程序
-- 注意: OpenIddict表由EF Core自动创建，首次启动应用后再执行此脚本
-- ================================================

-- 检查OpenIddict表是否存在，不存在则跳过
-- 此脚本需要在应用程序首次启动后手动执行
DELIMITER //

DROP PROCEDURE IF EXISTS seed_openiddict_clients//

CREATE PROCEDURE seed_openiddict_clients()
BEGIN
    -- 检查表是否存在
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'openiddict_applications') THEN
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
            NULL,
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

        -- 2. Mobile App - 移动应用
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
            NULL,
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

        -- 3. API Client - 机器对机器
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
            'temp_secret_need_hash',
            'API Service Client',
            'confidential',
            'implicit',
            '["ept:token", "gt:client_credentials", "scp:api.read", "scp:api.write"]',
            '[]',
            '[]',
            '[]',
            '{"application_type": "service", "environment": "development"}',
            UNIX_TIMESTAMP() * 1000,
            NOW()
        );

        SELECT 'OpenIddict测试客户端创建成功！' AS message;
        
        -- 显示结果
        SELECT client_id, display_name, type FROM openiddict_applications 
        WHERE client_id IN ('admin-portal', 'mobile-app', 'api-client');
    ELSE
        SELECT 'OpenIddict表尚未创建，请先启动应用程序以初始化EF Core迁移，然后手动执行此脚本。' AS message;
    END IF;
END//

DELIMITER ;

-- 执行存储过程
CALL seed_openiddict_clients();

-- 清理存储过程
DROP PROCEDURE IF EXISTS seed_openiddict_clients;
