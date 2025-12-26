# Dawning Identity API - OpenIddict 认证服务器

## 概述

Dawning Identity API 是基于 OpenIddict 实现的 OAuth 2.0 / OpenID Connect 认证服务器，为 Dawning 系统提供统一的身份认证和授权服务。

## 功能特性

- ✅ **OAuth 2.0 / OpenID Connect** 标准协议支持
- ✅ **多种授权流程**：
  - Password Grant (资源拥有者密码凭证)
  - Client Credentials Grant (客户端凭证)
  - Authorization Code Grant (授权码)
  - Refresh Token Grant (刷新令牌)
- ✅ **Token 管理**：JWT 令牌生成、验证、刷新
- ✅ **用户信息端点**：提供用户基本信息查询
- ✅ **作用域管理**：支持 openid、profile、email、roles 和自定义作用域
- ✅ **应用管理 API**：管理客户端应用、授权、令牌等

## 认证端点

### 1. Token 端点
**POST** `/connect/token`

获取访问令牌，支持多种授权类型。

#### 密码授权 (Password Grant)
```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=password
&username=admin
&password=admin
&scope=openid profile email api
&client_id=dawning-admin
```

**响应示例：**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "...",
  "scope": "openid profile email api"
}
```

#### 客户端凭证授权 (Client Credentials)
```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id=dawning-api
&client_secret=dawning-api-secret
&scope=api
```

#### 刷新令牌 (Refresh Token)
```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&refresh_token=<your_refresh_token>
&client_id=dawning-admin
```

### 2. UserInfo 端点
**GET** `/connect/userinfo`

获取当前用户信息（需要认证）。

```http
GET /connect/userinfo HTTP/1.1
Authorization: Bearer <access_token>
```

**响应示例：**
```json
{
  "sub": "1",
  "name": "admin",
  "email": "admin@dawning.com"
}
```

### 3. 授权端点
**GET/POST** `/connect/authorize`

用于授权码流程的授权端点。

### 4. 登出端点
**GET/POST** `/connect/logout`

登出当前用户会话。

## 管理 API

### 应用管理

#### 获取所有应用
```http
GET /api/openiddict/application/get-all
```

#### 根据 ID 获取应用
```http
GET /api/openiddict/application/get/{id}
```

#### 根据 ClientId 获取应用
```http
GET /api/openiddict/application/get-by-client-id/{clientId}
```

#### 创建应用
```http
POST /api/openiddict/application/insert
Content-Type: application/json

{
  "clientId": "my-app",
  "displayName": "My Application",
  "type": "public",
  "permissions": [
    "gt:password",
    "ept:token",
    "scp:openid",
    "scp:profile"
  ],
  "redirectUris": ["https://myapp.com/callback"]
}
```

### 作用域管理

#### 获取所有作用域
```http
GET /api/openiddict/scope/get-all
```

#### 根据名称获取作用域
```http
GET /api/openiddict/scope/get-by-name/{name}
```

### 授权管理

#### 获取用户授权
```http
GET /api/openiddict/authorization/get-by-subject/{subject}
```

### Token 管理

#### 获取用户 Token
```http
GET /api/openiddict/token/get-by-subject/{subject}
```

#### 清理过期 Token
```http
POST /api/openiddict/token/prune-expired
```

## 默认用户

系统预置了两个测试用户：

| 用户名 | 密码 | 角色 | 邮箱 |
|--------|------|------|------|
| admin | admin | admin | admin@dawning.com |
| user | user123 | user | user@dawning.com |

⚠️ **注意**：这些是演示用户，生产环境应该连接真实的用户数据库。

## 预配置的客户端应用

### dawning-admin (前端应用)
- **ClientId**: `dawning-admin`
- **类型**: Public (公共客户端，无需密钥)
- **支持的授权类型**: Password, Refresh Token, Authorization Code
- **作用域**: openid, profile, email, roles, api
- **回调地址**: 
  - http://localhost:5173/callback
  - https://localhost:5173/callback

### dawning-api (API 服务)
- **ClientId**: `dawning-api`
- **ClientSecret**: `dawning-api-secret`
- **类型**: Confidential (机密客户端)
- **支持的授权类型**: Client Credentials
- **作用域**: api

## 快速开始

### 1. 启动服务
```bash
cd c:\github\dawning\Dawning.Gateway\src\Dawning.Identity.Api
dotnet run
```

服务将运行在：
- HTTPS: https://localhost:7011
- HTTP: http://localhost:5011

### 2. 测试认证流程

运行测试脚本：
```powershell
cd c:\github\dawning\Dawning.Gateway
.\test-auth.ps1
```

或手动测试：
```powershell
# 获取 Token
$response = Invoke-RestMethod -Uri "https://localhost:7011/connect/token" `
    -Method Post `
    -ContentType "application/x-www-form-urlencoded" `
    -Body @{
        grant_type = "password"
        username = "admin"
        password = "admin"
        scope = "openid profile email api"
        client_id = "dawning-admin"
    } `
    -SkipCertificateCheck

