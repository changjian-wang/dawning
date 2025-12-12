# 创建系统日志示例数据
$baseUrl = "http://localhost:5202"

# 首先登录获取token
Write-Host "正在登录获取token..." -ForegroundColor Cyan
$loginBody = @{
    grant_type = "password"
    username = "admin"
    password = "admin"
    client_id = "dawning-admin"
    scope = "openid profile email roles api"
}

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/connect/token" -Method POST -Body $loginBody -ContentType "application/x-www-form-urlencoded"
$token = $loginResponse.access_token

Write-Host "登录成功，Token: $($token.Substring(0, 50))..." -ForegroundColor Green

# 准备HTTP头
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# 创建不同类型的系统日志
$logs = @(
    @{
        level = "Information"
        message = "用户成功登录系统"
        source = "AuthenticationService"
        username = "admin"
        ipAddress = "192.168.1.100"
        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
        requestPath = "/api/auth/login"
        requestMethod = "POST"
        statusCode = 200
    },
    @{
        level = "Information"
        message = "用户查询用户列表"
        source = "UserController"
        username = "admin"
        ipAddress = "192.168.1.100"
        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
        requestPath = "/api/user"
        requestMethod = "GET"
        statusCode = 200
    },
    @{
        level = "Warning"
        message = "用户尝试访问未授权的资源"
        source = "AuthorizationMiddleware"
        username = "guest"
        ipAddress = "192.168.1.105"
        userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)"
        requestPath = "/api/admin/settings"
        requestMethod = "GET"
        statusCode = 403
    },
    @{
        level = "Error"
        message = "数据库连接失败"
        exception = "MySql.Data.MySqlClient.MySqlException: Unable to connect to any of the specified MySQL hosts."
        stackTrace = "   at MySql.Data.MySqlClient.NativeDriver.Open()`n   at MySql.Data.MySqlClient.Driver.Open()`n   at MySql.Data.MySqlClient.MySqlConnection.Open()"
        source = "DatabaseService"
        username = "system"
        ipAddress = "127.0.0.1"
        userAgent = "Internal"
        requestPath = "/api/data/query"
        requestMethod = "POST"
        statusCode = 500
    },
    @{
        level = "Information"
        message = "用户创建新角色"
        source = "RoleController"
        username = "admin"
        ipAddress = "192.168.1.100"
        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
        requestPath = "/api/role"
        requestMethod = "POST"
        statusCode = 201
    },
    @{
        level = "Warning"
        message = "密码输入错误次数过多"
        source = "AuthenticationService"
        username = "testuser"
        ipAddress = "192.168.1.120"
        userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X)"
        requestPath = "/api/auth/login"
        requestMethod = "POST"
        statusCode = 401
    },
    @{
        level = "Error"
        message = "文件上传失败：文件大小超过限制"
        exception = "System.IO.IOException: The file exceeds the maximum allowed size of 10MB."
        source = "FileUploadService"
        username = "user1"
        ipAddress = "192.168.1.115"
        userAgent = "Mozilla/5.0 (X11; Linux x86_64) Chrome/91.0.4472.124"
        requestPath = "/api/file/upload"
        requestMethod = "POST"
        statusCode = 413
    },
    @{
        level = "Information"
        message = "系统配置更新成功"
        source = "SystemConfigController"
        username = "admin"
        ipAddress = "192.168.1.100"
        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/91.0.864.59"
        requestPath = "/api/config/update"
        requestMethod = "PUT"
        statusCode = 200
    },
    @{
        level = "Information"
        message = "用户登出系统"
        source = "AuthenticationService"
        username = "admin"
        ipAddress = "192.168.1.100"
        userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
        requestPath = "/api/auth/logout"
        requestMethod = "POST"
        statusCode = 200
    },
    @{
        level = "Warning"
        message = "API请求频率超过限制"
        source = "RateLimitMiddleware"
        username = "api_user"
        ipAddress = "203.0.113.50"
        userAgent = "Python/3.9 requests/2.26.0"
        requestPath = "/api/data/batch"
        requestMethod = "POST"
        statusCode = 429
    }
)

# 逐个创建日志
$successCount = 0
$failCount = 0

foreach ($log in $logs) {
    try {
        $body = $log | ConvertTo-Json
        Write-Host "`n正在创建日志: $($log.message)" -ForegroundColor Yellow
        
        $response = Invoke-RestMethod -Uri "$baseUrl/api/systemlog/test" `
            -Method POST `
            -Headers $headers `
            -Body $body
        
        Write-Host "  ✓ 创建成功 - Level: $($log.level), Source: $($log.source)" -ForegroundColor Green
        $successCount++
        
        # 稍微延迟，使日志时间戳有差异
        Start-Sleep -Milliseconds 200
    }
    catch {
        Write-Host "  ✗ 创建失败: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }
}

Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "日志创建完成！" -ForegroundColor Cyan
Write-Host "成功: $successCount 条" -ForegroundColor Green
Write-Host "失败: $failCount 条" -ForegroundColor Red
Write-Host "===============================================" -ForegroundColor Cyan
