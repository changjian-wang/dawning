# Identity API Documentation

This document describes the Identity and Admin APIs provided by Dawning Gateway.

## Base URL

```
https://your-gateway.com/api/admin
```

## Authentication

All Admin APIs require Bearer token authentication:

```http
Authorization: Bearer <access_token>
```

---

## User Management

### List Users

```http
GET /user?page=1&pageSize=20&keyword=
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "userName": "admin",
        "email": "admin@example.com",
        "phoneNumber": "1234567890",
        "roles": ["admin"],
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "total": 100
  }
}
```

### Get User by ID

```http
GET /user/{id}
```

### Create User

```http
POST /user
Content-Type: application/json

{
  "userName": "newuser",
  "email": "user@example.com",
  "password": "Password@123",
  "roleIds": ["role-uuid-1", "role-uuid-2"],
  "isActive": true
}
```

### Update User

```http
PUT /user/{id}
Content-Type: application/json

{
  "email": "updated@example.com",
  "roleIds": ["role-uuid-1"]
}
```

### Delete User

```http
DELETE /user/{id}
```

### Reset Password

```http
POST /user/{id}/reset-password
Content-Type: application/json

{
  "newPassword": "NewPassword@123"
}
```

---

## Role Management

### List Roles

```http
GET /role?page=1&pageSize=20
```

### Get Role by ID

```http
GET /role/{id}
```

### Create Role

```http
POST /role
Content-Type: application/json

{
  "name": "Editor",
  "code": "editor",
  "description": "Can edit content",
  "permissionIds": ["perm-1", "perm-2"]
}
```

### Update Role

```http
PUT /role/{id}
Content-Type: application/json

{
  "name": "Senior Editor",
  "permissionIds": ["perm-1", "perm-2", "perm-3"]
}
```

### Delete Role

```http
DELETE /role/{id}
```

---

## Permission Management

### List Permissions

```http
GET /permission?page=1&pageSize=50
```

### Get Permission Tree

```http
GET /permission/tree
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "System Management",
      "code": "system",
      "type": "menu",
      "children": [
        {
          "id": "uuid",
          "name": "User Management",
          "code": "system:user",
          "type": "menu",
          "children": [
            {
              "id": "uuid",
              "name": "Create User",
              "code": "system:user:create",
              "type": "button"
            }
          ]
        }
      ]
    }
  ]
}
```

### Create Permission

```http
POST /permission
Content-Type: application/json

{
  "name": "Delete User",
  "code": "system:user:delete",
  "type": "button",
  "parentId": "parent-perm-id"
}
```

---

## OpenIddict Management

### List Applications

```http
GET /openiddict/application?page=1&pageSize=20
```

### Get Application

```http
GET /openiddict/application/{id}
```

### Create Application

```http
POST /openiddict/application
Content-Type: application/json

{
  "clientId": "my-client",
  "displayName": "My Application",
  "clientSecret": "secret-value",
  "permissions": [
    "ept:token",
    "gt:client_credentials",
    "rst:token",
    "scp:api"
  ],
  "redirectUris": ["https://app.example.com/callback"],
  "postLogoutRedirectUris": ["https://app.example.com/logout"]
}
```

### Update Application

```http
PUT /openiddict/application/{id}
Content-Type: application/json

{
  "displayName": "Updated Name"
}
```

### Delete Application

```http
DELETE /openiddict/application/{id}
```

### List Scopes

```http
GET /openiddict/scope
```

### Create Scope

```http
POST /openiddict/scope
Content-Type: application/json

{
  "name": "api.read",
  "displayName": "Read API",
  "description": "Permission to read API resources"
}
```

---

## System Configuration

### Get All Configurations

```http
GET /system-config
```

### Get Configuration by Key

```http
GET /system-config/{key}
```

### Update Configuration

```http
PUT /system-config/{key}
Content-Type: application/json

{
  "value": "new-value"
}
```

---

## Audit Logs

### List Audit Logs

```http
GET /audit-log?page=1&pageSize=20&startTime=2024-01-01&endTime=2024-12-31
```

**Query Parameters:**
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)
- `startTime` - Filter start time (ISO 8601)
- `endTime` - Filter end time (ISO 8601)
- `userId` - Filter by user ID
- `action` - Filter by action type

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "uuid",
        "userId": "user-uuid",
        "userName": "admin",
        "action": "create",
        "entityType": "User",
        "entityId": "entity-uuid",
        "details": "Created user 'newuser'",
        "ipAddress": "192.168.1.1",
        "createdAt": "2024-01-01T10:30:00Z"
      }
    ],
    "total": 500
  }
}
```

---

## Error Responses

All APIs return errors in this format:

```json
{
  "success": false,
  "code": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Invalid email format"
    }
  ]
}
```

### Error Codes

| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing or invalid token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 409 | Conflict - Resource already exists |
| 500 | Internal Server Error |

---

## Rate Limiting

API rate limits:
- **Default**: 1000 requests/minute per client
- **Authentication endpoints**: 20 requests/minute per IP

Rate limit headers:
```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1704067200
```

---

See [Authentication Integration](../../docs/AUTHENTICATION_INTEGRATION.md) for OAuth setup.
