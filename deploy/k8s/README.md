# Kubernetes Deployment with Kind (Multi-Node)

This directory contains Kubernetes deployment configurations using Kustomize.

## Directory Structure

```
k8s/
├── kind-config.yaml      # Kind multi-node cluster configuration
├── setup-cluster.sh      # Cluster setup script
├── base/                 # Base configurations
│   ├── kustomization.yaml
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secrets.yaml
│   ├── ingress.yaml
│   ├── mysql/
│   ├── redis/
│   ├── zookeeper/
│   ├── kafka/
│   ├── identity-api/
│   ├── gateway-api/
│   └── admin-frontend/
└── overlays/             # Environment-specific overrides
    ├── dev/
    ├── staging/
    └── prod/
```

## Prerequisites

- Docker Desktop or Colima
- Kind (`brew install kind`)
- kubectl (`brew install kubectl`)

## Quick Start

### 1. Create Multi-Node Cluster

```bash
# Using setup script
chmod +x deploy/k8s/setup-cluster.sh
./deploy/k8s/setup-cluster.sh

# Or manually
kind create cluster --name dawning --config deploy/k8s/kind-config.yaml
```

### 2. Build Docker Images

```bash
# Build all images
cd apps/gateway
docker build -t dawning-identity-api:latest -f src/Dawning.Identity.Api/Dockerfile ../..
docker build -t dawning-gateway-api:latest -f src/Dawning.Gateway.Api/Dockerfile ../..

cd ../admin
docker build -t dawning-admin-frontend:latest .

# Load images to Kind cluster
kind load docker-image dawning-identity-api:latest --name dawning
kind load docker-image dawning-gateway-api:latest --name dawning
kind load docker-image dawning-admin-frontend:latest --name dawning
```

### 3. Deploy

```bash
# Deploy dev environment
kubectl apply -k deploy/k8s/overlays/dev

# Or deploy base directly
kubectl apply -k deploy/k8s/base

# Watch pods coming up
kubectl get pods -n dawning -w
```

### 4. Access Services

Add to `/etc/hosts`:
```
127.0.0.1 dawning.local api.dawning.local auth.dawning.local
```

Then access:
- Frontend: http://dawning.local
- API Gateway: http://api.dawning.local
- Identity API: http://auth.dawning.local

## Cluster Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Kind Cluster: dawning                     │
├──────────────┬──────────────┬──────────────┬────────────────┤
│Control Plane │   Worker 1   │   Worker 2   │    Worker 3    │
│              │ (infra)      │ (messaging)  │ (application)  │
├──────────────┼──────────────┼──────────────┼────────────────┤
│ • etcd       │ • MySQL      │ • Zookeeper  │ • Identity API │
│ • API Server │ • Redis      │ • Kafka      │ • Gateway API  │
│ • Scheduler  │              │              │ • Frontend     │
│ • Controller │              │              │                │
└──────────────┴──────────────┴──────────────┴────────────────┘
```

## Environment Overlays

| Environment | Replicas | Resources | Use Case |
|-------------|----------|-----------|----------|
| dev         | 1        | Low       | Local development |
| staging     | 2        | Medium    | Testing |
| prod        | 3        | High      | Production simulation |

Deploy specific environment:
```bash
kubectl apply -k deploy/k8s/overlays/dev
kubectl apply -k deploy/k8s/overlays/staging
kubectl apply -k deploy/k8s/overlays/prod
```

## Useful Commands

```bash
# Check cluster status
kubectl get nodes -o wide

# Check all resources
kubectl get all -n dawning

# Check pod distribution
kubectl get pods -n dawning -o wide

# View logs
kubectl logs -n dawning -l app=identity-api -f

# Scale deployment
kubectl scale deployment -n dawning gateway-api --replicas=5

# Port forward for debugging
kubectl port-forward -n dawning svc/mysql 3306:3306

# Delete cluster
kind delete cluster --name dawning
```

## Troubleshooting

### Pods stuck in Pending
```bash
kubectl describe pod -n dawning <pod-name>
# Check for resource constraints or node selector issues
```

### Image not found
```bash
# Ensure images are loaded to Kind
kind load docker-image <image>:latest --name dawning
```

### Connection refused
```bash
# Check if services are ready
kubectl get endpoints -n dawning
```
