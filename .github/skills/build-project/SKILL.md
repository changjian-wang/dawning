---
description: "Build Dawning Gateway backend (.NET 8) and Admin frontend (Vue 3). Trigger: 构建, build, compile, 编译, dotnet build, pnpm build, 编译错误, build error"
---

# Build Project Skill

## 目标

构建 Dawning 项目的后端（.NET 8 Gateway）和前端（Vue 3 Admin）。

## 触发条件

- **关键词**：构建, 编译, build, compile, dotnet build, pnpm build, 编译错误, build error, restore
- **文件模式**：`*.csproj`, `*.sln`, `Directory.Build.props`, `package.json`, `tsconfig.json`
- **用户意图**：构建项目、修复编译错误、恢复依赖

## 编排

- **前置**：代码变更后
- **后续**：运行测试

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} build-project — {触发原因}`

---

## Backend (Gateway)

```bash
cd apps/gateway
dotnet build --nologo -v q               # Fast build
dotnet clean && dotnet build --nologo     # Clean rebuild
dotnet restore && dotnet build --nologo   # Restore + build
```

### Backend Projects

```
src/
├── Dawning.Gateway.Api               # API Gateway
├── Dawning.Gateway.Middleware         # Middleware
├── Dawning.Identity.Api              # Identity Server
├── Dawning.Identity.Application      # Application services
├── Dawning.Identity.Domain           # Domain entities
├── Dawning.Identity.Domain.Core      # Domain core
├── Dawning.Identity.Infra.CrossCutting.IoC
├── Dawning.Identity.Infra.CrossCutting.Mapping
├── Dawning.Identity.Infra.Data       # Dapper data access
└── Dawning.Identity.Infra.Messaging  # Message bus
```

## Frontend (Admin)

```bash
cd apps/admin
pnpm install          # Install dependencies
pnpm build            # Production build
pnpm dev              # Dev server
pnpm type-check       # TypeScript check
pnpm lint             # ESLint
```

## SDK

```bash
cd sdk
dotnet build --nologo -v q
```

## Common Build Issues

- **Backend locked files**: stop running services, rebuild
- **Missing NuGet packages**: `dotnet restore`
- **Frontend node_modules**: `pnpm install`
- **TypeScript errors**: `pnpm type-check` for details

## 验收场景

- **输入**："构建报错 CS0246 找不到类型"
- **预期**：agent 检查 using 语句、项目引用、NuGet 包版本
- **上次验证**：2026-02-27
