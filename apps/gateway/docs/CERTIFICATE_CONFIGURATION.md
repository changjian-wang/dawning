# 生产环境证书配置示例

## 证书配置选项

### 1. 从文件加载 (推荐用于 Docker)

```json
{
  "OpenIddict": {
    "UseDevelopmentCertificate": false,
    "Certificates": {
      "Signing": {
        "Type": "File",
        "Path": "/app/certificates/signing.pfx",
        "Password": "your-password"
      },
      "Encryption": {
        "Type": "File",
        "Path": "/app/certificates/encryption.pfx",
        "Password": "your-password"
      }
    }
  }
}
```

### 2. 从证书存储加载 (推荐用于 Windows Server)

```json
{
  "OpenIddict": {
    "UseDevelopmentCertificate": false,
    "Certificates": {
      "Signing": {
        "Type": "Store",
        "StoreLocation": "CurrentUser",
        "StoreName": "My",
        "Thumbprint": "ABC123..."
      },
      "Encryption": {
        "Type": "Store",
        "StoreLocation": "CurrentUser",
        "StoreName": "My",
        "SubjectName": "CN=dawning-encryption"
      }
    }
  }
}
```

### 3. 从 Azure Key Vault 加载 (未实现)

```json
{
  "OpenIddict": {
    "UseDevelopmentCertificate": false,
    "Certificates": {
      "Signing": {
        "Type": "AzureKeyVault",
        "KeyVaultUrl": "https://your-vault.vault.azure.net/",
        "CertificateName": "dawning-signing"
      }
    }
  }
}
```

## 生成自签名证书（测试用）

### Windows (PowerShell)

```powershell
# 生成签名证书
$cert = New-SelfSignedCertificate -Subject "CN=Dawning Signing" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256 -NotAfter (Get-Date).AddYears(5)
$password = ConvertTo-SecureString -String "YourPassword123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ".\signing.pfx" -Password $password

# 生成加密证书
$cert = New-SelfSignedCertificate -Subject "CN=Dawning Encryption" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec KeyExchange -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256 -NotAfter (Get-Date).AddYears(5)
Export-PfxCertificate -Cert $cert -FilePath ".\encryption.pfx" -Password $password
```

### Linux (OpenSSL)

```bash
# 生成签名证书
openssl req -x509 -newkey rsa:2048 -keyout signing-key.pem -out signing-cert.pem -days 1825 -nodes -subj "/CN=Dawning Signing"
openssl pkcs12 -export -out signing.pfx -inkey signing-key.pem -in signing-cert.pem -password pass:YourPassword123!

# 生成加密证书
openssl req -x509 -newkey rsa:2048 -keyout encryption-key.pem -out encryption-cert.pem -days 1825 -nodes -subj "/CN=Dawning Encryption"
openssl pkcs12 -export -out encryption.pfx -inkey encryption-key.pem -in encryption-cert.pem -password pass:YourPassword123!
```

## Docker 部署配置

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
COPY certificates/ /app/certificates/
ENTRYPOINT ["dotnet", "Dawning.Identity.Api.dll"]
```

### docker-compose.yml

```yaml
services:
  identity-api:
    image: dawning-identity-api:latest
    ports:
      - "5001:8080"
    volumes:
      - ./certificates:/app/certificates:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__MySQL=Server=mysql;Database=dawning_identity;Uid=root;Pwd=password
      - OpenIddict__Certificates__Signing__Password=${CERT_PASSWORD}
      - OpenIddict__Certificates__Encryption__Password=${CERT_PASSWORD}
```

## 证书要求

1. **签名证书**: 用于签署 JWT tokens
   - 算法: RSA 2048+
   - 有效期: 建议 2-5 年
   - 用途: 数字签名

2. **加密证书**: 用于加密敏感数据
   - 算法: RSA 2048+
   - 有效期: 建议 2-5 年
   - 用途: 密钥加密

## 安全最佳实践

1. ✅ 使用强密码保护证书文件
2. ✅ 证书密码存储在环境变量或密钥管理服务中
3. ✅ 定期轮换证书（每 1-2 年）
4. ✅ 限制证书文件访问权限（chmod 400）
5. ✅ 备份证书文件到安全位置
6. ✅ 使用 CA 签发的证书（生产环境）
7. ✅ 监控证书过期时间
