# Dawning Identity API - Run Script
# Start backend development server

param(
    [switch]$Clean = $false
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Dawning Identity API - Run" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "$PSScriptRoot\Dawning.Gateway\src\Dawning.Identity.Api"

if ($Clean) {
    Write-Host "Cleaning and rebuilding..." -ForegroundColor Yellow
    dotnet clean $projectPath
    dotnet build $projectPath
    Write-Host ""
}

Write-Host "Starting Dawning Identity API..." -ForegroundColor Green
Write-Host "Listening on: http://localhost:5202" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5202/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Run project
Set-Location $projectPath
dotnet run
