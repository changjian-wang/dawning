# RBAC (Role-Based Access Control) Implementation

## Overview
This document describes the complete implementation of Role-Based Access Control (RBAC) in the Dawning Identity system.

## Implementation Date
December 9, 2025

## Database Schema

### Roles Table
Created in `sql/005_create_roles_table.sql`

**Structure:**
- `id` (CHAR(36)): Primary key, UUID
- `name` (VARCHAR(50)): Unique role name
- `display_name` (VARCHAR(100)): Display name
- `description` (TEXT): Role description
- `is_system` (TINYINT): System role flag (cannot be modified/deleted)
- `is_active` (TINYINT): Active status
- `permissions` (JSON): Array of permissions in format ["resource:action:scope"]
- `created_at`, `updated_at`, `created_by`, `updated_by`: Audit fields

**Predefined Roles:**
1. **super_admin**: Full system access with all permissions
2. **admin**: Administrative access for user and application management
3. **user_manager**: User CRUD operations
4. **auditor**: Read-only access to all resources
5. **user**: Self-management only

### User-Roles Association Table
Created in `sql/006_create_user_roles_table.sql`

**Structure:**
- `user_id` (CHAR(36)): Foreign key to users table
- `role_id` (CHAR(36)): Foreign key to roles table
- `created_at`, `created_by`: Audit fields
- Primary key on (user_id, role_id)
- Cascade delete on both foreign keys

**Initial Data:**
- admin user → admin role
- Test users (zhangsan, lisi, wangwu, zhaoliu, sunqi) → user role

## Backend Implementation

### 1. Authentication Layer Updates

#### UserAuthenticationDto (`Application/Dtos/Authentication/UserAuthenticationDto.cs`)
Changed from single `Role` string to `List<string> Roles`:
```csharp
public List<string> Roles { get; set; } = new();
```

#### UserAuthenticationService (`Application/Services/Authentication/UserAuthenticationService.cs`)
Updated all methods to load user roles from database:
- `ValidateCredentialsAsync()`: Loads roles after password validation
- `GetUserByIdAsync()`: Loads roles when fetching user by ID
- `GetByUsernameAsync()`: Loads roles when fetching by username

Uses `_userService.GetUserRolesAsync(userId)` to fetch roles from user_roles table.

### 2. JWT Token Generation

#### AuthController (`Api/Controllers/AuthController.cs`)
Updated `HandlePasswordGrantAsync()` to add all user roles as claims:

```csharp
// Add all user roles as role claims
foreach (var role in user.Roles)
{
    identity.AddClaim(new Claim(Claims.Role, role));
}
```

Each role becomes a separate claim in the JWT token, enabling ASP.NET Core's built-in role-based authorization.

### 3. Controller Authorization

Applied `[Authorize(Roles = "...")]` attributes to all controllers:

#### Administration Controllers (Admin-only access)
All require `admin` or `super_admin` role:
- **RoleController**: All role management operations
- **ClaimTypeController**: Claim type management
- **SystemConfigController**: System config management
- **ApplicationController**: OAuth2 application management
- **ScopeController**: OAuth2 scope management

```csharp
[Authorize(Roles = "admin,super_admin")]
```

#### UserController (Granular authorization)
Different operations have different role requirements:

**Read Operations:**
- `GetUserList()`: `admin,super_admin,user_manager,auditor`
- Auditors can view user lists but cannot modify

**Write Operations:**
- `CreateUser()`: `admin,super_admin,user_manager`
- `UpdateUser()`: `admin,super_admin,user_manager`
- `DeleteUser()`: `admin,super_admin,user_manager`
- `ResetPassword()`: `admin,super_admin,user_manager`

**Role Management Operations:**
- `AssignRoles()`: `admin,super_admin` only
- `RemoveRole()`: `admin,super_admin` only

**User Info:**
- `GetCurrentUserInfo()`: All authenticated users (base `[Authorize]`)

### 4. User Information DTO Updates

#### UserInfoDto (`Application/Dtos/User/UserInfoDto.cs`)
Changed to support multiple roles:
```csharp
public List<string> Roles { get; set; } = new();
```

This affects the `/api/user/info` endpoint response structure.

## Role-Based Access Matrix

| Controller/Endpoint | super_admin | admin | user_manager | auditor | user |
|---------------------|-------------|-------|--------------|---------|------|
| **User Management** |
| List Users | ✅ | ✅ | ✅ | ✅ | ❌ |
| Create User | ✅ | ✅ | ✅ | ❌ | ❌ |
| Update User | ✅ | ✅ | ✅ | ❌ | ❌ |
| Delete User | ✅ | ✅ | ✅ | ❌ | ❌ |
| Reset Password | ✅ | ✅ | ✅ | ❌ | ❌ |
| View Own Info | ✅ | ✅ | ✅ | ✅ | ✅ |
| Assign Roles | ✅ | ✅ | ❌ | ❌ | ❌ |
| Remove Role | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Role Management** |
| All Operations | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Claim Types** |
| All Operations | ✅ | ✅ | ❌ | ❌ | ❌ |
| **System Metadata** |
| All Operations | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Applications** |
| All Operations | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Scopes** |
| All Operations | ✅ | ✅ | ❌ | ❌ | ❌ |

