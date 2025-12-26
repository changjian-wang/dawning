# Dawning Helm Chart

Deploy Dawning Gateway to Kubernetes using Helm.

## Prerequisites

- Kubernetes 1.25+
- Helm 3.10+
- PV provisioner (for persistent volumes)

## Installation

### Quick Start

```bash
helm install dawning . --namespace dawning --create-namespace
```

### With Custom Values

```bash
helm install dawning . \
  --namespace dawning \
  --create-namespace \
  --set image.tag=v1.0.0 \
  --set mysql.password=your-password \
  --set ingress.enabled=true \
  --set ingress.hosts[0].host=dawning.example.com
```

### From Values File

```bash
helm install dawning . -f my-values.yaml --namespace dawning --create-namespace
```

## Configuration

### Key Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `replicaCount` | Number of replicas | `1` |
| `image.repository` | Image repository | `dawning-gateway` |
| `image.tag` | Image tag | `latest` |
| `image.pullPolicy` | Image pull policy | `IfNotPresent` |
| `service.type` | Service type | `ClusterIP` |
| `service.port` | Service port | `8080` |
| `ingress.enabled` | Enable ingress | `false` |
| `ingress.className` | Ingress class | `nginx` |
| `resources.limits.cpu` | CPU limit | `1000m` |
| `resources.limits.memory` | Memory limit | `1Gi` |
| `mysql.enabled` | Deploy MySQL | `true` |
| `mysql.password` | MySQL root password | `""` |
| `redis.enabled` | Deploy Redis | `true` |

### Full Values Example

```yaml
replicaCount: 3

image:
  repository: your-registry/dawning-gateway
  tag: "v1.0.0"
  pullPolicy: IfNotPresent

imagePullSecrets:
  - name: registry-secret

service:
  type: ClusterIP
  port: 8080

ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
  hosts:
    - host: dawning.example.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: dawning-tls
      hosts:
        - dawning.example.com

resources:
  limits:
    cpu: 1000m
    memory: 1Gi
  requests:
    cpu: 200m
    memory: 256Mi

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70

mysql:
  enabled: true
  password: secure-password
  persistence:
    enabled: true
    size: 10Gi

redis:
  enabled: true
  persistence:
    enabled: true
    size: 1Gi

env:
  ASPNETCORE_ENVIRONMENT: Production
```

## Upgrade

```bash
helm upgrade dawning . --namespace dawning -f my-values.yaml
```

## Uninstall

```bash
helm uninstall dawning --namespace dawning
```

## Troubleshooting

### Check Pod Status

```bash
kubectl get pods -n dawning
kubectl describe pod <pod-name> -n dawning
```

### View Logs

```bash
kubectl logs -n dawning deployment/dawning-gateway
```

### Check Configuration

```bash
kubectl get configmap -n dawning
kubectl get secret -n dawning
```

---

See [Deployment Guide](../../docs/DEPLOYMENT.md) for detailed deployment instructions.
