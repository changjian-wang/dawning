# Phase 3 分页配置测试脚本
# 测试自定义 PagedOptions 配置功能

Write-Host "=== Phase 3: 分页配置测试 ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5202/api/user"

Write-Host "测试 1: 使用自定义配置（MaxPageNumber=5000）" -ForegroundColor Yellow
Write-Host "请求: GET $baseUrl/custom-config?page=1&pageSize=2&maxPageNumber=5000"
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/custom-config?page=1&pageSize=2&maxPageNumber=5000" -Method Get -ContentType "application/json"
    
    Write-Host "✅ 响应成功" -ForegroundColor Green
    Write-Host "Code: $($response.code)"
    Write-Host "Message: $($response.message)"
    Write-Host ""
    
    Write-Host "📋 配置信息:" -ForegroundColor Cyan
    Write-Host "  MaxPageNumber: $($response.config.maxPageNumber)"
    Write-Host "  MaxCursorPageSize: $($response.config.maxCursorPageSize)"
    Write-Host "  DefaultPageSize: $($response.config.defaultPageSize)"
    Write-Host ""
    
    Write-Host "📄 分页信息:" -ForegroundColor Cyan
    Write-Host "  Page: $($response.data.pagination.page)"
    Write-Host "  PageSize: $($response.data.pagination.pageSize)"
    Write-Host "  Total: $($response.data.pagination.total)"
    Write-Host "  返回用户数: $($response.data.list.Count)"
    Write-Host ""
    
    if ($response.data.list.Count -gt 0) {
        Write-Host "🔍 第一个用户:" -ForegroundColor Cyan
        $user = $response.data.list[0]
        Write-Host "  ID: $($user.id)"
        Write-Host "  Username: $($user.username)"
        Write-Host "  Email: $($user.email)"
    }
    
} catch {
    Write-Host "❌ 测试失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "测试 2: 测试自定义游标分页大小限制" -ForegroundColor Yellow
Write-Host "请求: GET $baseUrl/custom-config?page=1&pageSize=3&maxCursorPageSize=500"
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/custom-config?page=1&pageSize=3&maxCursorPageSize=500" -Method Get -ContentType "application/json"
    
    Write-Host "✅ 响应成功" -ForegroundColor Green
    Write-Host "返回用户数: $($response.data.list.Count)"
    Write-Host "MaxCursorPageSize 配置: $($response.config.maxCursorPageSize)"
    Write-Host ""
    
} catch {
    Write-Host "❌ 测试失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "测试 3: 对比标准分页端点（使用默认配置）" -ForegroundColor Yellow
Write-Host "请求: GET $baseUrl?page=1&pageSize=2"
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "$baseUrl?page=1&pageSize=2" -Method Get -ContentType "application/json"
    
    Write-Host "✅ 响应成功（标准端点使用 MaxPageNumber=10000 默认值）" -ForegroundColor Green
    Write-Host "返回用户数: $($response.data.list.Count)"
    Write-Host ""
    
} catch {
    Write-Host "❌ 测试失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Phase 3 测试完成 ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Phase 3 主要特性:" -ForegroundColor Green
Write-Host "  1. PagedOptions 配置类支持自定义分页行为"
Write-Host "  2. MaxPageNumber 可按应用需求调整（默认10000）"
Write-Host "  3. MaxCursorPageSize 可配置游标分页限制（默认1000）"
Write-Host "  4. DefaultPageSize 可设置默认页大小（默认10）"
Write-Host "  5. 向后兼容：原有端点无需修改继续使用默认配置"
Write-Host ""
