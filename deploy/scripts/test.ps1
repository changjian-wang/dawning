# Dawning Identity API - Test Script
# Run all unit tests

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Dawning Identity API - Unit Tests" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "$PSScriptRoot\Dawning.Gateway\src\Dawning.Identity.Application.Tests"

Write-Host "Running unit tests..." -ForegroundColor Green
Write-Host ""

# Run tests
dotnet test $projectPath --logger "console;verbosity=normal"

$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "✗ Tests failed" -ForegroundColor Red
}

Write-Host ""
exit $exitCode
