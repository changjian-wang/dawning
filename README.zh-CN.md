# Dawning Identity Gateway

<p align="center">
  <img src="apps/admin/src/assets/images/logo-full.svg" alt="Dawning Logo" width="320" />
</p>

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Vue](https://img.shields.io/badge/Vue-3.5-4FC08D?logo=vue.js)
![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript)
![MySQL](https://img.shields.io/badge/MySQL-8.x-4479A1?logo=mysql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7.x-DC382D?logo=redis&logoColor=white)
![Kafka](https://img.shields.io/badge/Kafka-3.x-231F20?logo=apachekafka)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)
![Kubernetes](https://img.shields.io/badge/Kubernetes-Helm-326CE5?logo=kubernetes)
![License](https://img.shields.io/badge/License-MIT-green)

**企业级统一身份认证与 API 网关管理平台**

中文 | [English](README.md)

</div>

---

## 📖 项目简介

Dawning Identity Gateway 是一个基于 .NET 8 和 Vue 3 构建的现代化身份认证与 API 网关管理系统。采用领域驱动设计（DDD）架构，集成 OpenIddict 实现 OAuth 2.0/OpenID Connect 标准认证流程，为企业提供统一的身份认证中心和 API 网关服务。

## ✨ 核心特性

- 🔐 **OAuth 2.0 / OIDC** - 完整的 OpenIddict 集成，支持多种授权流程
- 👥 **用户管理** - 用户 CRUD、角色分配、状态管理、系统用户保护
- 🛡️ **RBAC 权限** - 基于角色的访问控制，细粒度权限管理
- 🔒 **安全策略** - 密码策略、登录锁定、审计日志
- 📊 **管理后台** - 基于 Arco Design Pro Vue 的现代化管理界面
- 🚀 **高性能** - Dapper ORM，优化的查询构建器
- 🌐 **API 网关** - 基于 YARP 的反向代理，动态路由管理
- 🔗 **多系统接入** - 为其他业务系统提供统一认证服务
- 🐳 **容器化部署** - Docker Compose 一键部署，Helm Chart 支持 K8s

## 🏗️ 技术栈

### 后端 (Dawning.Gateway)

| 技术 | 说明 |
|------|------|
| .NET 8 | 运行时框架 |
| ASP.NET Core | Web API 框架 |
| OpenIddict | OAuth 2.0 / OIDC 服务器 |
| Dapper | 轻量级 ORM |
| MySQL | 数据库 |
| AutoMapper | 对象映射 |
| xUnit + Moq | 单元测试 |

### 前端 (dawning-admin)

| 技术 | 说明 |
|------|------|
| Vue 3 | 前端框架 |
| TypeScript | 类型安全 |
| Arco Design | UI 组件库 |
| Pinia | 状态管理 |
| Vue Router | 路由管理 |
| Vite | 构建工具 |

## 📁 项目结构

```
dawning/
├── apps/
│   ├── admin/                       # Vue 3 前端管理系统
│   │   ├── src/
│   │   │   ├── api/                 # API 接口
│   │   │   ├── views/               # 页面视图
│   │   │   ├── store/               # 状态管理
│   │   │   └── router/              # 路由配置
│   │   └── config/                  # 构建配置
│   └── gateway/                     # .NET 8 后端服务
│       ├── src/
│       │   ├── Dawning.Gateway.Api/     # API 网关 (YARP)
│       │   ├── Dawning.Identity.Api/    # 身份认证 API
│       │   ├── Dawning.Identity.Application/  # 应用层
│       │   ├── Dawning.Identity.Domain/       # 领域层
│       │   └── Dawning.Identity.Domain.Core/  # 领域核心
│       └── docs/                    # API 文档
├── sdk/                             # Dawning SDK 组件
├── deploy/
│   ├── docker/                      # Docker Compose 配置
│   ├── helm/                        # Kubernetes Helm Chart
│   │   └── dawning/
│   │       ├── Chart.yaml
│   │       ├── values.yaml          # 默认配置
│   │       ├── values-dev.yaml      # 开发环境
│   │       └── values-prod.yaml     # 生产环境
│   └── scripts/                     # 部署脚本
├── docs/                            # 项目文档
└── .github/workflows/               # CI/CD 配置
```

## 🚀 快速开始

### 环境要求

- .NET 8.0 SDK
- Node.js 18+
- MySQL 8.0+
- pnpm (推荐) 或 npm
- Docker & Docker Compose (可选)

### 方式一：Docker Compose 一键启动（推荐）

```bash
cd deploy/docker

# 启动基础设施
docker-compose up -d mysql redis zookeeper kafka

# 启动所有服务
docker-compose --profile all up -d --build
```

这将启动：MySQL、Redis、Zookeeper、Kafka、Kafka UI 和后端服务。

### 方式二：手动启动

**后端启动**

```bash
# 1. 进入后端目录
cd apps/gateway

# 2. 还原依赖
dotnet restore

# 3. 配置数据库连接 (修改 appsettings.json)

# 4. 运行数据库迁移
mysql -u <user> -p <database> < docs/sql/migrations/001_initial_schema.sql

# 5. 启动 Identity API
cd src/Dawning.Identity.Api
dotnet run
```

**前端启动**

```bash
# 1. 进入前端目录
cd apps/admin

# 2. 安装依赖
pnpm install

# 3. 启动开发服务器
pnpm dev
```

### 默认账户

| 用户名 | 密码 | 角色 |
|--------|------|------|
| admin | Admin@123 | 系统管理员 |

## 🐳 Docker 部署

```bash
cd deploy/docker

# 启动基础设施
docker-compose up -d mysql redis zookeeper kafka

# 启动所有服务
docker-compose --profile all up -d --build

# 停止服务
docker-compose down

# 清理数据
docker-compose down -v
```

## ☸️ Kubernetes 部署

```bash
# 添加 Bitnami 仓库
helm repo add bitnami https://charts.bitnami.com/bitnami

# 更新依赖
cd deploy/helm/dawning && helm dependency update

# 开发环境部署
helm install dawning ./deploy/helm/dawning -f ./deploy/helm/dawning/values-dev.yaml -n dawning-dev --create-namespace

# 生产环境部署
helm install dawning ./deploy/helm/dawning -f ./deploy/helm/dawning/values-prod.yaml -n dawning --create-namespace
```

## 🔗 业务系统接入

其他业务系统可以通过 Dawning SDK 轻松接入统一认证：

**1. 添加 NuGet 包**
```xml
<PackageReference Include="Dawning.Identity" Version="1.2.0" />
```

**2. 配置认证**
```csharp
builder.Services.AddDawningAuthentication(builder.Configuration);
app.UseAuthentication();
app.UseAuthorization();
```

**3. 使用认证**
```csharp
[Authorize]
[HttpGet]
public IActionResult GetData() => Ok();
```

详见 [认证接入指南](docs/AUTHENTICATION_INTEGRATION.zh-CN.md)。

## 🧪 测试

```bash
# 运行单元测试
cd apps/gateway
dotnet test

# 测试覆盖: 52 个测试用例
# - UserServiceTests: 8 tests
# - RoleServiceTests: 11 tests
# - PermissionServiceTests: 9 tests
# - LoginLockoutServiceTests: 9 tests
# - PasswordPolicyServiceTests: 13 tests
```

## 📚 API 文档

启动后端后访问 Swagger UI：
- Identity API: `http://localhost:5202/swagger`
- Gateway API: `http://localhost:5000/swagger`

## 📖 文档导航

| 文档 | 说明 |
|------|------|
| [认证接入指南](docs/AUTHENTICATION_INTEGRATION.zh-CN.md) | 业务系统接入统一认证 |
| [Identity API](apps/gateway/docs/IDENTITY_API.zh-CN.md) | OAuth2/OIDC 端点说明 |
| [开发者指南](docs/DEVELOPER_GUIDE.zh-CN.md) | 开发规范与约定 |
| [部署文档](docs/DEPLOYMENT.zh-CN.md) | 生产环境部署 |
| [Helm 部署](deploy/helm/README.zh-CN.md) | Kubernetes 部署指南 |
| [用户指南](docs/USER_GUIDE.zh-CN.md) | 管理后台使用说明 |

## 🔧 配置说明

主要配置文件：`appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=dawning_identity;..."
  },
  "Security": {
    "Password": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireDigit": true
    },
    "Lockout": {
      "Enabled": true,
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 30
    }
  }
}
```

## 📄 许可证

本项目采用 [MIT 许可证](LICENSE)。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！
