#!/bin/bash
# =============================================
# Kind Cluster Setup Script
# =============================================

set -e

CLUSTER_NAME="dawning"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "🚀 Dawning Kubernetes Cluster Setup"
echo "===================================="

# Check prerequisites
check_prerequisites() {
    echo "📋 Checking prerequisites..."
    
    if ! command -v kind &> /dev/null; then
        echo "❌ Kind not found. Installing..."
        if [[ "$OSTYPE" == "darwin"* ]]; then
            brew install kind
        elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
            curl -Lo ./kind https://kind.sigs.k8s.io/dl/v0.20.0/kind-linux-amd64
            chmod +x ./kind
            sudo mv ./kind /usr/local/bin/kind
        else
            echo "Please install Kind manually: https://kind.sigs.k8s.io/docs/user/quick-start/#installation"
            exit 1
        fi
    fi
    
    if ! command -v kubectl &> /dev/null; then
        echo "❌ kubectl not found. Please install kubectl first."
        exit 1
    fi
    
    echo "✅ Prerequisites satisfied"
}

# Create cluster
create_cluster() {
    echo ""
    echo "🔧 Creating Kind cluster: $CLUSTER_NAME"
    
    # Check if cluster exists
    if kind get clusters 2>/dev/null | grep -q "^${CLUSTER_NAME}$"; then
        echo "⚠️  Cluster '$CLUSTER_NAME' already exists."
        read -p "Do you want to recreate it? (y/N) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            kind delete cluster --name $CLUSTER_NAME
        else
            echo "Using existing cluster."
            return
        fi
    fi
    
    kind create cluster --name $CLUSTER_NAME --config "$SCRIPT_DIR/kind-config.yaml"
    echo "✅ Cluster created successfully"
}

# Install Ingress Controller
install_ingress() {
    echo ""
    echo "🌐 Installing NGINX Ingress Controller..."
    
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
    
    echo "⏳ Waiting for Ingress Controller to be ready..."
    kubectl wait --namespace ingress-nginx \
        --for=condition=ready pod \
        --selector=app.kubernetes.io/component=controller \
        --timeout=90s
    
    echo "✅ Ingress Controller installed"
}

# Create namespace
create_namespace() {
    echo ""
    echo "📦 Creating dawning namespace..."
    kubectl create namespace dawning --dry-run=client -o yaml | kubectl apply -f -
    echo "✅ Namespace created"
}

# Deploy infrastructure
deploy_infrastructure() {
    echo ""
    echo "🏗️  Deploying infrastructure..."
    kubectl apply -k "$SCRIPT_DIR/base"
    echo "✅ Infrastructure deployed"
}

# Show status
show_status() {
    echo ""
    echo "📊 Cluster Status:"
    echo "=================="
    echo ""
    echo "Nodes:"
    kubectl get nodes -o wide
    echo ""
    echo "Pods in dawning namespace:"
    kubectl get pods -n dawning -o wide
    echo ""
    echo "Services:"
    kubectl get svc -n dawning
}

# Main
main() {
    check_prerequisites
    create_cluster
    install_ingress
    create_namespace
    
    echo ""
    echo "🎉 Kind cluster is ready!"
    echo ""
    echo "Next steps:"
    echo "  1. Deploy base infrastructure:"
    echo "     kubectl apply -k deploy/k8s/base"
    echo ""
    echo "  2. Or deploy specific environment:"
    echo "     kubectl apply -k deploy/k8s/overlays/dev"
    echo ""
    echo "  3. Check status:"
    echo "     kubectl get pods -n dawning -w"
    echo ""
}

main "$@"
