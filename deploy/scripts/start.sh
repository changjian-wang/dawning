#!/bin/bash
# =============================================
# Dawning Gateway - Quick Start Script (Linux/Mac)
# =============================================

set -e

MODE=${1:-infra}

echo "========================================"
echo "  Dawning Gateway Management System"
echo "========================================"
echo ""

case $MODE in
    "infra")
        echo "[1/2] Starting infrastructure (MySQL, Redis, Zookeeper, Kafka)..."
        docker-compose up -d mysql redis zookeeper kafka
        
        echo "[2/2] Waiting for health checks..."
        sleep 10
        docker-compose ps
        
        echo ""
        echo "✅ Infrastructure started successfully!"
        echo ""
        echo "Next steps:"
        echo "  1. Start backend: cd services/Dawning.Gateway/src/Dawning.Identity.Api && dotnet run"
        echo "  2. Start frontend: cd dawning-admin && pnpm dev"
        echo ""
        echo "Optional tools:"
        echo "  Kafka UI: docker-compose --profile debug up -d kafka-ui"
        echo "            Access: http://localhost:8080"
        ;;
    
    "dev")
        echo "[1/3] Starting infrastructure..."
        docker-compose up -d mysql redis zookeeper kafka
        
        echo "[2/3] Starting debug tools (Kafka UI)..."
        docker-compose --profile debug up -d kafka-ui
        
        echo "[3/3] Waiting for services to be ready..."
        sleep 15
        docker-compose ps
        
        echo ""
        echo "✅ Development environment started successfully!"
        echo ""
        echo "Service addresses:"
        echo "  MySQL:    localhost:3306 (user: dawning, pwd: dawning_password_2024)"
        echo "  Redis:    localhost:6379"
        echo "  Kafka:    localhost:9092"
        echo "  Kafka UI: http://localhost:8080"
        ;;
    
    "all")
        echo "Building and starting all services..."
        docker-compose --profile debug up -d --build
        
        echo ""
        echo "✅ All services started successfully!"
        echo ""
        echo "Service addresses:"
        echo "  Frontend:    http://localhost:80"
        echo "  Backend API: http://localhost:5001"
        echo "  Gateway API: http://localhost:5000"
        echo "  Kafka UI:    http://localhost:8080"
        ;;
    
    "stop")
        echo "Stopping all services..."
        docker-compose --profile debug down
        echo "✅ All services stopped."
        ;;
    
    "clean")
        echo "⚠️  Stopping and cleaning all data..."
        read -p "Are you sure you want to delete all volumes? (y/N): " confirm
        if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
            docker-compose --profile debug down -v
            echo "✅ All services and data cleaned."
        else
            echo "Cancelled."
        fi
        ;;
    
    *)
        echo "Usage: ./start.sh [infra|dev|all|stop|clean]"
        echo ""
        echo "Modes:"
        echo "  infra - Start infrastructure only (default)"
        echo "  dev   - Start infrastructure + debug tools"
        echo "  all   - Build and start all services"
        echo "  stop  - Stop all services"
        echo "  clean - Stop and delete all data"
        ;;
esac

echo ""
