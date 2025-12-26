# Dawning Gateway Administrator Guide

**Version**: 1.0.0  
**Last Updated**: 2025-12-26

---

## Table of Contents

1. [Administrator Responsibilities](#1-administrator-responsibilities)
2. [User Management](#2-user-management)
3. [Role and Permission Management](#3-role-and-permission-management)
4. [Security Configuration](#4-security-configuration)
5. [Gateway Configuration](#5-gateway-configuration)
6. [System Monitoring](#6-system-monitoring)
7. [Backup and Recovery](#7-backup-and-recovery)
8. [Troubleshooting](#8-troubleshooting)
9. [Best Practices](#9-best-practices)

---

## 1. Administrator Responsibilities

### 1.1 Daily Operations

- Monitor system status
- Process user permission requests
- Regular security log reviews
- Execute data backups

### 1.2 Security Management

- Configure password policies
- Manage access controls
- Review audit logs
- Update security settings

### 1.3 System Maintenance

- Update system configurations
- Manage API gateway routes
- Performance optimization
- Capacity planning

---

## 2. User Management

### 2.1 Creating Users

1. Navigate to **Administration** → **User Management**
2. Click **Add** button
3. Fill in required information:
   - Username (required)
   - Email (required)
   - Display Name
   - Password
4. Click **Submit**

### 2.2 Editing Users

1. Find the user in the list
2. Click the **Edit** icon
3. Modify user information
4. Click **Submit**

### 2.3 Disabling Users

1. Find the user in the list
2. Toggle the **Status** switch
3. Confirm the action

### 2.4 Resetting Passwords

1. Find the user in the list
2. Click **More** → **Reset Password**
3. Enter new password
4. Confirm the action

---

## 3. Role and Permission Management

### 3.1 System Roles

| Role | Description | Permissions |
|------|-------------|-------------|
| super_admin | Super Administrator | All permissions |
| admin | Administrator | User and application management |
| user_manager | User Manager | User CRUD operations |
| auditor | Auditor | Read-only access |
| user | Regular User | Self-management only |

### 3.2 Creating Custom Roles

1. Navigate to **Administration** → **Role Management**
2. Click **Create**
3. Enter role name and description
4. Select permissions
5. Click **Submit**

### 3.3 Assigning Roles to Users

1. Navigate to **User Management**
2. Click the **Assign Roles** icon for the target user
3. Select roles to assign
4. Click **Confirm**

---

## 4. Security Configuration

### 4.1 Password Policy

Configure password requirements in **System Config**:

- Minimum length
- Complexity requirements
- Expiration period
- History count

### 4.2 Session Management

- Session timeout duration
- Concurrent session limits
- Remember me settings

### 4.3 Access Control

- IP whitelist/blacklist
- Rate limiting
- CORS configuration

---

## 5. Gateway Configuration

### 5.1 Route Management

1. Navigate to **Gateway Management** → **Routes**
2. Add or modify routes
3. Configure:
   - Route path pattern
   - Target cluster
   - Authentication requirements
   - Rate limiting

### 5.2 Cluster Management

1. Navigate to **Gateway Management** → **Clusters**
2. Define backend service clusters
3. Configure:
   - Destination addresses
   - Health checks
   - Load balancing

### 5.3 Rate Limiting

Configure rate limiting policies:
- Request per second limits
- IP-based rules
- API-specific limits

---

## 6. System Monitoring

### 6.1 System Monitor Dashboard

Access real-time metrics:
- CPU usage
- Memory usage
- Active connections
- Request throughput

### 6.2 Audit Logs

View all administrative actions:
- User management operations
- Configuration changes
- Permission modifications

### 6.3 System Logs

Monitor application logs:
- Error logs
- Warning logs
- Information logs

### 6.4 Alert Management

Configure alert rules for:
- High error rates
- Performance degradation
- Resource exhaustion

---

## 7. Backup and Recovery

### 7.1 Database Backup

Regular backup schedule:
```bash
# Daily backup
mysqldump -u root -p dawning_identity > backup_$(date +%Y%m%d).sql
```

### 7.2 Configuration Backup

Export system configurations periodically:
- Gateway routes
- Rate limiting rules
- System settings

### 7.3 Recovery Procedures

1. Stop the application
2. Restore database from backup
3. Restore configuration files
4. Restart the application
5. Verify system status

---

## 8. Troubleshooting

### 8.1 Common Issues

| Issue | Possible Cause | Solution |
|-------|---------------|----------|
| Login failure | Invalid credentials | Check username/password |
| 403 Forbidden | Insufficient permissions | Verify role assignments |
| 502 Bad Gateway | Backend service down | Check cluster health |
| High latency | Resource constraints | Scale resources |

### 8.2 Log Analysis

Check logs at:
- Application logs: `/var/log/dawning/`
- System logs: `Administration` → `System Log`
- Audit logs: `Administration` → `Audit Log`

### 8.3 Health Checks

Access health endpoints:
- `/health` - Overall health status
- `/health/ready` - Readiness check
- `/health/live` - Liveness check

---

## 9. Best Practices

### 9.1 Security

- Enable MFA for administrator accounts
- Regular password rotation
- Principle of least privilege
- Regular security audits

### 9.2 Performance

- Monitor resource usage
- Configure appropriate rate limits
- Use caching effectively
- Regular maintenance windows

### 9.3 Operations

- Document all changes
- Use staging environment for testing
- Maintain runbooks for common procedures
- Regular backup verification

---

*For technical details, see the [Developer Guide](DEVELOPER_GUIDE.md).*
