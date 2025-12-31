# Kubernetes 部署 (Kind 多节点)

本目录包含使用 Kustomize 的 Kubernetes 部署配置。

## 目录结构

```
k8s/
├── kind-config.yaml      # Kind 多节点集群配置
├── setup-cluster.sh      # 集群安装脚本
├── base/                 # 基础配置
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
└── overlays/             # 环境特定覆盖
    ├── dev/
    ├── staging/
    └── prod/
```

## 前置条件

- Docker Desktop 或 Colima
- Kind (`brew install kind`)
- kubectl (`brew install kubectl`)

## 快速开始

### 1. 创建多节点集群

```bash
# 使用安装脚本
chmod +x deploy/k8s/setup-cluster.sh
./deploy/k8s/setup-cluster.sh

# 或手动创建
kind create cluster --name dawning --config deploy/k8s/kind-config.yaml
```

### 2. 构建 Docker 镜像

```bash
# 构建所有镜像
cd apps/gateway
docker build -t dawning-identity-api:latest -f src/Dawning.Identity.Api/Dockerfile ../..
docker build -t dawning-gateway-api:latest -f src/Dawning.Gateway.Api/Dockerfile ../..

cd ../admin
docker build -t dawning-admin-frontend:latest .

# 加载镜像到 Kind 集群
kind load docker-image dawning-identity-api:latest --name dawning
kind load docker-image dawning-gateway-api:latest --name dawning
kind load docker-image dawning-admin-frontend:latest --name dawning
```

### 3. 部署

```bash
# 部署开发环境
kubectl apply -k deploy/k8s/overlays/dev

# 或直接部署 base
kubectl apply -k deploy/k8s/base

# 监控 Pod 启动
kubectl get pods -n dawning -w
```

### 4. 访问服务

添加到 `/etc/hosts`:
```
127.0.0.1 dawning.local api.dawning.local auth.dawning.local
```

访问地址:
- 前端: http://dawning.local
- API 网关: http://api.dawning.local
- 认证 API: http://auth.dawning.local

## 集群架构

```
┌─────────────────────────────────────────────────────────────┐
│                    Kind 集群: dawning                        │
├──────────────┬──────────────┬──────────────┬────────────────┤
│ 控制平面     │   Worker 1   │   Worker 2   │    Worker 3    │
│              │ (基础设施)   │ (消息队列)   │ (应用服务)     │
├──────────────┼──────────────┼──────────────┼────────────────┤
│ • etcd       │ • MySQL      │ • Zookeeper  │ • Identity API │
│ • API Server │ • Redis      │ • Kafka      │ • Gateway API  │
│ • Scheduler  │              │              │ • Frontend     │
│ • Controller │              │              │                │
└──────────────┴──────────────┴──────────────┴────────────────┘
```

## 环境配置

| 环境    | 副本数 | 资源配置 | 用途           |
|---------|--------|----------|----------------|
| dev     | 1      | 低       | 本地开发       |
| staging | 2      | 中       | 测试环境       |
| prod    | 3      | 高       | 生产环境模拟   |

部署特定环境:
```bash
kubectl apply -k deploy/k8s/overlays/dev
kubectl apply -k deploy/k8s/overlays/staging
kubectl apply -k deploy/k8s/overlays/prod
```

## 常用命令

```bash
# 查看集群状态
kubectl get nodes -o wide

# 查看所有资源
kubectl get all -n dawning

# 查看 Pod 分布
kubectl get pods -n dawning -o wide

# 查看日志
kubectl logs -n dawning -l app=identity-api -f

# 扩缩容
kubectl scale deployment -n dawning gateway-api --replicas=5

# 端口转发调试
kubectl port-forward -n dawning svc/mysql 3306:3306

# 删除集群
kind delete cluster --name dawning
```

## 故障排除

### Pod 卡在 Pending 状态
```bash
kubectl describe pod -n dawning <pod-name>
# 检查资源限制或节点选择器问题
```

### 镜像未找到
```bash
# 确保镜像已加载到 Kind
kind load docker-image <image>:latest --name dawning
```

### 连接被拒绝
```bash
# 检查服务是否就绪
kubectl get endpoints -n dawning
```
