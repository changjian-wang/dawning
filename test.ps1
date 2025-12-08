# Dawning Identity API - 测试脚本
# 运行所有单元测试

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Dawning Identity API - 单元测试" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "$PSScriptRoot\Dawning.Gateway\src\Dawning.Identity.Application.Tests"

Write-Host "运行单元测试..." -ForegroundColor Green
Write-Host ""

# 运行测试
dotnet test $projectPath --logger "console;verbosity=normal"

$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "✓ 所有测试通过!" -ForegroundColor Green
} else {
    Write-Host "✗ 测试失败" -ForegroundColor Red
}

Write-Host ""
exit $exitCode
