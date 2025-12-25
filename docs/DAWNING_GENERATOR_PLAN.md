# Dawning Generator 项目计划

> 一个类似 ABP.io 的项目脚手架生成器，基于 Dawning SDK 快速创建微服务项目。

## 📋 项目概述

### 目标
- 提供 Web UI 让用户自定义项目配置
- 一键生成符合 Dawning 规范的 .NET 微服务项目
- 支持多种架构模式和可选模块
- 用户登录后可保存项目历史和模板收藏

### 仓库信息
- **仓库名**: `changjian-wang/dawning-generator`
- **本地路径**: `c:\github\dawning-generator` (请根据实际情况调整)

---

## 🛠️ 技术栈

### 后端
| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0 | 运行时 |
| ASP.NET Core | 8.0 | Web API |
| Scriban | 5.x | 模板引擎 |
| Dawning.Identity | 1.2.0 | JWT 认证 |
| Dawning.Core | 1.2.0 | 统一响应/异常 |
| Dawning.Logging | 1.2.0 | Serilog 日志 |
| Dapper | 2.1.x | 数据访问 |
| MySQL | 8.0 | 数据库 |

### 前端
| 技术 | 版本 | 用途 |
|------|------|------|
| Vue | 3.4 | 前端框架 |
| TypeScript | 5.x | 类型安全 |
| Arco Design Vue | 2.x | UI 组件库 |
| Pinia | 2.x | 状态管理 |
| Vue Router | 4.x | 路由 |
| Axios | 1.x | HTTP 客户端 |
| JSZip | 3.x | 前端 ZIP 生成 (备用) |

---

## 📁 目录结构

