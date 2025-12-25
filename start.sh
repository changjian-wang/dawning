#!/bin/bash
# =============================================
# Dawning Gateway - 一键启动脚本 (Linux/Mac)
# =============================================

set -e

MODE=${1:-infra}

echo "========================================"
echo "  Dawning Gateway Management System"
echo "========================================"
echo ""

case $MODE in
    "infra")
        echo "[1/2] 启动基础设施 (MySQL, Redis, Zookeeper, Kafka)..."
        docker-compose up -d mysql redis zookeeper kafka
        
        echo "[2/2] 等待服务健康检查..."
        sleep 10
        docker-compose ps
        
        echo ""
        echo "✅ 基础设施启动完成！"
        echo ""
        echo "下一步:"
        echo "  1. 启动后端: cd services/Dawning.Gateway/src/Dawning.Identity.Api && dotnet run"
        echo "  2. 启动前端: cd dawning-admin && pnpm dev"
        echo ""
        echo "可选工具:"
        echo "  Kafka UI: docker-compose --profile debug up -d kafka-ui"
        echo "            访问: http://localhost:8080"
        ;;
    
    "dev")
        echo "[1/3] 启动基础设施..."
        docker-compose up -d mysql redis zookeeper kafka
        
        echo "[2/3] 启动调试工具 (Kafka UI)..."
        docker-compose --profile debug up -d kafka-ui
        
        echo "[3/3] 等待服务就绪..."
        sleep 15
        docker-compose ps
        
        echo ""
        echo "✅ 开发环境启动完成！"
        echo ""
        echo "服务地址:"
        echo "  MySQL:    localhost:3306 (user: dawning, pwd: dawning_password_2024)"
        echo "  Redis:    localhost:6379"
        echo "  Kafka:    localhost:9092"
        echo "  Kafka UI: http://localhost:8080"
        ;;
    
    "all")
        echo "构建并启动所有服务..."
        docker-compose --profile debug up -d --build
        
        echo ""
        echo "✅ 所有服务启动完成！"
        echo ""
        echo "服务地址:"
        echo "  前端:     http://localhost:80"
        echo "  后端API:  http://localhost:5001"
        echo "  网关API:  http://localhost:5000"
        echo "  Kafka UI: http://localhost:8080"
        ;;
    
    "stop")
        echo "停止所有服务..."
        docker-compose --profile debug down
        echo "✅ 所有服务已停止。"
        ;;
    
    "clean")
        echo "⚠️  停止并清理所有数据..."
        read -p "确定要删除所有数据卷吗? (y/N): " confirm
        if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
            docker-compose --profile debug down -v
            echo "✅ 所有服务和数据已清理。"
        else
            echo "已取消。"
        fi
        ;;
    
    *)
        echo "用法: ./start.sh [infra|dev|all|stop|clean]"
        echo ""
        echo "模式:"
        echo "  infra - 仅启动基础设施 (默认)"
        echo "  dev   - 启动基础设施 + 调试工具"
        echo "  all   - 构建并启动所有服务"
        echo "  stop  - 停止所有服务"
        echo "  clean - 停止并删除所有数据"
        ;;
esac

echo ""
