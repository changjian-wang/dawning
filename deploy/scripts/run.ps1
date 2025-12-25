# Dawning Identity API - 运行脚本
# 启动后端开发服务器

param(
    [switch]$Clean = $false
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Dawning Identity API - 运行" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "$PSScriptRoot\Dawning.Gateway\src\Dawning.Identity.Api"

if ($Clean) {
    Write-Host "清理并重新构建..." -ForegroundColor Yellow
    dotnet clean $projectPath
    dotnet build $projectPath
    Write-Host ""
}

Write-Host "启动 Dawning Identity API..." -ForegroundColor Green
Write-Host "监听地址: http://localhost:5202" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5202/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "按 Ctrl+C 停止服务器" -ForegroundColor Yellow
Write-Host ""

# 运行项目
Set-Location $projectPath
dotnet run
