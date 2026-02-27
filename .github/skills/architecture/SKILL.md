---
description: "Generate architecture diagrams and documentation for Dawning: Mermaid, system architecture, DDD layers, deployment diagrams. Trigger: 架构, architecture, 架构图, diagram, 系统设计, system design, 模块关系, 依赖关系"
---

# Architecture Skill

## 目标

生成 Dawning 项目的架构图、架构文档和模块依赖分析。

## 触发条件

- **关键词**：架构, architecture, 架构图, diagram, 系统设计, system design, 模块关系, 依赖关系, DDD, 分层
- **文件模式**：`*.sln`, `*.csproj`, `docs/**`
- **用户意图**：生成架构图、分析模块依赖、编写架构文档

## 编排

- **前置**：无
- **后续**：`markdown`（架构文档格式化）

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} architecture — {触发原因}`

---

## 项目架构

Dawning 采用 **DDD 分层架构**：

```
┌─────────────────────────────────────────────────┐
│              Presentation Layer                  │
│  dawning-admin (Vue 3)  │  Gateway.Api          │
│                         │  Identity.Api          │
├─────────────────────────────────────────────────┤
│              Application Layer                   │
│         Identity.Application                     │
│    (Services, DTOs, Commands, Queries)           │
├─────────────────────────────────────────────────┤
│                Domain Layer                      │
│  Identity.Domain  │  Identity.Domain.Core        │
│  (Entities)       │  (Base, Interfaces)          │
├─────────────────────────────────────────────────┤
│             Infrastructure Layer                 │
│  Infra.Data  │  Infra.Mapping  │  Infra.Messaging│
│  (Dapper)    │  (AutoMapper)   │  (RabbitMQ)     │
├─────────────────────────────────────────────────┤
│            Cross-Cutting (SDK Packages)          │
│  Core │ Extensions │ Identity │ Caching │ ...    │
└─────────────────────────────────────────────────┘
```

## Gateway 项目结构

```
apps/gateway/src/
├── Dawning.Gateway.Api/                    # API 网关 (YARP)
├── Dawning.Gateway.Middleware/             # 限流、日志、鉴权
├── Dawning.Identity.Api/                   # 身份认证 (OpenIddict)
├── Dawning.Identity.Application/           # 应用服务层
├── Dawning.Identity.Domain/                # 领域模型
├── Dawning.Identity.Domain.Core/           # 领域核心
├── Dawning.Identity.Infra.Data/            # Dapper + MySQL
├── Dawning.Identity.Infra.CrossCutting.IoC/
├── Dawning.Identity.Infra.CrossCutting.Mapping/
└── Dawning.Identity.Infra.Messaging/
```

## SDK 包结构

```
sdk/
├── Dawning.Core/        # 异常、中间件、统一结果
├── Dawning.Extensions/  # 字符串、集合、JSON
├── Dawning.Identity/    # JWT、用户上下文
├── Dawning.Caching/     # 内存缓存、Redis
├── Dawning.Logging/     # 结构化日志
├── Dawning.Messaging/   # RabbitMQ、Service Bus
├── Dawning.ORM.Dapper/  # Dapper 增强
└── Dawning.Resilience/  # 重试、熔断
```

## Mermaid 模板

### 系统架构图

```mermaid
graph TB
    Browser --> VueApp[Vue 3 Admin]
    VueApp --> YARP[API Gateway]
    YARP --> RateLimit --> Auth --> RequestLog
    RequestLog --> IdentityAPI[Identity API]
    IdentityAPI --> MySQL[(MySQL)]
    IdentityAPI --> Redis[(Redis)]
```

### 认证流程

```mermaid
sequenceDiagram
    Client->>Gateway: POST /connect/token
    Gateway->>Identity: 转发
    Identity->>MySQL: 验证凭证
    Identity->>Redis: 缓存 Token
    Identity-->>Client: Access Token
```

## 验收场景

- **输入**："画一下系统的架构图"
- **预期**：agent 生成 Mermaid 架构图，标注主要组件和数据流
- **上次验证**：2026-02-27 ✅
