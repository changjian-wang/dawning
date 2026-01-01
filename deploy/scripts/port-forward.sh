#!/bin/bash

# ============================================================================
# Dawning K8s 端口转发脚本
# 用于将 K8s 服务端口转发到本地，方便开发调试
# ============================================================================

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 默认配置
NAMESPACE="${NAMESPACE:-dawning}"
OVERLAY="${OVERLAY:-dev}"

# 根据 overlay 确定服务名前缀
case "$OVERLAY" in
  dev)
    PREFIX="dev-"
    ;;
  staging)
    PREFIX="staging-"
    ;;
  prod)
    PREFIX="prod-"
    ;;
  *)
    PREFIX=""
    ;;
esac

# 服务端口配置（与 Docker Compose 保持一致）
IDENTITY_API_PORT="${IDENTITY_API_PORT:-5001}"
GATEWAY_API_PORT="${GATEWAY_API_PORT:-5000}"
FRONTEND_PORT="${FRONTEND_PORT:-8088}"

# PID 文件存储目录
PID_DIR="/tmp/dawning-port-forward"

print_banner() {
    echo -e "${BLUE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║           Dawning K8s 端口转发管理器                         ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_info() {
    echo -e "${BLUE}[i]${NC} $1"
}

# 检查 kubectl 是否可用
check_kubectl() {
    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl 未安装，请先安装 kubectl"
        exit 1
    fi
}

# 检查集群连接
check_cluster() {
    if ! kubectl cluster-info &> /dev/null; then
        print_error "无法连接到 Kubernetes 集群"
        exit 1
    fi
}

# 检查 namespace 是否存在
check_namespace() {
    if ! kubectl get namespace "$NAMESPACE" &> /dev/null; then
        print_error "Namespace '$NAMESPACE' 不存在"
        exit 1
    fi
}

# 检查服务是否存在
check_service() {
    local service=$1
    if ! kubectl get svc "$service" -n "$NAMESPACE" &> /dev/null; then
        return 1
    fi
    return 0
}

# 检查 Pod 是否就绪
check_pod_ready() {
    local service=$1
    local app_label="${service#$PREFIX}"  # 移除前缀获取 app label
    local ready_pods=$(kubectl get pods -n "$NAMESPACE" -l "app=$app_label" -o jsonpath='{.items[*].status.conditions[?(@.type=="Ready")].status}' 2>/dev/null | grep -c "True" || echo "0")
    
    if [ "$ready_pods" -gt 0 ]; then
        return 0
    fi
    return 1
}

# 启动单个端口转发
start_port_forward() {
    local service=$1
    local local_port=$2
    local remote_port=$3
    local pid_file="$PID_DIR/${service}.pid"
    
    # 检查服务是否存在
    if ! check_service "$service"; then
        print_warning "服务 $service 不存在，跳过"
        return 1
    fi
    
    # 检查 Pod 是否就绪
    if ! check_pod_ready "$service"; then
        print_warning "服务 $service 的 Pod 未就绪，跳过"
        return 1
    fi
    
    # 检查是否已经在运行
    if [ -f "$pid_file" ]; then
        local old_pid=$(cat "$pid_file")
        if ps -p "$old_pid" &> /dev/null; then
            print_warning "端口转发 $service ($local_port -> $remote_port) 已在运行 (PID: $old_pid)"
            return 0
        fi
    fi
    
    # 检查端口是否被占用
    if lsof -i ":$local_port" &> /dev/null; then
        print_warning "本地端口 $local_port 已被占用"
        return 1
    fi
    
    # 启动端口转发
    kubectl port-forward "svc/$service" "$local_port:$remote_port" -n "$NAMESPACE" &> /dev/null &
    local pid=$!
    echo "$pid" > "$pid_file"
    
    # 等待端口转发就绪
    sleep 1
    if ps -p "$pid" &> /dev/null; then
        print_status "已启动 $service: localhost:$local_port -> $remote_port (PID: $pid)"
        return 0
    else
        print_error "启动 $service 端口转发失败"
        rm -f "$pid_file"
        return 1
    fi
}

# 停止单个端口转发
stop_port_forward() {
    local service=$1
    local pid_file="$PID_DIR/${service}.pid"
    
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        if ps -p "$pid" &> /dev/null; then
            kill "$pid" 2>/dev/null
            print_status "已停止 $service 端口转发 (PID: $pid)"
        fi
        rm -f "$pid_file"
    fi
}

