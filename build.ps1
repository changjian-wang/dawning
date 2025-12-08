# Dawning Identity API - 构建脚本
# 编译整个解决方案

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Dawning Identity API - 构建" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$solutionPath = "$PSScriptRoot\Dawning.Gateway\Dawning.Gateway.sln"

Write-Host "清理旧的构建文件..." -ForegroundColor Yellow
dotnet clean $solutionPath --configuration Release

Write-Host ""
Write-Host "开始构建..." -ForegroundColor Green
dotnet build $solutionPath --configuration Release

$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "✓ 构建成功!" -ForegroundColor Green
} else {
    Write-Host "✗ 构建失败" -ForegroundColor Red
}

Write-Host ""
exit $exitCode
