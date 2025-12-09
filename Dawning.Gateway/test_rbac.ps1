# RBAC Testing Script
# Test Role-Based Access Control implementation

$baseUrl = "http://localhost:5202"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RBAC Testing Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Login as admin user
Write-Host "Test 1: Login as admin user" -ForegroundColor Yellow
$adminLoginBody = @{
    grant_type = "password"
    username = "admin"
    password = "admin"
    scope = "openid profile email api"
} | ConvertTo-Json

try {
    $adminResponse = Invoke-RestMethod -Uri "$baseUrl/connect/token" -Method Post -Body $adminLoginBody -ContentType "application/json"
    $adminToken = $adminResponse.access_token
    Write-Host "✓ Admin login successful" -ForegroundColor Green
    Write-Host "  Token: $($adminToken.Substring(0, 50))..." -ForegroundColor Gray
    
    # Decode JWT to check roles
    $tokenParts = $adminToken.Split('.')
    $payload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($tokenParts[1] + "=="))
    Write-Host "  Token Payload (excerpt):" -ForegroundColor Gray
    $payloadObj = $payload | ConvertFrom-Json
    Write-Host "    sub: $($payloadObj.sub)" -ForegroundColor Gray
    Write-Host "    name: $($payloadObj.name)" -ForegroundColor Gray
    Write-Host "    role: $($payloadObj.role)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Admin login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Admin can access role management endpoint
Write-Host "Test 2: Admin accesses role management (should succeed)" -ForegroundColor Yellow
$adminHeaders = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

try {
    $roles = Invoke-RestMethod -Uri "$baseUrl/api/role/all" -Method Get -Headers $adminHeaders
    Write-Host "✓ Admin can access role management" -ForegroundColor Green
    Write-Host "  Found $($roles.data.Count) roles" -ForegroundColor Gray
    foreach ($role in $roles.data) {
        Write-Host "    - $($role.name) ($($role.displayName))" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Admin role access failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Admin can list users
Write-Host "Test 3: Admin accesses user list (should succeed)" -ForegroundColor Yellow
try {
    $users = Invoke-RestMethod -Uri "$baseUrl/api/user?page=1&pageSize=5" -Method Get -Headers $adminHeaders
    Write-Host "✓ Admin can list users" -ForegroundColor Green
    Write-Host "  Found $($users.data.total) total users" -ForegroundColor Gray
} catch {
    Write-Host "✗ Admin user list access failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Login as regular user
Write-Host "Test 4: Login as regular user (zhangsan)" -ForegroundColor Yellow
$userLoginBody = @{
    grant_type = "password"
    username = "zhangsan"
    password = "123456"
    scope = "openid profile email api"
} | ConvertTo-Json

try {
    $userResponse = Invoke-RestMethod -Uri "$baseUrl/connect/token" -Method Post -Body $userLoginBody -ContentType "application/json"
    $userToken = $userResponse.access_token
    Write-Host "✓ Regular user login successful" -ForegroundColor Green
    Write-Host "  Token: $($userToken.Substring(0, 50))..." -ForegroundColor Gray
    
    # Decode JWT to check roles
    $tokenParts = $userToken.Split('.')
    $payload = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($tokenParts[1] + "=="))
    Write-Host "  Token Payload (excerpt):" -ForegroundColor Gray
    $payloadObj = $payload | ConvertFrom-Json
    Write-Host "    sub: $($payloadObj.sub)" -ForegroundColor Gray
    Write-Host "    name: $($payloadObj.name)" -ForegroundColor Gray
    Write-Host "    role: $($payloadObj.role)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Regular user login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 5: Regular user tries to access role management (should fail)
Write-Host "Test 5: Regular user tries to access role management (should fail with 403)" -ForegroundColor Yellow
$userHeaders = @{
    "Authorization" = "Bearer $userToken"
    "Content-Type" = "application/json"
}

try {
    $roles = Invoke-RestMethod -Uri "$baseUrl/api/role/all" -Method Get -Headers $userHeaders
    Write-Host "✗ Regular user should NOT be able to access role management!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403 -or $_.Exception.Response.StatusCode.value__ -eq 403) {
        Write-Host "✓ Regular user correctly blocked from role management (403 Forbidden)" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""

# Test 6: Regular user tries to list users (should fail)
Write-Host "Test 6: Regular user tries to list users (should fail with 403)" -ForegroundColor Yellow
try {
    $users = Invoke-RestMethod -Uri "$baseUrl/api/user?page=1&pageSize=5" -Method Get -Headers $userHeaders
    Write-Host "✗ Regular user should NOT be able to list users!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403 -or $_.Exception.Response.StatusCode.value__ -eq 403) {
        Write-Host "✓ Regular user correctly blocked from listing users (403 Forbidden)" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""

# Test 7: Regular user can access their own info
Write-Host "Test 7: Regular user accesses own info (should succeed)" -ForegroundColor Yellow
try {
    $userInfo = Invoke-RestMethod -Uri "$baseUrl/api/user/info" -Method Get -Headers $userHeaders
    Write-Host "✓ Regular user can access own info" -ForegroundColor Green
    Write-Host "  Username: $($userInfo.data.username)" -ForegroundColor Gray
    Write-Host "  Roles: $($userInfo.data.roles -join ', ')" -ForegroundColor Gray
} catch {
    Write-Host "✗ Regular user info access failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 8: Admin can create user (should succeed)
Write-Host "Test 8: Admin creates new user (should succeed)" -ForegroundColor Yellow
$newUserData = @{
    username = "testuser_rbac_$(Get-Random -Maximum 9999)"
    password = "Test123456"
    email = "testuser@example.com"
    role = "user"
    isActive = $true
} | ConvertTo-Json

try {
    $createResult = Invoke-RestMethod -Uri "$baseUrl/api/user" -Method Post -Headers $adminHeaders -Body $newUserData
    Write-Host "✓ Admin successfully created user" -ForegroundColor Green
    Write-Host "  User ID: $($createResult.data.id)" -ForegroundColor Gray
    Write-Host "  Username: $($createResult.data.username)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Admin user creation failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 9: Regular user tries to create user (should fail)
Write-Host "Test 9: Regular user tries to create user (should fail with 403)" -ForegroundColor Yellow
$newUserData2 = @{
    username = "testuser2_$(Get-Random -Maximum 9999)"
    password = "Test123456"
    email = "testuser2@example.com"
    role = "user"
    isActive = $true
} | ConvertTo-Json

try {
    $createResult = Invoke-RestMethod -Uri "$baseUrl/api/user" -Method Post -Headers $userHeaders -Body $newUserData2
    Write-Host "✗ Regular user should NOT be able to create users!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 403 -or $_.Exception.Response.StatusCode.value__ -eq 403) {
        Write-Host "✓ Regular user correctly blocked from creating users (403 Forbidden)" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RBAC Testing Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
