# Dawning Identity API - Build Script
# Build the entire solution

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Dawning Identity API - Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$solutionPath = "$PSScriptRoot\Dawning.Gateway\Dawning.Gateway.sln"

Write-Host "Cleaning old build files..." -ForegroundColor Yellow
dotnet clean $solutionPath --configuration Release

Write-Host ""
Write-Host "Building..." -ForegroundColor Green
dotnet build $solutionPath --configuration Release

$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "✓ Build succeeded!" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
}

Write-Host ""
exit $exitCode