$token = $response.access_token

# 获取用户信息
Invoke-RestMethod -Uri "https://localhost:7011/connect/userinfo" `
    -Headers @{ Authorization = "Bearer $token" } `
    -SkipCertificateCheck
```

### 3. 前端集成

前端应用可以使用标准的 OIDC 客户端库（如 `oidc-client-ts`）进行集成。

示例配置：
```javascript
{
  authority: 'https://localhost:7011',
  client_id: 'dawning-admin',
  redirect_uri: 'http://localhost:5173/callback',
  response_type: 'code',
  scope: 'openid profile email api',
  post_logout_redirect_uri: 'http://localhost:5173/login'
}
```

## 配置

### appsettings.json
```json
{
  "ConnectionStrings": {
    "MySQL": "data source=localhost; database=dawning_identity; uid=root; pwd=password"
  },
  "OpenIddict": {
    "UseDevelopmentCertificate": true
  }
}
```

- `UseDevelopmentCertificate`: 开发环境设置为 `true`，生产环境应配置真实证书

## 安全建议

### 开发环境
- ✅ 使用开发证书
- ✅ 禁用 HTTPS 传输安全要求（仅限开发）
- ✅ 使用简单的测试用户

### 生产环境
- ⚠️ **必须**使用真实的 SSL/TLS 证书
- ⚠️ **必须**启用 HTTPS 传输安全要求
- ⚠️ **必须**连接真实的用户数据库
- ⚠️ **必须**使用加密存储密码和密钥
- ⚠️ **必须**实现完整的用户管理功能
- ⚠️ 考虑使用持久化存储（数据库）而不是内存存储
- ⚠️ 实施速率限制和防暴力破解机制
- ⚠️ 定期清理过期的 Token 和授权

## 下一步

### 待实现功能
1. **用户管理**：
   - 连接真实的用户数据库
   - 用户注册、激活、密码重置
   - 多因素认证 (MFA)

2. **持久化存储**：
   - 实现自定义 OpenIddict Store
   - 将应用、授权、Token 存储到 MySQL

3. **增强安全性**：
   - 实施 PKCE (Proof Key for Code Exchange)
   - Token 绑定 (Token Binding)
   - 设备授权流程

4. **管理界面**：
   - 应用管理界面
   - 用户管理界面
   - 授权审计日志

5. **扩展功能**：
   - 社交登录 (Google, GitHub, etc.)
   - 单点登录 (SSO)
   - 联合身份

## 故障排除

### 常见问题

#### 1. 证书错误
```
The SSL connection could not be established
```
**解决方案**：使用 `-SkipCertificateCheck` 参数（仅开发环境）

#### 2. 端口占用
```
Address already in use
```
**解决方案**：修改 `launchSettings.json` 中的端口配置

#### 3. 数据库连接失败
```
Unable to connect to database
```
**解决方案**：检查 `appsettings.json` 中的连接字符串

## 相关资源

- [OpenIddict 官方文档](https://documentation.openiddict.com/)
- [OAuth 2.0 规范](https://oauth.net/2/)
- [OpenID Connect 规范](https://openid.net/connect/)
- [JWT.io](https://jwt.io/) - JWT Token 调试工具

## 许可证

MIT License
