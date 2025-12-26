# Authentication Integration Guide

**Version**: 1.0.0  
**Last Updated**: 2025-12-26

---

## Table of Contents

1. [Overview](#1-overview)
2. [OpenIddict Configuration](#2-openiddict-configuration)
3. [OAuth 2.0 Flows](#3-oauth-20-flows)
4. [Client Integration](#4-client-integration)
5. [Token Management](#5-token-management)
6. [Security Best Practices](#6-security-best-practices)

---

## 1. Overview

Dawning Gateway uses **OpenIddict** as the OAuth 2.0 / OpenID Connect server, supporting:

- Authorization Code Flow (with PKCE)
- Client Credentials Flow
- Resource Owner Password Flow
- Refresh Token Flow

---

## 2. OpenIddict Configuration

### 2.1 Supported Grant Types

| Grant Type | Use Case |
|------------|----------|
| `client_credentials` | Server-to-server authentication |
| `password` | Legacy applications (not recommended) |
| `authorization_code` | Web applications |
| `refresh_token` | Token renewal |

### 2.2 Creating OAuth Clients

Via Admin Panel:
1. Navigate to **OpenIddict** > **Applications**
2. Click **Add Application**
3. Configure:
   - **Client ID**: Unique identifier
   - **Client Secret**: For confidential clients
   - **Grant Types**: Select appropriate flows
   - **Redirect URIs**: For authorization code flow
   - **Scopes**: Requested permissions

---

## 3. OAuth 2.0 Flows

### 3.1 Client Credentials Flow

For machine-to-machine authentication:

```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id=your-client-id
&client_secret=your-client-secret
&scope=api
```

Response:
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

### 3.2 Authorization Code Flow (with PKCE)

**Step 1: Generate Code Verifier and Challenge**

```javascript
// Generate random code verifier (43-128 chars)
const codeVerifier = generateRandomString(64);

// Create SHA256 hash and base64url encode
const codeChallenge = base64UrlEncode(sha256(codeVerifier));
```

**Step 2: Authorization Request**

```http
GET /connect/authorize
  ?response_type=code
  &client_id=your-client-id
  &redirect_uri=https://your-app.com/callback
  &scope=openid profile api
  &state=random-state-value
  &code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM
  &code_challenge_method=S256
```

**Step 3: Token Exchange**

```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&client_id=your-client-id
&code=authorization-code-from-callback
&redirect_uri=https://your-app.com/callback
&code_verifier=your-original-code-verifier
```

### 3.3 Resource Owner Password Flow

⚠️ **Not recommended for new applications**

```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=your-client-id
&client_secret=your-client-secret
&username=user@example.com
&password=user-password
&scope=openid profile api
```

---

## 4. Client Integration

### 4.1 JavaScript / TypeScript

```typescript
// Using fetch API
async function getToken() {
  const response = await fetch('/connect/token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams({
      grant_type: 'client_credentials',
      client_id: 'your-client-id',
      client_secret: 'your-client-secret',
      scope: 'api'
    })
  });
  
  return response.json();
}

// Using the token
async function callApi(accessToken: string) {
  const response = await fetch('/api/resource', {
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  });
  
  return response.json();
}
```

### 4.2 C# / .NET

```csharp
using var client = new HttpClient();

// Get token
var tokenResponse = await client.PostAsync("/connect/token", 
    new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "client_credentials",
        ["client_id"] = "your-client-id",
        ["client_secret"] = "your-client-secret",
        ["scope"] = "api"
    }));

var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

// Use token
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token.AccessToken);

var response = await client.GetAsync("/api/resource");
```

### 4.3 Python

```python
import requests

# Get token
token_response = requests.post(
    'https://your-gateway.com/connect/token',
    data={
        'grant_type': 'client_credentials',
        'client_id': 'your-client-id',
        'client_secret': 'your-client-secret',
        'scope': 'api'
    }
)
token = token_response.json()

# Use token
headers = {'Authorization': f"Bearer {token['access_token']}"}
response = requests.get('https://your-gateway.com/api/resource', headers=headers)
```

---

## 5. Token Management

### 5.1 Token Lifetime

Default token lifetimes:
- Access Token: 1 hour
- Refresh Token: 14 days
- ID Token: 1 hour

### 5.2 Refresh Tokens

```http
POST /connect/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=your-client-id
&client_secret=your-client-secret
&refresh_token=your-refresh-token
```

### 5.3 Token Revocation

```http
POST /connect/revoke HTTP/1.1
Content-Type: application/x-www-form-urlencoded

token=access-or-refresh-token
&client_id=your-client-id
&client_secret=your-client-secret
```

---

## 6. Security Best Practices

### 6.1 Client Security

- ✅ Use HTTPS in production
- ✅ Store client secrets securely
- ✅ Use PKCE for public clients
- ✅ Rotate secrets periodically
- ❌ Never expose secrets in client-side code

### 6.2 Token Security

- ✅ Use short-lived access tokens
- ✅ Validate token signatures
- ✅ Check token expiration
- ✅ Verify token audience and issuer
- ❌ Never log tokens

### 6.3 Scope Management

Request only necessary scopes:

```http
scope=api:read api:write
```

Define granular scopes in your application.

---

## Endpoints Reference

| Endpoint | Purpose |
|----------|---------|
| `/connect/authorize` | Authorization endpoint |
| `/connect/token` | Token endpoint |
| `/connect/revoke` | Token revocation |
| `/connect/introspect` | Token introspection |
| `/connect/userinfo` | User info (OIDC) |
| `/.well-known/openid-configuration` | Discovery document |

---

*See [Developer Guide](DEVELOPER_GUIDE.md) for implementation details.*
