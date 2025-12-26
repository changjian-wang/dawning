# Dawning 网关管理系统 - 部署指南

## 目录

- [快速开始](#快速开始)
- [环境要求](#环境要求)
- [Docker 部署](#docker-部署)
- [手动部署](#手动部署)
- [配置说明](#配置说明)
- [生产环境配置](#生产环境配置)
- [故障排查](#故障排查)

---

## 快速开始

### 使用 Docker Compose (推荐)

```bash
# 1. 克隆项目
git clone https://github.com/changjian-wang/dawning.git
cd dawning

# 2. 复制环境配置文件
cp .env.example .env

# 3. 修改 .env 文件中的敏感信息
# 编辑 MYSQL_PASSWORD, MYSQL_ROOT_PASSWORD 等

# 4. 启动所有服务
docker-compose up -d

# 5. 查看服务状态
docker-compose ps

# 6. 查看日志
docker-compose logs -f
```

服务启动后访问:
- **管理后台**: http://localhost
- **Identity API**: http://localhost:5001
- **Gateway API**: http://localhost:5000

---

## 环境要求

### 最低配置

| 组件 | 版本 | 说明 |
|------|------|------|
| Docker | 20.10+ | 容器运行时 |
| Docker Compose | 2.0+ | 容器编排 |
| 内存 | 4GB+ | 推荐 8GB |
| 磁盘 | 20GB+ | 日志和数据库存储 |

### 开发环境

| 组件 | 版本 |
|------|------|
| .NET SDK | 8.0 |
| Node.js | 20+ |
| pnpm | 8+ |
| MySQL | 8.0 |
| Redis | 7+ |

---

## Docker 部署

### 1. 生产环境部署

```bash
# 使用生产配置
docker-compose -f docker-compose.yml up -d

# 或者指定环境文件
docker-compose --env-file .env.production up -d
```

### 2. 开发环境部署

```bash
# 使用开发配置（支持热重载）
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
```

### 3. 单独构建镜像

```bash
# 构建 Identity API
docker build -t dawning-identity-api:latest \
  -f Dawning.Gateway/src/Dawning.Identity.Api/Dockerfile \
  ./Dawning.Gateway

# 构建 Gateway API
docker build -t dawning-gateway-api:latest \
  -f Dawning.Gateway/src/Dawning.Gateway.Api/Dockerfile \
  ./Dawning.Gateway

# 构建前端
docker build -t dawning-admin:latest ./dawning-admin
```

### 4. 推送到镜像仓库

```bash
# 登录 GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# 打标签
docker tag dawning-identity-api:latest ghcr.io/changjian-wang/dawning-identity-api:latest

# 推送
docker push ghcr.io/changjian-wang/dawning-identity-api:latest
```

---

## 手动部署

### 1. 后端服务

```bash
# 进入后端目录
cd Dawning.Gateway

# 还原依赖
dotnet restore

# 构建
dotnet build -c Release

# 发布 Identity API
dotnet publish src/Dawning.Identity.Api -c Release -o ./publish/identity-api

# 发布 Gateway API
dotnet publish src/Dawning.Gateway.Api -c Release -o ./publish/gateway-api

# 运行
cd publish/identity-api
dotnet Dawning.Identity.Api.dll
```

### 2. 前端服务

```bash
# 进入前端目录
cd dawning-admin

# 安装依赖
pnpm install

# 构建
pnpm build

# 部署到 Nginx
cp -r dist/* /var/www/html/
```

---

## 配置说明

### 环境变量

| 变量名 | 默认值 | 说明 |
|--------|--------|------|
| `MYSQL_ROOT_PASSWORD` | dawning_root_2024 | MySQL root 密码 |
| `MYSQL_DATABASE` | dawning_identity | 数据库名称 |
| `MYSQL_USER` | dawning | 数据库用户 |
| `MYSQL_PASSWORD` | dawning_password_2024 | 数据库密码 |
| `REDIS_PORT` | 6379 | Redis 端口 |
| `IDENTITY_API_PORT` | 5001 | Identity API 端口 |
| `GATEWAY_API_PORT` | 5000 | Gateway API 端口 |
| `FRONTEND_PORT` | 80 | 前端端口 |

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=mysql;Port=3306;Database=dawning_identity;Uid=dawning;Pwd=your_secure_password;"
  },
  "Redis": {
    "Connection": "redis:6379"
  },
  "OpenIddict": {
    "UseDevelopmentCertificate": false,
    "Certificates": {
      "Signing": {
        "Source": "File",
        "Path": "/app/certs/signing.pfx",
        "Password": "your_cert_password"
      },
      "Encryption": {
        "Source": "File",
        "Path": "/app/certs/encryption.pfx",
        "Password": "your_cert_password"
      }
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## 生产环境配置

### 1. 生成证书

```bash
# 创建证书目录
mkdir -p certs

# 生成签名证书
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 \
  -keyout certs/signing.key -out certs/signing.crt \
  -subj "/CN=Dawning Signing Certificate" \
  -nodes

# 转换为 PFX 格式
openssl pkcs12 -export -out certs/signing.pfx \
  -inkey certs/signing.key -in certs/signing.crt \
  -password pass:your_cert_password

# 生成加密证书
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 \
  -keyout certs/encryption.key -out certs/encryption.crt \
  -subj "/CN=Dawning Encryption Certificate" \
  -nodes

openssl pkcs12 -export -out certs/encryption.pfx \
  -inkey certs/encryption.key -in certs/encryption.crt \
  -password pass:your_cert_password
```

### 2. HTTPS 配置

使用反向代理 (如 Traefik 或 Nginx) 来终止 SSL：

```yaml
# docker-compose.override.yml
services:
  traefik:
    image: traefik:v2.10
    command:
      - "--api.insecure=true"
      - "--providers.docker=true"
      - "--entrypoints.websecure.address=:443"
      - "--certificatesresolvers.letsencrypt.acme.tlschallenge=true"
      - "--certificatesresolvers.letsencrypt.acme.email=admin@example.com"
      - "--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json"
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - ./letsencrypt:/letsencrypt
```

### 3. 数据库备份

```bash
# 备份数据库
docker exec dawning-mysql mysqldump -u root -p$MYSQL_ROOT_PASSWORD \
  dawning_identity > backup_$(date +%Y%m%d).sql

# 恢复数据库
docker exec -i dawning-mysql mysql -u root -p$MYSQL_ROOT_PASSWORD \
  dawning_identity < backup_20241216.sql
```

### 4. 日志管理

```bash
# 查看服务日志
docker-compose logs -f identity-api

# 日志轮转配置
docker-compose logs --tail=1000 gateway-api

# 清理旧日志
docker system prune -f
```

---

## 故障排查

### 常见问题

#### 1. 数据库连接失败

```bash
# 检查 MySQL 容器状态
docker-compose ps mysql

# 查看 MySQL 日志
docker-compose logs mysql

# 测试连接
docker exec -it dawning-mysql mysql -u dawning -p
```

#### 2. 服务健康检查失败

```bash
# 检查服务健康状态
docker inspect --format='{{.State.Health.Status}}' dawning-identity-api

# 手动测试健康端点
curl http://localhost:5001/health
```

#### 3. 前端无法连接后端

```bash
# 检查网络连接
docker network inspect dawning_dawning-network

# 检查 nginx 配置
docker exec dawning-admin-frontend cat /etc/nginx/conf.d/default.conf
```

#### 4. 权限问题

```bash
# 修复文件权限
sudo chown -R 1001:1001 ./certs
sudo chmod 600 ./certs/*.pfx
```

### 日志位置

| 服务 | 容器内路径 | 宿主机挂载 |
|------|-----------|-----------|
| Identity API | /app/Logs | identity_logs 卷 |
| Gateway API | /app/Logs | gateway_logs 卷 |
| MySQL | /var/log/mysql | mysql_data 卷 |
| Nginx | /var/log/nginx | - |

### 性能调优

```yaml
# docker-compose.override.yml
services:
  mysql:
    command: >
      --innodb_buffer_pool_size=512M
      --max_connections=1000
      --query_cache_size=64M
  
  identity-api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
```

---

## CI/CD

项目已配置 GitHub Actions 自动化流程：

- **PR 检查**: 构建验证、类型检查、提交消息规范检查
- **主分支推送**: 自动构建、测试、发布 Docker 镜像
- **安全扫描**: Trivy 漏洞扫描

查看 `.github/workflows/` 目录了解详细配置。

---

## 联系支持

如遇到问题，请提交 Issue 或联系开发团队。
