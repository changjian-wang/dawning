# Dawning Identity Database Scripts

## Directory Structure

```
sql/
├── README.md        # This file
├── schema/          # Core table creation scripts (execute in order)
└── seed/            # Initial data scripts (execute after schema)
```

## New Environment Setup

Execute scripts in the following order:

### Step 1: Create Database

```sql
CREATE DATABASE IF NOT EXISTS `dawning_identity` 
DEFAULT CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE dawning_identity;
```

### Step 2: Execute Schema Scripts

Execute all scripts in `schema/` directory in order:

```bash
mysql -u root -p dawning_identity < schema/001_users.sql
mysql -u root -p dawning_identity < schema/002_claim_types_system_configs.sql
mysql -u root -p dawning_identity < schema/003_roles.sql
mysql -u root -p dawning_identity < schema/004_user_roles.sql
mysql -u root -p dawning_identity < schema/005_permissions.sql
mysql -u root -p dawning_identity < schema/006_openiddict.sql
mysql -u root -p dawning_identity < schema/007_api_identity_resources.sql
mysql -u root -p dawning_identity < schema/008_system_logs.sql
mysql -u root -p dawning_identity < schema/009_audit_logs.sql
mysql -u root -p dawning_identity < schema/010_gateway.sql
mysql -u root -p dawning_identity < schema/011_rate_limit.sql
mysql -u root -p dawning_identity < schema/012_alert_rules.sql
mysql -u root -p dawning_identity < schema/013_user_sessions.sql
mysql -u root -p dawning_identity < schema/014_request_logs.sql
mysql -u root -p dawning_identity < schema/015_backup_records.sql
mysql -u root -p dawning_identity < schema/016_multitenancy.sql
```

### Step 3: Execute Seed Scripts

```bash
mysql -u root -p dawning_identity < seed/001_seed_admin_and_roles.sql
```

## Schema Scripts

| File | Description |
|------|-------------|
| 001_users.sql | Users table with authentication fields |
| 002_claim_types_system_configs.sql | Claim types and system configuration |
| 003_roles.sql | Roles table |
| 004_user_roles.sql | User-role association table |
| 005_permissions.sql | Permissions table |
| 006_openiddict.sql | OpenIddict tables (applications, scopes, tokens, authorizations) |
| 007_api_identity_resources.sql | API and Identity resources |
| 008_system_logs.sql | System logs table |
| 009_audit_logs.sql | Audit logs table |
| 010_gateway.sql | Gateway routes and clusters |
| 011_rate_limit.sql | Rate limiting policies |
| 012_alert_rules.sql | Alert rules and history |
| 013_user_sessions.sql | User sessions table |
| 014_request_logs.sql | Request logs table |
| 015_backup_records.sql | Backup records table |
| 016_multitenancy.sql | Multi-tenancy support (tenants table + tenant_id fields) |

## Seed Scripts

| File | Description |
|------|-------------|
| 001_seed_admin_and_roles.sql | Initial admin user, system roles, and default tenant |

## Default Credentials

After running seed scripts:

- **Username:** admin
- **Password:** Admin@123

⚠️ **Important:** Change the admin password immediately after first login!
