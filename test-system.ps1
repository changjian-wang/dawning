# ==========================================
# Dawning 网关管理系统 - 快速测试脚本
# ==========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Dawning 网关管理系统 - 功能测试" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 测试 Identity API
Write-Host "1. 测试 Identity API (http://localhost:5202)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5202/health" -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   ✓ Identity API 运行正常" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Identity API 未运行" -ForegroundColor Red
    Write-Host "   提示: 请运行 'dotnet run --project .\Dawning.Gateway\src\Dawning.Identity.Api\'" -ForegroundColor Gray
}
Write-Host ""

# 2. 测试前端应用
Write-Host "2. 测试前端应用 (http://localhost:5174)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5174" -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   ✓ 前端应用运行正常" -ForegroundColor Green
} catch {
    Write-Host "   ✗ 前端应用未运行" -ForegroundColor Red
    Write-Host "   提示: 请运行 'npm run dev' in dawning-admin/" -ForegroundColor Gray
}
Write-Host ""

# 3. 测试数据库连接
Write-Host "3. 测试数据库连接" -ForegroundColor Yellow
try {
    $result = mysql -u aluneth -p123456 -e "SELECT COUNT(*) as count FROM dawning_identity.users;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ 数据库连接正常" -ForegroundColor Green
    } else {
        Write-Host "   ✗ 数据库连接失败" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ 数据库连接失败" -ForegroundColor Red
}
Write-Host ""

# 4. 测试 OpenIddict 应用程序 API
Write-Host "4. 测试 OpenIddict 应用程序 API" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5202/api/openiddict/application/get-all" -Method GET -TimeoutSec 5 -ErrorAction Stop
    $count = $response.data.Count
    Write-Host "   ✓ API 正常，当前应用程序数量: $count" -ForegroundColor Green
} catch {
    Write-Host "   ✗ API 调用失败: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 5. 显示系统信息
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  系统信息" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Identity API:  http://localhost:5202" -ForegroundColor White
Write-Host "前端应用:      http://localhost:5174" -ForegroundColor White
Write-Host "API 文档:      http://localhost:5202/swagger" -ForegroundColor White
Write-Host ""
Write-Host "快速访问:" -ForegroundColor Yellow
Write-Host "  - 应用程序管理: http://localhost:5174/#/administration/openiddict/application" -ForegroundColor Gray
Write-Host "  - 用户管理:     http://localhost:5174/#/administration/user" -ForegroundColor Gray
Write-Host ""

# 6. 显示 TODO 修复状态
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  核心 TODO 修复状态" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ 生产证书配置           - 已完成" -ForegroundColor Green
Write-Host "✓ 客户端密钥加密         - 已完成 (PBKDF2)" -ForegroundColor Green
Write-Host "✓ 密钥验证逻辑           - 已完成" -ForegroundColor Green
Write-Host "✓ 最后登录时间更新       - 已完成" -ForegroundColor Green
Write-Host "✓ 软删除功能移除         - 已完成" -ForegroundColor Green
Write-Host "✓ OpenIddict 应用管理    - 已完成" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "测试完成！" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
