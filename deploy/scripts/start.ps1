# =============================================
# Dawning Gateway - Quick Start Script (Windows)
# =============================================

param(
    [ValidateSet("infra", "dev", "all", "stop", "clean")]
    [string]$Mode = "infra"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Dawning Gateway Management System" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

switch ($Mode) {
    "infra" {
        Write-Host "[1/2] Starting infrastructure (MySQL, Redis, Zookeeper, Kafka)..." -ForegroundColor Yellow
        docker-compose up -d mysql redis zookeeper kafka
        
        Write-Host "[2/2] Waiting for health checks..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        docker-compose ps
        
        Write-Host ""
        Write-Host "Infrastructure started successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  1. Start backend: cd services\Dawning.Gateway\src\Dawning.Identity.Api && dotnet run"
        Write-Host "  2. Start frontend: cd dawning-admin && pnpm dev"
        Write-Host ""
        Write-Host "Optional tools:" -ForegroundColor Cyan
        Write-Host "  Kafka UI: docker-compose --profile debug up -d kafka-ui"
        Write-Host "            Access: http://localhost:8080"
    }
    
    "dev" {
        Write-Host "[1/3] Starting infrastructure..." -ForegroundColor Yellow
        docker-compose up -d mysql redis zookeeper kafka
        
        Write-Host "[2/3] Starting debug tools (Kafka UI)..." -ForegroundColor Yellow
        docker-compose --profile debug up -d kafka-ui
        
        Write-Host "[3/3] Waiting for services to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 15
        docker-compose ps
        
        Write-Host ""
        Write-Host "Development environment started successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Service addresses:" -ForegroundColor Cyan
        Write-Host "  MySQL:    localhost:3306 (user: dawning, pwd: dawning_password_2024)"
        Write-Host "  Redis:    localhost:6379"
        Write-Host "  Kafka:    localhost:9092"
        Write-Host "  Kafka UI: http://localhost:8080"
    }
    
    "all" {
        Write-Host "Building and starting all services..." -ForegroundColor Yellow
        docker-compose --profile debug up -d --build
        
        Write-Host ""
        Write-Host "All services started successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Service addresses:" -ForegroundColor Cyan
        Write-Host "  Frontend:    http://localhost:80"
        Write-Host "  Backend API: http://localhost:5001"
        Write-Host "  Gateway API: http://localhost:5000"
        Write-Host "  Kafka UI:    http://localhost:8080"
    }
    
    "stop" {
        Write-Host "Stopping all services..." -ForegroundColor Yellow
        docker-compose --profile debug down
        Write-Host "All services stopped." -ForegroundColor Green
    }
    
    "clean" {
        Write-Host "Stopping and cleaning all data..." -ForegroundColor Red
        $confirm = Read-Host "Are you sure you want to delete all volumes? (y/N)"
        if ($confirm -eq "y" -or $confirm -eq "Y") {
            docker-compose --profile debug down -v
            Write-Host "All services and data cleaned." -ForegroundColor Green
        } else {
            Write-Host "Cancelled." -ForegroundColor Yellow
        }
    }
}

Write-Host ""
