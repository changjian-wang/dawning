# ArgoCD + Kustomize Deployment

This directory contains ArgoCD application definitions for GitOps-based deployment.

## Architecture

```
Git Repository (Single Source of Truth)
    ├── deploy/k8s/overlays/dev      → ArgoCD → Dev Cluster
    ├── deploy/k8s/overlays/staging  → ArgoCD → Staging Cluster
    └── deploy/k8s/overlays/prod     → ArgoCD → Prod Cluster
```

## Files

```
argocd/
├── install-argocd.sh       # ArgoCD installation script
├── project.yaml            # ArgoCD project definition (RBAC)
├── application-dev.yaml    # Dev environment application
├── application-staging.yaml # Staging environment application
└── application-prod.yaml   # Production environment application
```

## Quick Start

### 1. Install ArgoCD

```bash
# On Kind cluster
chmod +x deploy/argocd/install-argocd.sh
./deploy/argocd/install-argocd.sh

# Wait for installation
kubectl get pods -n argocd -w
```

### 2. Access ArgoCD UI

```bash
# Port forward
kubectl port-forward svc/argocd-server -n argocd 8080:443

# Get admin password
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

# Open browser
open https://localhost:8080
# Username: admin
# Password: (from above command)
```

### 3. Update Git Repository URL

Edit the `repoURL` in all application files:

```bash
# Update all application files at once
sed -i '' 's|https://github.com/YOUR_ORG/dawning.git|https://github.com/YOUR_USERNAME/dawning.git|g' deploy/argocd/*.yaml
```

### 4. Deploy Applications

```bash
# Deploy project definition (optional but recommended)
kubectl apply -f deploy/argocd/project.yaml

# Deploy dev environment
kubectl apply -f deploy/argocd/application-dev.yaml

# Deploy staging environment
kubectl apply -f deploy/argocd/application-staging.yaml

# Deploy production (manual sync only)
kubectl apply -f deploy/argocd/application-prod.yaml
```

### 5. Verify Deployment

```bash
# Check ArgoCD applications
kubectl get applications -n argocd

# Check application status
argocd app list

# View application details
argocd app get dawning-dev

# View sync status
argocd app sync dawning-dev
```

## Sync Policies

| Environment | Auto-Sync | Self-Heal | Prune | Reason |
|-------------|-----------|-----------|-------|--------|
| Dev | ✅ Yes | ✅ Yes | ✅ Yes | Fast iteration |
| Staging | ❌ Manual | ❌ No | ✅ Yes | Controlled testing |
| Prod | ❌ Manual | ❌ No | ⚠️ Manual | Safety first |

## GitOps Workflow

### Development Flow

```bash
# 1. Make changes to Kustomize files
vim deploy/k8s/overlays/dev/kustomization.yaml

# 2. Commit and push
git add deploy/k8s/overlays/dev/
git commit -m "Update dev config"
git push

# 3. ArgoCD auto-syncs (if enabled)
# Or manual sync:
argocd app sync dawning-dev
```

### Production Flow

```bash
# 1. Test in dev/staging first
argocd app sync dawning-dev
argocd app sync dawning-staging

# 2. Merge to release branch
git checkout release
git merge main

# 3. Manual review in ArgoCD UI

# 4. Manual sync (after approval)
argocd app sync dawning-prod --dry-run  # Preview
argocd app sync dawning-prod            # Execute
```

## ArgoCD CLI Commands

```bash
# Install CLI
brew install argocd

# Login
argocd login localhost:8080 --username admin --insecure

# List applications
argocd app list

# Get application details
argocd app get dawning-dev

# Sync application
argocd app sync dawning-dev

# View sync history
argocd app history dawning-dev

# Rollback to previous version
argocd app rollback dawning-dev 1

# Delete application
argocd app delete dawning-dev
```

## Monitoring

### Application Health

```bash
# Check application health
kubectl get applications -n argocd

# View detailed status
argocd app get dawning-dev

# View resources
argocd app resources dawning-dev
```

### Logs

```bash
# ArgoCD controller logs
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-application-controller -f

# ArgoCD server logs
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-server -f
```

## Troubleshooting

### Application OutOfSync

```bash
# Check differences
argocd app diff dawning-dev

# Force sync
argocd app sync dawning-dev --force

# Refresh application
argocd app get dawning-dev --refresh
```

### Sync Failed

```bash
# View sync result
argocd app get dawning-dev

# View detailed sync result
kubectl describe application dawning-dev -n argocd

# Retry sync
argocd app sync dawning-dev --retry-limit 3
```

### Access Issues

```bash
# Check ArgoCD service
kubectl get svc -n argocd

# Check pods
kubectl get pods -n argocd

# Restart ArgoCD server
kubectl rollout restart deployment argocd-server -n argocd
```

## Best Practices

1. **Always test in dev/staging first** before production
2. **Use manual sync for production** - never enable auto-sync
3. **Review diffs before syncing** - use `argocd app diff`
4. **Use separate Git branches** - main for dev/staging, release for prod
5. **Enable notifications** - integrate with Slack/email
6. **Regular backups** - backup ArgoCD config and secrets
7. **RBAC setup** - use AppProjects to limit permissions

## Integration with CI/CD

### GitHub Actions Example

```yaml
name: Update Dev Environment
on:
  push:
    branches: [main]
    paths: ['deploy/k8s/overlays/dev/**']

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Trigger ArgoCD Sync
        run: |
          argocd app sync dawning-dev --auth-token ${{ secrets.ARGOCD_TOKEN }}
```

## Security

- **Change default admin password** immediately after installation
- **Enable RBAC** - create separate users for team members
- **Use HTTPS** - configure TLS certificates
- **Secret management** - use sealed-secrets or external secrets operator
- **Audit logs** - enable and monitor ArgoCD audit logs

## Uninstall

```bash
# Delete applications
kubectl delete -f deploy/argocd/

# Delete ArgoCD
kubectl delete -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Delete namespace
kubectl delete namespace argocd
```
