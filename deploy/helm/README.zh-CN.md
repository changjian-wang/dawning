# Dawning Gateway - Helm Chart

本 Helm Chart 用于将 Dawning Gateway 管理系统部署到 Kubernetes 集群。

## 前置要求

- Kubernetes 1.23+
- Helm 3.8+
- PV provisioner（用于持久化存储）
- Ingress Controller（可选，用于外部访问）

## 快速开始

### 1. 添加 Bitnami 仓库（基础设施依赖）

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

### 2. 更新依赖

```bash
cd helm/dawning
helm dependency update
```

### 3. 安装

**开发环境：**
```bash
helm install dawning ./helm/dawning \
  -f ./helm/dawning/values-dev.yaml \
  -n dawning-dev --create-namespace
```

**生产环境：**
```bash
helm install dawning ./helm/dawning \
  -f ./helm/dawning/values-prod.yaml \
  -n dawning --create-namespace \
  --set database.external.password=<your-db-password> \
  --set secrets.jwtSecret=<your-jwt-secret>
```

## 配置说明

### values.yaml 结构

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `global.namespace` | 命名空间 | `dawning` |
| `global.environment` | 环境标识 | `production` |
| `identityApi.replicaCount` | Identity API 副本数 | `2` |
| `identityApi.image.repository` | 镜像仓库 | `dawning/identity-api` |
| `gatewayApi.replicaCount` | Gateway API 副本数 | `2` |
| `adminFrontend.replicaCount` | 前端副本数 | `2` |
| `ingress.enabled` | 是否启用 Ingress | `true` |
| `mysql.enabled` | 是否部署内置 MySQL | `true` |
| `redis.enabled` | 是否部署内置 Redis | `true` |
| `kafka.enabled` | 是否部署内置 Kafka | `true` |

### 使用外部数据库

生产环境建议使用云托管数据库服务：

```yaml
# values-prod.yaml
database:
  external:
    enabled: true
    host: "your-rds.amazonaws.com"
    port: 3306
    database: dawning_identity
    username: "dawning"
    password: ""  # 通过 --set 传入

mysql:
  enabled: false
```

### 自定义 Ingress

```yaml
ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
  hosts:
    - host: dawning.your-domain.com
      paths:
        - path: /
          pathType: Prefix
          service: admin-frontend
        - path: /api
          pathType: Prefix
          service: identity-api
  tls:
    - secretName: dawning-tls
      hosts:
        - dawning.your-domain.com
```

## 常用命令

### 查看部署状态

```bash
# 查看所有资源
kubectl get all -n dawning

# 查看 Pod 日志
kubectl logs -f deployment/dawning-identity-api -n dawning

# 查看 ConfigMap
kubectl get configmap -n dawning
```

### 升级

```bash
helm upgrade dawning ./helm/dawning -n dawning
```

### 回滚

```bash
# 查看历史版本
helm history dawning -n dawning

# 回滚到上一版本
helm rollback dawning -n dawning
```

### 卸载

```bash
helm uninstall dawning -n dawning

# 如果需要删除 PVC
kubectl delete pvc -l app.kubernetes.io/instance=dawning -n dawning
```

## 本地开发调试

使用 port-forward 访问服务：

```bash
# 前端
kubectl port-forward svc/dawning-admin-frontend 8080:80 -n dawning-dev

# Identity API
kubectl port-forward svc/dawning-identity-api 5001:5001 -n dawning-dev

# MySQL
kubectl port-forward svc/dawning-mysql 3306:3306 -n dawning-dev
```

## 构建 Docker 镜像

在部署之前，需要构建并推送 Docker 镜像：

```bash
# Identity API
docker build -t dawning/identity-api:latest -f services/Dawning.Gateway/src/Dawning.Identity.Api/Dockerfile .

# Gateway API
docker build -t dawning/gateway-api:latest -f services/Dawning.Gateway/src/Dawning.Gateway.Api/Dockerfile .

# Admin Frontend
docker build -t dawning/admin-frontend:latest -f dawning-admin/Dockerfile .
```

## 目录结构

```
helm/dawning/
├── Chart.yaml              # Chart 元信息
├── values.yaml             # 默认配置
├── values-dev.yaml         # 开发环境配置
├── values-prod.yaml        # 生产环境配置
└── templates/
    ├── _helpers.tpl        # 模板函数
    ├── configmap.yaml      # ConfigMap
    ├── secrets.yaml        # Secrets
    ├── ingress.yaml        # Ingress
    ├── NOTES.txt           # 安装后提示
    ├── identity-api/
    │   ├── deployment.yaml
    │   ├── service.yaml
    │   └── hpa.yaml
    ├── gateway-api/
    │   ├── deployment.yaml
    │   ├── service.yaml
    │   └── hpa.yaml
    └── admin-frontend/
        ├── deployment.yaml
        └── service.yaml
```

## 故障排查

### Pod 启动失败

```bash
# 查看事件
kubectl describe pod <pod-name> -n dawning

# 查看日志
kubectl logs <pod-name> -n dawning --previous
```

### 数据库连接失败

1. 检查 Secret 是否正确创建
2. 检查网络策略
3. 验证数据库服务是否可达

### Ingress 不生效

1. 确认 Ingress Controller 已安装
2. 检查 IngressClass 是否正确
3. 查看 Ingress Controller 日志