```
dawning-generator/
├── .github/
│   └── workflows/
│       ├── ci.yml                    # CI 流程
│       └── deploy.yml                # 部署流程
├── src/
│   ├── Dawning.Generator.Api/        # Web API 项目
│   │   ├── Controllers/
│   │   │   ├── GeneratorController.cs
│   │   │   ├── TemplateController.cs
│   │   │   ├── ProjectHistoryController.cs
│   │   │   └── HealthController.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Dawning.Generator.Api.csproj
│   ├── Dawning.Generator.Application/   # 应用层
│   │   ├── Dtos/
│   │   │   ├── GenerateProjectRequest.cs
│   │   │   ├── GenerateProjectResponse.cs
│   │   │   ├── TemplateOptionDto.cs
│   │   │   └── ProjectHistoryDto.cs
│   │   ├── Services/
│   │   │   ├── IProjectGeneratorService.cs
│   │   │   ├── ProjectGeneratorService.cs
│   │   │   ├── ITemplateService.cs
│   │   │   ├── TemplateService.cs
│   │   │   ├── IProjectHistoryService.cs
│   │   │   └── ProjectHistoryService.cs
│   │   ├── Templates/
│   │   │   └── TemplateEngine.cs     # Scriban 封装
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs
│   │   ├── DependencyInjection.cs
│   │   └── Dawning.Generator.Application.csproj
│   ├── Dawning.Generator.Domain/     # 领域层
│   │   ├── Entities/
│   │   │   ├── ProjectHistory.cs
│   │   │   ├── TemplateFavorite.cs
│   │   │   └── GenerationStats.cs
│   │   ├── Enums/
│   │   │   ├── DatabaseType.cs
│   │   │   ├── ArchitectureType.cs
│   │   │   └── DotNetVersion.cs
│   │   ├── Repositories/
│   │   │   ├── IProjectHistoryRepository.cs
│   │   │   └── ITemplateFavoriteRepository.cs
│   │   └── Dawning.Generator.Domain.csproj
│   └── Dawning.Generator.Infrastructure/  # 基础设施层
│       ├── Data/
│       │   ├── IDbConnectionFactory.cs
│       │   └── MySqlConnectionFactory.cs
│       ├── Repositories/
│       │   ├── ProjectHistoryRepository.cs
│       │   └── TemplateFavoriteRepository.cs
│       ├── DependencyInjection.cs
│       └── Dawning.Generator.Infrastructure.csproj
├── templates/                         # 项目模板源文件
│   ├── layered/                      # 分层架构模板
│   │   ├── {{ProjectName}}.sln.scriban
│   │   ├── src/
│   │   │   ├── {{ProjectName}}.Api/
│   │   │   │   ├── {{ProjectName}}.Api.csproj.scriban
│   │   │   │   ├── Program.cs.scriban
│   │   │   │   ├── appsettings.json.scriban
│   │   │   │   └── Controllers/
│   │   │   │       └── SampleController.cs.scriban
│   │   │   ├── {{ProjectName}}.Application/
│   │   │   ├── {{ProjectName}}.Domain/
│   │   │   └── {{ProjectName}}.Infrastructure/
│   │   ├── tests/
│   │   ├── Dockerfile.scriban
│   │   └── README.md.scriban
│   ├── simple/                       # 简单架构模板
│   │   ├── {{ProjectName}}.csproj.scriban
│   │   ├── Program.cs.scriban
│   │   └── ...
│   └── shared/                       # 共享模板片段
│       ├── _docker.scriban
│       ├── _github-actions.scriban
│       ├── _helm-chart.scriban
│       └── _gitignore.scriban
├── web/                               # Vue 前端
│   ├── public/
│   ├── src/
│   │   ├── api/
│   │   │   ├── generator.ts
│   │   │   ├── template.ts
│   │   │   └── auth.ts
│   │   ├── components/
│   │   │   ├── ProjectForm/
│   │   │   │   ├── BasicConfig.vue
│   │   │   │   ├── DatabaseConfig.vue
│   │   │   │   ├── ModuleSelector.vue
│   │   │   │   └── OptionalFeatures.vue
│   │   │   └── common/
│   │   ├── views/
│   │   │   ├── generator/
│   │   │   │   ├── index.vue         # 生成器主页
│   │   │   │   └── result.vue        # 生成结果页
│   │   │   ├── history/
│   │   │   │   └── index.vue         # 项目历史
│   │   │   ├── favorites/
│   │   │   │   └── index.vue         # 收藏模板
│   │   │   └── login/
│   │   │       └── index.vue         # 登录页
│   │   ├── store/
│   │   │   ├── modules/
│   │   │   │   ├── user.ts
│   │   │   │   └── generator.ts
│   │   │   └── index.ts
│   │   ├── router/
│   │   │   └── index.ts
│   │   ├── utils/
│   │   │   └── request.ts
│   │   ├── App.vue
│   │   └── main.ts
│   ├── index.html
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
├── scripts/
│   └── init.sql                      # 数据库初始化
├── tests/
│   ├── Dawning.Generator.Api.Tests/
│   └── Dawning.Generator.Application.Tests/
├── docker-compose.yml
├── docker-compose.dev.yml
├── Dockerfile
├── .dockerignore
├── .gitignore
├── README.md
├── LICENSE
└── Dawning.Generator.sln
```

---

## 🗄️ 数据库设计

### 表结构

```sql
-- 项目生成历史
CREATE TABLE project_histories (
    id CHAR(36) PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    project_name VARCHAR(100) NOT NULL,
    namespace_prefix VARCHAR(100),
    architecture_type VARCHAR(20) NOT NULL,      -- 'layered' | 'simple'
    dotnet_version VARCHAR(10) NOT NULL,         -- 'net8.0' | 'net9.0'
    database_type VARCHAR(20) NOT NULL,          -- 'mysql' | 'postgresql' | 'sqlserver' | 'sqlite'
    selected_modules JSON NOT NULL,              -- ["identity", "core", "logging", ...]
    optional_features JSON NOT NULL,             -- ["docker", "tests", "helm", "github-actions", ...]
    service_port INT NOT NULL,
    download_count INT DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_user_id (user_id),
    INDEX idx_created_at (created_at)
);

-- 模板收藏
CREATE TABLE template_favorites (
    id CHAR(36) PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    config JSON NOT NULL,                        -- 完整配置快照
    is_default BOOLEAN DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_user_id (user_id),
    UNIQUE KEY uk_user_name (user_id, name)
);

-- 生成统计 (管理员用)
CREATE TABLE generation_stats (
    id CHAR(36) PRIMARY KEY,
    date DATE NOT NULL,
    total_generations INT DEFAULT 0,
    unique_users INT DEFAULT 0,
    by_architecture JSON,                        -- {"layered": 100, "simple": 50}
    by_database JSON,                            -- {"mysql": 80, "postgresql": 40, ...}
    by_module JSON,                              -- {"identity": 150, "caching": 80, ...}
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_date (date)
);
```

