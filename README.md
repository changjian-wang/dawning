# Dawning Identity Gateway

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Vue](https://img.shields.io/badge/Vue-3.5-4FC08D?logo=vue.js)
![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript)
![MySQL](https://img.shields.io/badge/MySQL-8.x-4479A1?logo=mysql&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

**企业级身份认证与授权网关系统**

[English](#english) | [中文](#中文)

</div>

---

## 中文

### 📖 项目简介

Dawning Identity Gateway 是一个基于 .NET 8 和 Vue 3 构建的现代化身份认证与授权管理系统。采用领域驱动设计（DDD）架构，集成 OpenIddict 实现 OAuth 2.0/OpenID Connect 标准认证流程。

### ✨ 核心特性

- 🔐 **OAuth 2.0 / OIDC** - 完整的 OpenIddict 集成，支持多种授权流程
- 👥 **用户管理** - 用户 CRUD、角色分配、状态管理
- 🛡️ **RBAC 权限** - 基于角色的访问控制，细粒度权限管理
- 🔒 **安全策略** - 密码策略、登录锁定、审计日志
- 📊 **管理后台** - 基于 Arco Design Pro Vue 的现代化管理界面
- 🚀 **高性能** - Dapper ORM，优化的查询构建器

### 🏗️ 技术栈

#### 后端 (Dawning.Gateway)
| 技术 | 说明 |
|------|------|
| .NET 8 | 运行时框架 |
| ASP.NET Core | Web API 框架 |
| OpenIddict | OAuth 2.0 / OIDC 服务器 |
| Dapper | 轻量级 ORM |
| MySQL | 数据库 |
| AutoMapper | 对象映射 |
| xUnit + Moq | 单元测试 |

#### 前端 (dawning-admin)
| 技术 | 说明 |
|------|------|
| Vue 3 | 前端框架 |
| TypeScript | 类型安全 |
| Arco Design | UI 组件库 |
| Pinia | 状态管理 |
| Vue Router | 路由管理 |
| Vite | 构建工具 |

### 📁 项目结构

```
dawning/
├── Dawning.Gateway/                 # 后端服务
│   ├── src/
│   │   ├── Dawning.Gateway.Api/     # API 网关
│   │   ├── Dawning.Identity.Api/    # 身份认证 API
│   │   ├── Dawning.Identity.Application/  # 应用层
│   │   ├── Dawning.Identity.Domain/       # 领域层
│   │   ├── Dawning.Identity.Domain.Core/  # 领域核心
│   │   └── Shared/                  # 共享组件
│   └── docs/                        # 文档
├── dawning-admin/                   # 前端管理系统
│   ├── src/
│   │   ├── api/                     # API 接口
│   │   ├── views/                   # 页面视图
│   │   ├── store/                   # 状态管理
│   │   └── router/                  # 路由配置
│   └── config/                      # 构建配置
└── docs/                            # 项目文档
```

### 🚀 快速开始

#### 环境要求

- .NET 8.0 SDK
- Node.js 18+
- MySQL 8.0+
- pnpm (推荐) 或 npm

#### 后端启动

```bash
# 1. 进入后端目录
cd Dawning.Gateway

# 2. 还原依赖
dotnet restore

# 3. 配置数据库连接 (修改 appsettings.json)

# 4. 运行数据库迁移
mysql -u <user> -p <database> < docs/sql/migrations/001_initial_schema.sql

# 5. 启动 Identity API
cd src/Dawning.Identity.Api
dotnet run
```

#### 前端启动

```bash
# 1. 进入前端目录
cd dawning-admin

# 2. 安装依赖
pnpm install

# 3. 启动开发服务器
pnpm dev
```

#### 使用脚本启动 (Windows)

```powershell
# 一键启动后端和前端
./run.ps1
```

### 🧪 测试

```bash
# 运行单元测试
cd Dawning.Gateway
dotnet test

# 测试覆盖: 52 个测试用例
# - UserServiceTests: 8 tests
# - RoleServiceTests: 11 tests
# - PermissionServiceTests: 9 tests
# - LoginLockoutServiceTests: 9 tests
# - PasswordPolicyServiceTests: 13 tests
```

### 📚 API 文档

启动后端后访问 Swagger UI：
- Identity API: `https://localhost:5001/swagger`

### 🔧 配置说明

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

### 📄 许可证

本项目采用 [MIT 许可证](LICENSE)。

---

## English

### 📖 Introduction

Dawning Identity Gateway is a modern identity authentication and authorization management system built with .NET 8 and Vue 3. It adopts Domain-Driven Design (DDD) architecture and integrates OpenIddict for OAuth 2.0/OpenID Connect standard authentication flows.

### ✨ Key Features

- 🔐 **OAuth 2.0 / OIDC** - Complete OpenIddict integration with multiple authorization flows
- 👥 **User Management** - User CRUD, role assignment, status management
- 🛡️ **RBAC Permissions** - Role-based access control with fine-grained permissions
- 🔒 **Security Policies** - Password policies, login lockout, audit logging
- 📊 **Admin Dashboard** - Modern admin interface based on Arco Design Pro Vue
- 🚀 **High Performance** - Dapper ORM with optimized query builder

### 🚀 Quick Start

```bash
# Backend
cd Dawning.Gateway/src/Dawning.Identity.Api
dotnet run

# Frontend
cd dawning-admin
pnpm install && pnpm dev
```

### 📄 License

This project is licensed under the [MIT License](LICENSE).
