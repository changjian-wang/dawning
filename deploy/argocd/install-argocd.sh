#!/bin/bash
# =============================================
# ArgoCD Installation Script for Kind
# =============================================

set -e

CLUSTER_NAME="dawning"
ARGOCD_VERSION="stable"

echo "🚀 Installing ArgoCD on Kind cluster"
echo "===================================="

# Check if kind cluster exists
if ! kind get clusters 2>/dev/null | grep -q "^${CLUSTER_NAME}$"; then
    echo "❌ Kind cluster '${CLUSTER_NAME}' not found!"
    echo "Please run: ./setup-cluster.sh first"
    exit 1
fi

# Install ArgoCD
echo ""
echo "📦 Installing ArgoCD..."
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/${ARGOCD_VERSION}/manifests/install.yaml

echo "⏳ Waiting for ArgoCD to be ready..."
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=argocd-server -n argocd --timeout=300s

# Get initial admin password
echo ""
echo "🔑 ArgoCD Initial Admin Password:"
ARGOCD_PASSWORD=$(kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d)
echo "   Username: admin"
echo "   Password: ${ARGOCD_PASSWORD}"

# Port forward (in background)
echo ""
echo "🌐 Starting port-forward to ArgoCD UI..."
kubectl port-forward svc/argocd-server -n argocd 8080:443 > /dev/null 2>&1 &
PORT_FORWARD_PID=$!

echo ""
echo "✅ ArgoCD Installation Complete!"
echo ""
echo "Access ArgoCD UI:"
echo "  URL: https://localhost:8080"
echo "  Username: admin"
echo "  Password: ${ARGOCD_PASSWORD}"
echo ""
echo "ArgoCD CLI Login:"
echo "  argocd login localhost:8080 --username admin --password ${ARGOCD_PASSWORD} --insecure"
echo ""
echo "Stop port-forward:"
echo "  kill ${PORT_FORWARD_PID}"
echo ""
echo "Next steps:"
echo "  1. Edit deploy/argocd/*.yaml files to update your Git repo URL"
echo "  2. Deploy applications:"
echo "     kubectl apply -f deploy/argocd/application-dev.yaml"
echo ""