---

## 🔌 API 设计

### 认证
复用 Dawning.Identity.Api，所有需要认证的接口带 `[Authorize]`

### 端点

```
POST   /api/generator/generate          # 生成项目并返回 ZIP
GET    /api/generator/options           # 获取可选配置项

GET    /api/history                     # 获取当前用户的生成历史
GET    /api/history/{id}                # 获取单个历史详情
DELETE /api/history/{id}                # 删除历史记录
POST   /api/history/{id}/regenerate     # 重新生成

GET    /api/favorites                   # 获取收藏列表
POST   /api/favorites                   # 创建收藏
PUT    /api/favorites/{id}              # 更新收藏
DELETE /api/favorites/{id}              # 删除收藏
POST   /api/favorites/{id}/apply        # 应用收藏配置生成项目

GET    /api/stats                       # [管理员] 获取统计数据
```

### 请求/响应示例

```json
// POST /api/generator/generate
// Request:
{
  "projectName": "EduSchedule",
  "namespacePrefix": "Dawning",           // 可选，生成 Dawning.EduSchedule
  "architectureType": "layered",          // layered | simple
  "dotnetVersion": "net8.0",
  "database": {
    "type": "mysql",
    "connectionStringTemplate": "Server=localhost;Database=edu_schedule_db;..."
  },
  "modules": [
    "identity",                           // 必选
    "core",
    "logging",
    "dapper",
    "caching",
    "resilience"
  ],
  "features": {
    "includeTests": true,
    "includeDocker": true,
    "includeHelmChart": false,
    "includeGitHubActions": true,
    "includeSwagger": true,
    "includeHealthChecks": true
  },
  "servicePort": 5400,
  "saveToHistory": true
}

// Response: (application/zip)
// 返回 ZIP 文件流，文件名: EduSchedule.zip
```

---

## 🖥️ 前端页面设计

### 1. 生成器主页 `/generator`

