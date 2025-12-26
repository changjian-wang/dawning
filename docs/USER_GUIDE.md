# Dawning Gateway User Guide

**Version**: 1.0.0  
**Last Updated**: 2025-12-26

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Login and Authentication](#2-login-and-authentication)
3. [User Management](#3-user-management)
4. [Roles and Permissions](#4-roles-and-permissions)
5. [System Configuration](#5-system-configuration)
6. [Gateway Management](#6-gateway-management)
7. [Monitoring and Logs](#7-monitoring-and-logs)
8. [FAQ](#8-faq)

---

## 1. System Overview

### 1.1 What is Dawning Gateway

Dawning Gateway is an enterprise-grade API Gateway management system built with .NET 8 and Vue 3, providing:

- **Identity Authentication** - OAuth 2.0 / OpenID Connect based on OpenIddict
- **API Gateway** - Reverse proxy and routing based on YARP
- **User Management** - Complete user, role, and permission management
- **System Monitoring** - Real-time request monitoring and performance analysis

### 1.2 Key Features

| Feature | Description |
|---------|-------------|
| Single Sign-On | Unified authentication across all services |
| Role-Based Access | Fine-grained permission control |
| API Routing | Dynamic route configuration |
| Rate Limiting | Protect services from abuse |
| Audit Logging | Track all administrative actions |

---

## 2. Login and Authentication

### 2.1 Login

1. Navigate to the login page
2. Enter your username and password
3. Click **Login**

### 2.2 Forgot Password

1. Click **Forgot Password** on the login page
2. Enter your email address
3. Check your email for reset instructions
4. Follow the link to reset your password

### 2.3 Logout

1. Click your avatar in the top-right corner
2. Select **Logout**

---

## 3. User Management

### 3.1 View User List

1. Navigate to **Administration** → **User Permission** → **User Management**
2. View all users in the system
3. Use filters to search for specific users

### 3.2 User Details

Click the **View** icon to see:
- Basic information
- Assigned roles
- Account status
- Activity history

### 3.3 Edit Profile

1. Click your avatar → **Profile**
2. Update your information
3. Click **Save**

---

## 4. Roles and Permissions

### 4.1 Understanding Roles

Roles define what actions a user can perform:

| Role | Access Level |
|------|--------------|
| Super Admin | Full system access |
| Admin | Administrative functions |
| User Manager | User management only |
| Auditor | Read-only access |
| User | Self-management only |

### 4.2 Viewing Permissions

1. Navigate to **Administration** → **User Permission** → **Permission Management**
2. Browse the permission tree
3. View permission details

---

## 5. System Configuration

### 5.1 Accessing Settings

Navigate to **Administration** → **Settings** → **System Config**

### 5.2 Configuration Categories

- **Security** - Password policies, session settings
- **Gateway** - Default route settings
- **Monitoring** - Alert thresholds
- **General** - System-wide settings

---

## 6. Gateway Management

### 6.1 Routes

View and manage API routes:

1. Navigate to **Gateway Management** → **Routes**
2. View configured routes
3. Check route status

### 6.2 Clusters

View backend service clusters:

1. Navigate to **Gateway Management** → **Clusters**
2. View cluster configurations
3. Check health status

### 6.3 Rate Limiting

View rate limit policies:

1. Navigate to **Gateway Management** → **Rate Limit**
2. View configured policies
3. Check current limits

---

## 7. Monitoring and Logs

### 7.1 System Monitor

Access real-time system metrics:

1. Navigate to **Monitoring** → **System Monitor**
2. View:
   - System health status
   - Performance metrics
   - Active connections

### 7.2 Audit Log

View administrative action history:

1. Navigate to **Monitoring** → **Audit Log**
2. Filter by:
   - User
   - Action type
   - Date range
3. View action details

### 7.3 System Log

View application logs:

1. Navigate to **Monitoring** → **System Log**
2. Filter by:
   - Log level
   - Message content
   - Time range
3. View full log details

### 7.4 Alert Management

Manage system alerts:

1. Navigate to **Monitoring** → **Alert Management**
2. View alert rules
3. Check alert history

---

## 8. FAQ

### Q: How do I change my password?

1. Click your avatar → **Profile**
2. Navigate to **Security** tab
3. Click **Change Password**
4. Enter current and new password
5. Click **Confirm**

### Q: Why can't I access certain features?

Your access is determined by your assigned roles. Contact your administrator to request additional permissions.

### Q: How do I enable two-factor authentication?

1. Go to **Profile** → **Security**
2. Click **Enable 2FA**
3. Follow the setup instructions

### Q: What should I do if I'm locked out?

Contact your system administrator to unlock your account.

### Q: How can I view my login history?

1. Go to **Profile** → **Security**
2. View **Recent Activity** section

---

*For administrative tasks, see the [Administrator Guide](ADMIN_GUIDE.md).*
*For technical details, see the [Developer Guide](DEVELOPER_GUIDE.md).*
