-- 插入系统日志示例数据
USE dawning_identity;

INSERT INTO system_logs (id, level, message, exception, stack_trace, source, user_id, username, ip_address, user_agent, request_path, request_method, status_code, created_at, timestamp)
VALUES
-- 1. Information - 用户登录
(UUID(), 'Information', '用户成功登录系统', NULL, NULL, 'AuthenticationService', NULL, 'admin', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', '/api/auth/login', 'POST', 200, NOW(), UNIX_TIMESTAMP(NOW())),

-- 2. Information - 查询用户列表
(UUID(), 'Information', '用户查询用户列表', NULL, NULL, 'UserController', NULL, 'admin', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', '/api/user', 'GET', 200, NOW(), UNIX_TIMESTAMP(NOW())),

-- 3. Warning - 未授权访问
(UUID(), 'Warning', '用户尝试访问未授权的资源', NULL, NULL, 'AuthorizationMiddleware', NULL, 'guest', '192.168.1.105', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', '/api/admin/settings', 'GET', 403, NOW(), UNIX_TIMESTAMP(NOW())),

-- 4. Error - 数据库连接失败
(UUID(), 'Error', '数据库连接失败', 'MySql.Data.MySqlClient.MySqlException: Unable to connect to any of the specified MySQL hosts.', '   at MySql.Data.MySqlClient.NativeDriver.Open()\n   at MySql.Data.MySqlClient.Driver.Open()\n   at MySql.Data.MySqlClient.MySqlConnection.Open()', 'DatabaseService', NULL, 'system', '127.0.0.1', 'Internal', '/api/data/query', 'POST', 500, NOW(), UNIX_TIMESTAMP(NOW())),

-- 5. Information - 创建角色
(UUID(), 'Information', '用户创建新角色', NULL, NULL, 'RoleController', NULL, 'admin', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', '/api/role', 'POST', 201, NOW(), UNIX_TIMESTAMP(NOW())),

-- 6. Warning - 密码错误
(UUID(), 'Warning', '密码输入错误次数过多', NULL, NULL, 'AuthenticationService', NULL, 'testuser', '192.168.1.120', 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X) AppleWebKit/605.1.15', '/api/auth/login', 'POST', 401, NOW(), UNIX_TIMESTAMP(NOW())),

-- 7. Error - 文件上传失败
(UUID(), 'Error', '文件上传失败：文件大小超过限制', 'System.IO.IOException: The file exceeds the maximum allowed size of 10MB.', '   at FileUploadService.ValidateFileSize(Stream fileStream)\n   at FileUploadService.UploadAsync(IFormFile file)', 'FileUploadService', NULL, 'user1', '192.168.1.115', 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 Chrome/91.0.4472.124', '/api/file/upload', 'POST', 413, NOW(), UNIX_TIMESTAMP(NOW())),

-- 8. Information - 系统配置更新
(UUID(), 'Information', '系统配置更新成功', NULL, NULL, 'SystemConfigController', NULL, 'admin', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/91.0.864.59', '/api/config/update', 'PUT', 200, NOW(), UNIX_TIMESTAMP(NOW())),

-- 9. Information - 用户登出
(UUID(), 'Information', '用户登出系统', NULL, NULL, 'AuthenticationService', NULL, 'admin', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', '/api/auth/logout', 'POST', 200, NOW(), UNIX_TIMESTAMP(NOW())),

-- 10. Warning - API限流
(UUID(), 'Warning', 'API请求频率超过限制', NULL, NULL, 'RateLimitMiddleware', NULL, 'api_user', '203.0.113.50', 'Python/3.9 requests/2.26.0', '/api/data/batch', 'POST', 429, NOW(), UNIX_TIMESTAMP(NOW()));

SELECT COUNT(*) as total_logs FROM system_logs;