```
┌──────────────────────────────────────────────────────────────┐
│  🚀 Dawning 项目生成器                          [登录/用户头像] │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─ 步骤 1: 基础配置 ─────────────────────────────────────┐  │
│  │                                                        │  │
│  │  项目名称:  [EduSchedule          ]                    │  │
│  │  命名空间:  [Dawning              ] (可选前缀)          │  │
│  │  服务端口:  [5400                 ]                    │  │
│  │  .NET 版本: ○ .NET 8.0  ● .NET 9.0                     │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌─ 步骤 2: 架构模式 ─────────────────────────────────────┐  │
│  │                                                        │  │
│  │  ┌─────────────────┐  ┌─────────────────┐              │  │
│  │  │   📦 分层架构    │  │   📄 简单架构    │              │  │
│  │  │                 │  │                 │              │  │
│  │  │ Api/Application │  │  单项目结构      │              │  │
│  │  │ Domain/Infra    │  │  适合小型服务    │              │  │
│  │  │                 │  │                 │              │  │
│  │  │    [✓ 推荐]     │  │                 │              │  │
│  │  └─────────────────┘  └─────────────────┘              │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌─ 步骤 3: 数据库 ───────────────────────────────────────┐  │
│  │                                                        │  │
│  │  ● MySQL  ○ PostgreSQL  ○ SQL Server  ○ SQLite        │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌─ 步骤 4: SDK 模块 ─────────────────────────────────────┐  │
│  │                                                        │  │
│  │  ☑ Dawning.Identity  (必选) - JWT 认证/用户上下文       │  │
│  │  ☑ Dawning.Core      - 统一响应/业务异常               │  │
│  │  ☑ Dawning.Logging   - Serilog 结构化日志              │  │
│  │  ☑ Dawning.ORM.Dapper - Dapper CRUD 扩展              │  │
│  │  ☐ Dawning.Caching   - Redis 分布式缓存                │  │
│  │  ☐ Dawning.Messaging - 消息队列                        │  │
│  │  ☐ Dawning.Resilience - Polly 重试/熔断               │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌─ 步骤 5: 可选功能 ─────────────────────────────────────┐  │
│  │                                                        │  │
│  │  ☑ 单元测试 (xUnit)    ☑ Dockerfile                   │  │
│  │  ☑ Swagger 文档        ☑ 健康检查                      │  │
│  │  ☐ Helm Chart          ☐ GitHub Actions               │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│              [⭐ 保存为模板]     [🚀 生成项目]               │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### 2. 生成结果页 `/generator/result`

```
┌──────────────────────────────────────────────────────────────┐
│  ✅ 项目生成成功！                                           │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  项目名称: EduSchedule                                       │
│  架构类型: 分层架构                                          │
│  数据库:   MySQL                                             │
│                                                              │
│  包含文件:                                                   │
│  ├── EduSchedule.sln                                        │
│  ├── src/                                                   │
│  │   ├── EduSchedule.Api/                                   │
│  │   ├── EduSchedule.Application/                           │
│  │   ├── EduSchedule.Domain/                                │
│  │   └── EduSchedule.Infrastructure/                        │
│  ├── tests/                                                 │
│  ├── Dockerfile                                             │
│  └── README.md                                              │
│                                                              │
│  [📥 下载 ZIP]  [🔄 重新配置]  [📋 查看快速开始指南]          │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### 3. 历史记录页 `/history`

### 4. 收藏模板页 `/favorites`

---

## 📝 分阶段任务清单

### Phase 1: 项目初始化 (约 30 分钟)

```markdown
任务 1.1: 创建解决方案和项目结构
- [ ] 创建 Dawning.Generator.sln
- [ ] 创建 Dawning.Generator.Api 项目
- [ ] 创建 Dawning.Generator.Application 项目
- [ ] 创建 Dawning.Generator.Domain 项目
- [ ] 创建 Dawning.Generator.Infrastructure 项目
- [ ] 添加项目引用关系

任务 1.2: 配置 SDK 依赖
- [ ] 配置 NuGet 源 (GitHub Packages)
- [ ] 添加 Dawning.Identity 包引用
- [ ] 添加 Dawning.Core 包引用
- [ ] 添加 Dawning.Logging 包引用

任务 1.3: 基础配置
- [ ] 创建 appsettings.json
- [ ] 创建 appsettings.Development.json
- [ ] 配置 Program.cs (认证、日志、Swagger)
```

### Phase 2: 领域层和基础设施层 (约 45 分钟)

```markdown
任务 2.1: Domain 层
- [ ] 创建 ProjectHistory 实体
- [ ] 创建 TemplateFavorite 实体
- [ ] 创建 GenerationStats 实体
- [ ] 创建枚举类型 (DatabaseType, ArchitectureType, DotNetVersion)
- [ ] 创建仓储接口

任务 2.2: Infrastructure 层
- [ ] 创建数据库连接工厂
- [ ] 实现 ProjectHistoryRepository
- [ ] 实现 TemplateFavoriteRepository
- [ ] 创建 init.sql 数据库初始化脚本
```

### Phase 3: 模板引擎和生成器 (约 1.5 小时)

