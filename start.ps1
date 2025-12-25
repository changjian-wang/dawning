# =============================================
# Dawning Gateway - 一键启动脚本 (Windows)
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
        Write-Host "[1/2] 启动基础设施 (MySQL, Redis, Zookeeper, Kafka)..." -ForegroundColor Yellow
        docker-compose up -d mysql redis zookeeper kafka
        
        Write-Host "[2/2] 等待服务健康检查..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        docker-compose ps
        
        Write-Host ""
        Write-Host "基础设施启动完成！" -ForegroundColor Green
        Write-Host ""
        Write-Host "下一步:" -ForegroundColor Cyan
        Write-Host "  1. 启动后端: cd services\Dawning.Gateway\src\Dawning.Identity.Api && dotnet run"
        Write-Host "  2. 启动前端: cd dawning-admin && pnpm dev"
        Write-Host ""
        Write-Host "可选工具:" -ForegroundColor Cyan
        Write-Host "  Kafka UI: docker-compose --profile debug up -d kafka-ui"
        Write-Host "            访问: http://localhost:8080"
    }
    
    "dev" {
        Write-Host "[1/3] 启动基础设施..." -ForegroundColor Yellow
        docker-compose up -d mysql redis zookeeper kafka
        
        Write-Host "[2/3] 启动调试工具 (Kafka UI)..." -ForegroundColor Yellow
        docker-compose --profile debug up -d kafka-ui
        
        Write-Host "[3/3] 等待服务就绪..." -ForegroundColor Yellow
        Start-Sleep -Seconds 15
        docker-compose ps
        
        Write-Host ""
        Write-Host "开发环境启动完成！" -ForegroundColor Green
        Write-Host ""
        Write-Host "服务地址:" -ForegroundColor Cyan
        Write-Host "  MySQL:    localhost:3306 (user: dawning, pwd: dawning_password_2024)"
        Write-Host "  Redis:    localhost:6379"
        Write-Host "  Kafka:    localhost:9092"
        Write-Host "  Kafka UI: http://localhost:8080"
    }
    
    "all" {
        Write-Host "构建并启动所有服务..." -ForegroundColor Yellow
        docker-compose --profile debug up -d --build
        
        Write-Host ""
        Write-Host "所有服务启动完成！" -ForegroundColor Green
        Write-Host ""
        Write-Host "服务地址:" -ForegroundColor Cyan
        Write-Host "  前端:     http://localhost:80"
        Write-Host "  后端API:  http://localhost:5001"
        Write-Host "  网关API:  http://localhost:5000"
        Write-Host "  Kafka UI: http://localhost:8080"
    }
    
    "stop" {
        Write-Host "停止所有服务..." -ForegroundColor Yellow
        docker-compose --profile debug down
        Write-Host "所有服务已停止。" -ForegroundColor Green
    }
    
    "clean" {
        Write-Host "停止并清理所有数据..." -ForegroundColor Red
        $confirm = Read-Host "确定要删除所有数据卷吗? (y/N)"
        if ($confirm -eq "y" -or $confirm -eq "Y") {
            docker-compose --profile debug down -v
            Write-Host "所有服务和数据已清理。" -ForegroundColor Green
        } else {
            Write-Host "已取消。" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