# 启动所有端口转发
start_all() {
    print_banner
    print_info "Namespace: $NAMESPACE"
    print_info "Overlay: $OVERLAY (前缀: ${PREFIX:-无})"
    echo ""
    
    check_kubectl
    check_cluster
    check_namespace
    
    # 创建 PID 目录
    mkdir -p "$PID_DIR"
    
    echo -e "${BLUE}正在启动端口转发...${NC}"
    echo ""
    
    # 启动各服务的端口转发
    start_port_forward "${PREFIX}admin-frontend" "$FRONTEND_PORT" 80
    start_port_forward "${PREFIX}identity-api" "$IDENTITY_API_PORT" 5001
    start_port_forward "${PREFIX}gateway-api" "$GATEWAY_API_PORT" 5000
    
    echo ""
    echo -e "${GREEN}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}端口转发已启动！您可以通过以下地址访问服务：${NC}"
    echo ""
    echo -e "  ${BLUE}Admin Frontend${NC}: http://localhost:${FRONTEND_PORT}"
    echo -e "  ${BLUE}Identity API${NC}:   http://localhost:${IDENTITY_API_PORT}"
    echo -e "  ${BLUE}Gateway API${NC}:    http://localhost:${GATEWAY_API_PORT}"
    echo ""
    echo -e "${YELLOW}提示：${NC}"
    echo "  - 使用 '$0 stop' 停止所有端口转发"
    echo "  - 使用 '$0 status' 查看端口转发状态"
    echo "  - 端口转发会在后台运行，关闭终端不会影响"
    echo -e "${GREEN}════════════════════════════════════════════════════════════════${NC}"
}

# 停止所有端口转发
stop_all() {
    print_banner
    print_info "正在停止所有端口转发..."
    echo ""
    
    if [ -d "$PID_DIR" ]; then
        for pid_file in "$PID_DIR"/*.pid; do
            if [ -f "$pid_file" ]; then
                local service=$(basename "$pid_file" .pid)
                stop_port_forward "$service"
            fi
        done
        rm -rf "$PID_DIR"
    fi
    
    print_status "所有端口转发已停止"
}

# 查看状态
show_status() {
    print_banner
    print_info "Namespace: $NAMESPACE"
    print_info "Overlay: $OVERLAY"
    echo ""
    
    echo -e "${BLUE}端口转发状态：${NC}"
    echo ""
    
    local has_running=false
    
    if [ -d "$PID_DIR" ]; then
        for pid_file in "$PID_DIR"/*.pid; do
            if [ -f "$pid_file" ]; then
                local service=$(basename "$pid_file" .pid)
                local pid=$(cat "$pid_file")
                if ps -p "$pid" &> /dev/null; then
                    print_status "$service - 运行中 (PID: $pid)"
                    has_running=true
                else
                    print_warning "$service - 已停止"
                    rm -f "$pid_file"
                fi
            fi
        done
    fi
    
    if [ "$has_running" = false ]; then
        print_info "没有正在运行的端口转发"
    fi
    
    echo ""
    echo -e "${BLUE}服务状态：${NC}"
    echo ""
    kubectl get pods -n "$NAMESPACE" -o wide 2>/dev/null || print_error "无法获取 Pod 状态"
}

# 查看日志
show_logs() {
    local service=$1
    
    if [ -z "$service" ]; then
        echo "用法: $0 logs <service>"
        echo "可用服务: identity-api, gateway-api, admin-frontend"
        exit 1
    fi
    
    local full_service="${PREFIX}${service}"
    local app_label="$service"
    
    print_info "显示 $full_service 的日志..."
    kubectl logs -f -l "app=$app_label" -n "$NAMESPACE" --tail=100
}

# 显示帮助
show_help() {
    print_banner
    echo "用法: $0 [命令] [选项]"
    echo ""
    echo "命令:"
    echo "  start     启动所有端口转发 (默认)"
    echo "  stop      停止所有端口转发"
    echo "  restart   重启所有端口转发"
    echo "  status    查看端口转发状态"
    echo "  logs      查看服务日志 (例: $0 logs identity-api)"
    echo "  help      显示此帮助信息"
    echo ""
    echo "环境变量:"
    echo "  NAMESPACE          K8s 命名空间 (默认: dawning)"
    echo "  OVERLAY            Kustomize overlay (默认: dev)"
    echo "  IDENTITY_API_PORT  Identity API 本地端口 (默认: 5001)"
    echo "  GATEWAY_API_PORT   Gateway API 本地端口 (默认: 5000)"
    echo "  FRONTEND_PORT      前端本地端口 (默认: 8088)"
    echo ""
    echo "示例:"
    echo "  $0                          # 启动端口转发"
    echo "  $0 start                    # 启动端口转发"
    echo "  $0 stop                     # 停止端口转发"
    echo "  $0 status                   # 查看状态"
    echo "  OVERLAY=staging $0 start    # 使用 staging overlay"
    echo "  FRONTEND_PORT=3000 $0       # 自定义前端端口"
}

# 主入口
main() {
    local command="${1:-start}"
    
    case "$command" in
        start)
            start_all
            ;;
        stop)
            stop_all
            ;;
        restart)
            stop_all
            sleep 1
            start_all
            ;;
        status)
            show_status
            ;;
        logs)
            show_logs "$2"
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "未知命令: $command"
            echo ""
            show_help
            exit 1
            ;;
    esac
}

main "$@"
