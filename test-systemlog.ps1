# ==========================================
# 系统日志功能测试脚本
# ==========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  系统日志功能测试" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$apiBase = "http://localhost:5202"

# 1. 检查数据库表
Write-Host "1. 检查 system_logs 表是否存在" -ForegroundColor Yellow
try {
    $result = mysql -u aluneth -p123456 dawning_identity -e "SHOW TABLES LIKE 'system_logs';" 2>$null
    if ($result -match "system_logs") {
        Write-Host "   ✓ system_logs 表已创建" -ForegroundColor Green
    } else {
        Write-Host "   ✗ system_logs 表不存在" -ForegroundColor Red
        exit
    }
} catch {
    Write-Host "   ✗ 数据库连接失败" -ForegroundColor Red
    exit
}
Write-Host ""

# 2. 查看表结构
Write-Host "2. 查看表结构" -ForegroundColor Yellow
mysql -u aluneth -p123456 dawning_identity -e "DESCRIBE system_logs;" 2>$null
Write-Host ""

# 3. 获取 token（使用 admin 账户）
Write-Host "3. 获取认证 token" -ForegroundColor Yellow
$tokenBody = @{
    grant_type = "password"
    client_id = "dawning-admin"
    client_secret = "dawning-admin-secret"
    scope = "openid profile email roles api"
    username = "admin"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $tokenResponse = Invoke-RestMethod -Uri "$apiBase/connect/token" -Method POST -Body $tokenBody -ContentType "application/json" -ErrorAction Stop
    $token = $tokenResponse.access_token
    Write-Host "   ✓ Token 获取成功" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Token 获取失败: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   提示: 请确保已初始化 admin 账户" -ForegroundColor Gray
    exit
}
Write-Host ""

# 4. 创建测试日志
Write-Host "4. 创建测试日志" -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

$testLog = @{
    level = "Information"
    message = "这是一条测试日志消息"
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$apiBase/api/systemlog/test" -Method POST -Headers $headers -Body $testLog -ErrorAction Stop
    Write-Host "   ✓ 测试日志创建成功" -ForegroundColor Green
    Write-Host "   日志ID: $($createResponse.data.id)" -ForegroundColor Cyan
} catch {
    Write-Host "   ✗ 创建日志失败: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 5. 查询日志列表
Write-Host "5. 查询日志列表" -ForegroundColor Yellow
try {
    $logsResponse = Invoke-RestMethod -Uri "$apiBase/api/systemlog/paged?page=1&pageSize=10" -Method GET -Headers $headers -ErrorAction Stop
    $logCount = $logsResponse.data.items.Count
    Write-Host "   ✓ 查询成功，共 $logCount 条日志" -ForegroundColor Green
    
    if ($logCount -gt 0) {
        Write-Host ""
        Write-Host "   最近的日志:" -ForegroundColor Cyan
        $logsResponse.data.items | Select-Object -First 3 | ForEach-Object {
            Write-Host "   - [$($_.level)] $($_.message) ($($ ($_.createdAt))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "   ✗ 查询日志失败: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 6. 直接查询数据库
Write-Host "6. 直接查询数据库中的日志记录" -ForegroundColor Yellow
mysql -u aluneth -p123456 dawning_identity -e "SELECT id, level, LEFT(message, 40) as message, created_at FROM system_logs ORDER BY created_at DESC LIMIT 5;" 2>$null
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  测试完成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
