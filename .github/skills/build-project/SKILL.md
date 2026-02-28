---
description: |
  Use when: Building Gateway backend (.NET 8), Admin frontend (Vue 3), or SDK packages; fixing compilation errors
  Don't use when:
    - Running tests (use create-tests)
    - Deploying containers (use deployment)
    - Writing code (use code-patterns)
    - Diagnosing runtime errors (use troubleshooting)
    - Creating API endpoints (use create-api)
  Inputs: Build command or compilation error
  Outputs: Successful build or resolved compilation error
  Success criteria: `dotnet build` and/or `pnpm build` succeed with 0 errors
---

# Build Project Skill

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

