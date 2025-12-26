# Certificate Configuration Guide

This guide explains how to configure HTTPS certificates for Dawning Gateway.

## Overview

Dawning Gateway supports multiple certificate configuration methods:

1. **Development**: Self-signed certificates
2. **Production**: CA-signed certificates
3. **Kubernetes**: cert-manager with Let's Encrypt

## Development Certificates

### Generate Self-Signed Certificate

```bash
# Windows (PowerShell)
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"

# Linux/macOS
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=localhost"
```

### Configure in appsettings.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "./certs/cert.pfx",
          "Password": "your-password"
        }
      }
    }
  }
}
```

## Production Certificates

### Option 1: PFX Certificate

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/app/certs/production.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  }
}
```

### Option 2: PEM Certificate

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/app/certs/cert.pem",
          "KeyPath": "/app/certs/key.pem"
        }
      }
    }
  }
}
```

### Option 3: Certificate Store (Windows)

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Subject": "CN=your-domain.com",
          "Store": "My",
          "Location": "LocalMachine"
        }
      }
    }
  }
}
```

## Kubernetes with cert-manager

### Install cert-manager

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

### Create ClusterIssuer

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
      - http01:
          ingress:
            class: nginx
```

### Configure Ingress with TLS

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: dawning-ingress
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - dawning.your-domain.com
      secretName: dawning-tls
  rules:
    - host: dawning.your-domain.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: dawning-gateway
                port:
                  number: 8080
```

## Certificate Renewal

### Manual Renewal

Replace certificate files and restart the application:

```bash
# Docker
docker-compose restart gateway

# Kubernetes
kubectl rollout restart deployment/dawning-gateway -n dawning
```

### Automatic Renewal

For Kubernetes with cert-manager, certificates are renewed automatically 30 days before expiration.

## Security Recommendations

- ✅ Use TLS 1.2 or higher
- ✅ Use strong cipher suites
- ✅ Enable HSTS in production
- ✅ Keep private keys secure
- ✅ Set up certificate monitoring
- ✅ Plan for certificate renewal

## Troubleshooting

### Certificate Not Found

```bash
# Check certificate file exists
ls -la /app/certs/

# Verify certificate is valid
openssl x509 -in cert.pem -text -noout
```

### Certificate Chain Issues

Ensure the certificate file includes the full chain:
1. Server certificate
2. Intermediate certificate(s)
3. Root certificate (optional)

### Permission Issues

```bash
# Linux: Ensure correct permissions
chmod 600 /app/certs/key.pem
chmod 644 /app/certs/cert.pem
```

---

See [Deployment Guide](../../docs/DEPLOYMENT.md) for full deployment instructions.
