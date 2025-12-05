# 简单登录测试
$body = "grant_type=password&client_id=dawning-admin&username=admin&password=admin123&scope=openid profile email roles api"

try {
    $response = Invoke-RestMethod `
        -Uri "http://localhost:5202/connect/token" `
        -Method POST `
        -Body $body `
        -ContentType "application/x-www-form-urlencoded" `
        -ErrorAction Stop
    
    Write-Host "✅ 登录成功！" -ForegroundColor Green
    Write-Host "Token: $($response.access_token.Substring(0,50))..." -ForegroundColor Cyan
    $response.access_token | Out-File "$env:TEMP\dawning_token.txt" -Encoding UTF8 -NoNewline
    
    # 测试获取用户列表
    $headers = @{ "Authorization" = "Bearer $($response.access_token)" }
    $users = Invoke-RestMethod -Uri "http://localhost:5202/api/user?pageIndex=1&pageSize=10" -Headers $headers
    Write-Host "`n✅ 用户列表获取成功！共 $($users.data.totalCount) 个用户" -ForegroundColor Green
    $users.data.items | ForEach-Object {
        Write-Host "  - $($_.username) ($($_.email))" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ 错误: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "详情: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
}