## Files Modified/Created

### Created Files:
1. `sql/005_create_roles_table.sql` - Role table schema and data
2. `sql/006_create_user_roles_table.sql` - User-role association table
3. Domain layer: `Role.cs`, `UserRole.cs`, `RoleModel.cs`, `IRoleRepository.cs`, `IUserRoleRepository.cs`
4. Infrastructure: `RoleEntity.cs`, `RoleRepository.cs`, `UserRoleEntity.cs`, `UserRoleRepository.cs`, `RoleMappers.cs`
5. Application: `RoleDto.cs`, `RoleService.cs`, `UserRoleDto.cs`, etc.
6. API: `RoleController.cs`

### Modified Files:
1. **Authentication:**
   - `UserAuthenticationDto.cs`: Role → Roles
   - `UserAuthenticationService.cs`: Load roles from database
   - `AuthController.cs`: Add all roles as claims to JWT

2. **User Information:**
   - `UserInfoDto.cs`: Role → Roles
   - `UserController.cs`: Updated GetCurrentUserInfo()

3. **Authorization:**
   - `UserController.cs`: Added role-based [Authorize] attributes
   - `RoleController.cs`: Added [Authorize(Roles = "admin,super_admin")]
   - `ClaimTypeController.cs`: Added [Authorize(Roles = "admin,super_admin")]
   - `SystemConfigController.cs`: Added [Authorize(Roles = "admin,super_admin")]
   - `ApplicationController.cs`: Added [Authorize(Roles = "admin,super_admin")]
   - `ScopeController.cs`: Added [Authorize(Roles = "admin,super_admin")]

4. **Repository Layer:**
   - `IUnitOfWork.cs`: Added Role and UserRole repositories
   - `UnitOfWork.cs`: Registered new repositories
   - `IUserService.cs`: Added role management methods
   - `UserService.cs`: Implemented role operations

## Testing Recommendations

### 1. Login with Different Roles
Test with existing users:
- Username: `admin`, Password: `admin` (has admin role)
- Other test users have user role

### 2. API Endpoint Tests

**Test Admin Access:**
```bash
# Login as admin
POST /connect/token
{
  "grant_type": "password",
  "username": "admin",
  "password": "admin"
}

# Try admin endpoints (should succeed)
GET /api/role
GET /api/claim-type
POST /api/user
```

**Test Regular User Access:**
```bash
# Login as regular user (zhangsan)
POST /connect/token
{
  "grant_type": "password",
  "username": "zhangsan",
  "password": "123456"
}

# Try admin endpoints (should return 403 Forbidden)
GET /api/role
POST /api/user
```

**Test Auditor Access:**
Create a test user with auditor role:
```bash
# Should be able to list users
GET /api/user

# Should NOT be able to create users (403)
POST /api/user
```

### 3. JWT Token Inspection
Decode the JWT token to verify roles are included:
```json
{
  "sub": "user-id",
  "name": "admin",
  "email": "admin@example.com",
  "role": ["admin"],  // Multiple roles if user has multiple
  "exp": 1234567890
}
```

## Security Considerations

### 1. System Roles Protection
System roles (is_system = true) cannot be:
- Modified via API
- Deleted via API
- Implemented in `RoleService.cs`

### 2. Cascade Deletion
When a user is deleted, all their role associations are automatically removed (CASCADE DELETE).
When a role is deleted, all user associations are automatically removed.

### 3. Role Assignment Restrictions
Only `admin` and `super_admin` can assign/remove roles from users.

### 4. Multi-Role Support
Users can have multiple roles simultaneously, and the JWT token includes all roles as separate claims.

## Future Enhancements

1. **Permission-Based Authorization**: Currently uses role names. Could implement custom authorization handlers to check specific permissions from the roles.permissions JSON field.

2. **Dynamic Policy Configuration**: Configure policies from database rather than hard-coded attributes.

3. **Role Hierarchy**: Implement role inheritance (e.g., super_admin inherits all admin permissions).

4. **Audit Logging**: Log all role assignments, modifications, and authorization failures.

5. **Frontend Integration**: Update Vue.js frontend to handle multiple roles per user.

## Migration Notes

### Breaking Changes:
1. **API Response Structure**: `/api/user/info` now returns `roles` (array) instead of `role` (string)
2. **JWT Token Structure**: Token now contains multiple `role` claims instead of single claim

### Backward Compatibility:
None. This is a breaking change requiring frontend updates to handle array of roles.

## Conclusion

The RBAC implementation provides:
- ✅ Database schema with many-to-many user-role relationship
- ✅ Complete role management API
- ✅ JWT token integration with multiple roles
- ✅ Controller-level authorization with granular permissions
- ✅ System role protection
- ✅ Comprehensive role-based access matrix

All code compiles successfully and is ready for testing.
