# 测试 Dawning Identity API

Write-Host "Starting backend..." -ForegroundColor Green
$backend = Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd 'c:\github\dawning\Dawning.Gateway\src\Dawning.Identity.Api'; dotnet run"
) -PassThru -WindowStyle Normal

Write-Host "Waiting for backend to start (15 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`nTesting dev-reset-admin endpoint..." -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5202/api/dev/dev-reset-admin" -Method POST
    Write-Host "Success!" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5)
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host $reader.ReadToEnd() -ForegroundColor Red
    }
}

Write-Host "`nPress any key to stop backend and exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Stop-Process -Id $backend.Id -Force
Write-Host "Backend stopped." -ForegroundColor Yellow