```markdown
任务 3.1: 模板引擎封装
- [ ] 安装 Scriban 包
- [ ] 创建 TemplateEngine 类
- [ ] 实现模板加载和渲染

任务 3.2: 创建模板文件 (分层架构)
- [ ] 创建 Solution 模板
- [ ] 创建 Api 项目模板
- [ ] 创建 Application 项目模板
- [ ] 创建 Domain 项目模板
- [ ] 创建 Infrastructure 项目模板
- [ ] 创建 Tests 项目模板
- [ ] 创建 Dockerfile 模板
- [ ] 创建 README 模板

任务 3.3: 创建模板文件 (简单架构)
- [ ] 创建单项目模板

任务 3.4: 项目生成服务
- [ ] 创建 IProjectGeneratorService 接口
- [ ] 实现 ProjectGeneratorService
- [ ] 实现 ZIP 打包逻辑
```

### Phase 4: API 控制器 (约 45 分钟)

```markdown
任务 4.1: 控制器实现
- [ ] 创建 GeneratorController (生成项目)
- [ ] 创建 TemplateController (获取选项)
- [ ] 创建 ProjectHistoryController (历史管理)
- [ ] 创建 FavoriteController (收藏管理)
- [ ] 创建 HealthController (健康检查)

任务 4.2: DTO 定义
- [ ] 创建请求/响应 DTO
- [ ] 配置 AutoMapper 映射
```

### Phase 5: Vue 前端 (约 2 小时)

```markdown
任务 5.1: 项目初始化
- [ ] 使用 Vite 创建 Vue 3 + TypeScript 项目
- [ ] 安装 Arco Design Vue
- [ ] 安装 Pinia, Vue Router, Axios
- [ ] 配置项目结构

任务 5.2: 基础配置
- [ ] 配置 Axios 拦截器 (Token 处理)
- [ ] 配置路由
- [ ] 配置 Pinia Store

任务 5.3: 页面开发
- [ ] 开发登录页
- [ ] 开发生成器主页 (表单组件)
- [ ] 开发结果页
- [ ] 开发历史记录页
- [ ] 开发收藏模板页

任务 5.4: API 集成
- [ ] 实现 generator API
- [ ] 实现 history API
- [ ] 实现 favorites API
```

### Phase 6: Docker 和部署 (约 30 分钟)

```markdown
任务 6.1: Docker 配置
- [ ] 创建后端 Dockerfile
- [ ] 创建前端 Dockerfile
- [ ] 创建 docker-compose.yml
- [ ] 创建 docker-compose.dev.yml

任务 6.2: CI/CD
- [ ] 创建 .github/workflows/ci.yml
- [ ] 创建 .github/workflows/deploy.yml
```

### Phase 7: 测试和文档 (约 30 分钟)

```markdown
任务 7.1: 单元测试
- [ ] ProjectGeneratorService 测试
- [ ] TemplateEngine 测试

任务 7.2: 文档
- [ ] 更新 README.md
- [ ] 创建 API 文档
```

---

## 🔗 与 Dawning 主项目的关系

```
dawning (主仓库)
├── sdk/                    # Dawning SDK (发布到 NuGet)
│   ├── Dawning.Identity
│   ├── Dawning.Core
│   └── ...
├── services/
│   └── Dawning.Gateway/    # 认证网关服务
└── dawning-admin/          # 管理前端

dawning-generator (本仓库)
├── 通过 NuGet 引用 Dawning SDK
├── 通过 HTTP 调用 Dawning.Identity.Api 进行认证
└── 生成的项目也引用 Dawning SDK
```

---

## 🚀 新 Chat 使用说明

在新的 Chat 中，可以这样开始：

```
我要创建 dawning-generator 项目，这是一个类似 ABP.io 的项目脚手架生成器。

本地路径: c:\github\dawning-generator
仓库: changjian-wang/dawning-generator

请参考计划文档: c:\github\dawning\docs\DAWNING_GENERATOR_PLAN.md

从 Phase 1 开始执行。
```

---

## ⚙️ 配置参考

### appsettings.json 示例

```json
{
  "DawningAuth": {
    "Authority": "http://localhost:5202",
    "Issuer": "http://localhost:5202",
    "RequireHttpsMetadata": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=dawning_generator;Uid=root;Pwd=password;"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  },
  "Templates": {
    "BasePath": "./templates"
  }
}
```

---

*文档创建于: 2025-12-25*
*适用于: Dawning Generator v1.0*
