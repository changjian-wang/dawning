# Dawning Gateway Deployment Guide

**Version**: 1.0.0  
**Last Updated**: 2025-12-26

---

## Table of Contents

1. [Deployment Overview](#1-deployment-overview)
2. [Prerequisites](#2-prerequisites)
3. [Configuration](#3-configuration)
4. [Docker Deployment](#4-docker-deployment)
5. [Kubernetes Deployment](#5-kubernetes-deployment)
6. [Database Migration](#6-database-migration)
7. [Health Checks](#7-health-checks)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Deployment Overview

Dawning Gateway supports multiple deployment modes:

| Mode | Use Case |
|------|----------|
| Docker Compose | Development, Small deployments |
| Kubernetes + Helm | Production, High availability |
| Standalone | Testing, Demo |

---

## 2. Prerequisites

### 2.1 System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| CPU | 2 cores | 4+ cores |
| Memory | 4 GB | 8+ GB |
| Disk | 20 GB | 50+ GB SSD |

### 2.2 Software Requirements

- Docker 20.10+
- Docker Compose 2.0+ (for Docker deployment)
- kubectl 1.25+ (for Kubernetes)
- Helm 3.10+ (for Kubernetes)

### 2.3 External Dependencies

- MySQL 8.x
- Redis 7.x
- (Optional) Kafka 3.x

---

## 3. Configuration

### 3.1 Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `MySQL__ConnectionString` | MySQL connection string | - |
| `Redis__ConnectionString` | Redis connection string | - |
| `Jwt__Secret` | JWT signing key | - |
| `Jwt__Issuer` | Token issuer | - |
| `Jwt__Audience` | Token audience | - |

### 3.2 Production Configuration

Create `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=mysql;Database=dawning;User=app;Password=***;",
    "Redis": "redis:6379"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key",
    "Issuer": "https://your-domain.com",
    "Audience": "dawning-api"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## 4. Docker Deployment

### 4.1 Quick Start

```bash
cd deploy/docker
docker-compose up -d
```

### 4.2 Build Images

```bash
# Build gateway image
docker build -t dawning-gateway:latest -f apps/gateway/Dockerfile .

# Build admin image
docker build -t dawning-admin:latest -f apps/admin/Dockerfile .
```

### 4.3 Docker Compose Configuration

```yaml
# deploy/docker/docker-compose.yml
version: '3.8'
services:
  gateway:
    image: dawning-gateway:latest
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - mysql
      - redis

  admin:
    image: dawning-admin:latest
    ports:
      - "3000:80"
    depends_on:
      - gateway

  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: dawning
    volumes:
      - mysql_data:/var/lib/mysql

  redis:
    image: redis:7-alpine
    volumes:
      - redis_data:/data

volumes:
  mysql_data:
  redis_data:
```

### 4.4 Scaling

```bash
docker-compose up -d --scale gateway=3
```

---

## 5. Kubernetes Deployment

### 5.1 Install with Helm

```bash
cd deploy/helm/dawning

# Install
helm install dawning . \
  --namespace dawning \
  --create-namespace \
  --set mysql.password=your-password

# Upgrade
helm upgrade dawning . --namespace dawning
```

### 5.2 Values Configuration

Edit `values.yaml`:

```yaml
replicaCount: 3

image:
  repository: your-registry/dawning-gateway
  tag: "latest"
  pullPolicy: IfNotPresent

service:
  type: ClusterIP
  port: 8080

ingress:
  enabled: true
  className: nginx
  hosts:
    - host: dawning.your-domain.com
      paths:
        - path: /
          pathType: Prefix

resources:
  limits:
    cpu: 1000m
    memory: 1Gi
  requests:
    cpu: 200m
    memory: 256Mi

mysql:
  enabled: true
  password: your-password

redis:
  enabled: true
```

### 5.3 TLS Configuration

```yaml
ingress:
  enabled: true
  tls:
    - secretName: dawning-tls
      hosts:
        - dawning.your-domain.com
```

Create TLS secret:

```bash
kubectl create secret tls dawning-tls \
  --cert=path/to/tls.crt \
  --key=path/to/tls.key \
  -n dawning
```

---

## 6. Database Migration

### 6.1 Initial Setup

Execute schema scripts in order:

```bash
# Connect to MySQL
mysql -u root -p dawning

# Execute schema scripts
source apps/gateway/docs/sql/schema/001_create_users_table.sql
source apps/gateway/docs/sql/schema/002_create_roles_table.sql
# ... continue with all schema scripts

# Execute seed data
source apps/gateway/docs/sql/seed/001_init_admin.sql
```

### 6.2 Default Admin Account

After initialization:
- **Username**: `admin`
- **Password**: `Admin@123`

⚠️ **Change the default password immediately after first login!**

---

## 7. Health Checks

### 7.1 Endpoints

| Endpoint | Description |
|----------|-------------|
| `/health` | Basic health check |
| `/health/ready` | Readiness probe |
| `/health/live` | Liveness probe |

### 7.2 Kubernetes Probes

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 5
```

---

## 8. Troubleshooting

### 8.1 Common Issues

**Container fails to start:**
```bash
# Check logs
docker logs dawning-gateway

# Kubernetes
kubectl logs -n dawning deployment/dawning-gateway
```

**Database connection failed:**
- Verify connection string
- Check network connectivity
- Ensure MySQL is running

**Redis connection failed:**
- Verify Redis is running
- Check firewall rules

### 8.2 Log Levels

Set via environment variable:
```bash
Logging__LogLevel__Default=Debug
```

### 8.3 Performance Tuning

For high traffic:
- Increase replica count
- Configure connection pooling
- Enable Redis caching
- Use CDN for static assets

---

*See [Developer Guide](DEVELOPER_GUIDE.md) for development setup.*
*See [Administrator Guide](ADMIN_GUIDE.md) for system configuration.*
