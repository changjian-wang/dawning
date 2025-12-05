# 用户管理API测试脚本
$baseUrl = "http://localhost:5202"
$tokenFile = "$env:TEMP\dawning_token.txt"

Write-Host "=== Dawning Identity User Management API Test ===" -ForegroundColor Cyan
Write-Host ""

# 1. 登录获取token
Write-Host "[1/8] 登录获取Token..." -ForegroundColor Yellow
$loginBody = "grant_type=password&client_id=dawning-admin&username=admin&password=admin123&scope=openid profile email roles api"
try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/connect/token" -Method POST -Body $loginBody -ContentType "application/x-www-form-urlencoded"
    $token = $loginResponse.access_token
    $token | Out-File -FilePath $tokenFile -Encoding UTF8 -NoNewline
    Write-Host "✅ Token获取成功" -ForegroundColor Green
    Write-Host "   Token: $($token.Substring(0,50))..." -ForegroundColor Gray
} catch {
    Write-Host "❌ 登录失败: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host ""

# 2. 获取用户列表
Write-Host "[2/8] 获取用户列表..." -ForegroundColor Yellow
try {
    $usersResponse = Invoke-RestMethod -Uri "$baseUrl/api/user?pageIndex=1&pageSize=10" -Method GET -Headers $headers
    Write-Host "✅ 获取成功，共 $($usersResponse.data.totalCount) 个用户" -ForegroundColor Green
    $usersResponse.data.items | ForEach-Object {
        Write-Host "   - $($_.username) ($($_.email)) [$($_.role)]" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ 获取失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 3. 检查用户名可用性
Write-Host "[3/8] 检查用户名可用性..." -ForegroundColor Yellow
try {
    $checkUsername = Invoke-RestMethod -Uri "$baseUrl/api/user/check-username?username=testuser" -Method GET -Headers $headers
    Write-Host "✅ testuser 可用: $($checkUsername.data)" -ForegroundColor Green
} catch {
    Write-Host "❌ 检查失败: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 4. 创建新用户
Write-Host "[4/8] 创建新用户..." -ForegroundColor Yellow
$newUser = @{
    username = "testuser"
    password = "Test@123"
    email = "testuser@dawning.com"
    phoneNumber = "13800138000"
    displayName = "测试用户"
    role = "user"
    remark = "通过API测试创建"
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/user" -Method POST -Headers $headers -Body $newUser
    $newUserId = $createResponse.data.id
    Write-Host "✅ 创建成功，用户ID: $newUserId" -ForegroundColor Green
    Write-Host "   Username: $($createResponse.data.username)" -ForegroundColor Gray
    Write-Host "   Email: $($createResponse.data.email)" -ForegroundColor Gray
} catch {
    Write-Host "❌ 创建失败: $($_.Exception.Message)" -ForegroundColor Red
    $newUserId = $null
}

Write-Host ""

# 5. 获取单个用户
if ($newUserId) {
    Write-Host "[5/8] 获取用户详情 ($newUserId)..." -ForegroundColor Yellow
    try {
        $userDetail = Invoke-RestMethod -Uri "$baseUrl/api/user/$newUserId" -Method GET -Headers $headers
        Write-Host "✅ 获取成功" -ForegroundColor Green
        Write-Host "   DisplayName: $($userDetail.data.displayName)" -ForegroundColor Gray
        Write-Host "   PhoneNumber: $($userDetail.data.phoneNumber)" -ForegroundColor Gray
        Write-Host "   CreatedAt: $($userDetail.data.createdAt)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ 获取失败: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[5/8] ⏭️  跳过（用户创建失败）" -ForegroundColor Gray
}

Write-Host ""

# 6. 更新用户
if ($newUserId) {
    Write-Host "[6/8] 更新用户信息..." -ForegroundColor Yellow
    $updateUser = @{
        displayName = "测试用户（已更新）"
        phoneNumber = "13900139000"
        remark = "通过API测试更新"
    } | ConvertTo-Json
    
    try {
        $updateResponse = Invoke-RestMethod -Uri "$baseUrl/api/user/$newUserId" -Method PUT -Headers $headers -Body $updateUser
        Write-Host "✅ 更新成功" -ForegroundColor Green
        Write-Host "   DisplayName: $($updateResponse.data.displayName)" -ForegroundColor Gray
        Write-Host "   PhoneNumber: $($updateResponse.data.phoneNumber)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ 更新失败: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[6/8] ⏭️  跳过（用户创建失败）" -ForegroundColor Gray
}

Write-Host ""

# 7. 修改密码
if ($newUserId) {
    Write-Host "[7/8] 修改用户密码..." -ForegroundColor Yellow
    $changePassword = @{
        userId = $newUserId
        oldPassword = "Test@123"
        newPassword = "NewPass@456"
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/user/change-password" -Method POST -Headers $headers -Body $changePassword | Out-Null
        Write-Host "✅ 密码修改成功" -ForegroundColor Green
    } catch {
        Write-Host "❌ 密码修改失败: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[7/8] ⏭️  跳过（用户创建失败）" -ForegroundColor Gray
}

Write-Host ""

# 8. 删除用户（软删除）
if ($newUserId) {
    Write-Host "[8/8] 删除用户 (软删除)..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/user/$newUserId" -Method DELETE -Headers $headers | Out-Null
        Write-Host "✅ 删除成功" -ForegroundColor Green
        
        # 验证软删除
        Write-Host "   验证软删除..." -ForegroundColor Gray
        $verifyResponse = Invoke-RestMethod -Uri "$baseUrl/api/user?pageIndex=1&pageSize=10&includeDeleted=true" -Method GET -Headers $headers
        $deletedUser = $verifyResponse.data.items | Where-Object { $_.id -eq $newUserId }
        if ($deletedUser -and $deletedUser.isDeleted) {
            Write-Host "   ✓ 用户已标记为删除 (is_deleted=true)" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  未找到删除标记" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ 删除失败: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[8/8] ⏭️  跳过（用户创建失败）" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== 测试完成 ===" -ForegroundColor Cyan
