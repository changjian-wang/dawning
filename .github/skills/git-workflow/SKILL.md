---
description: |
  Use when: Making git commits with conventional format (scopes: admin, gateway, identity, sdk), running pre-commit checks for backend and frontend
  Don't use when: Writing code (use code-patterns or create-api); building (use build-project); creating tests (use create-tests); writing changelog (use changelog); deploying (use deployment)
  Inputs: Changes to commit
  Outputs: Git commit with conventional message, pre-commit checks passed
  Success criteria: Commit follows `type(scope): subject` format, backend and frontend checks pass
---

# Git Workflow Skill

## Standard Flow

```bash
git status
git diff
git add -A
git commit -m "type(scope): summary"
git push
```

## Commit Format

```text
type(scope): description
```

### Types

`feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`

### Recommended Scopes

- **Backend**: `admin`, `gateway`, `identity`, `sdk`, `core`, `auth`, `user`
- **Frontend**: `admin-ui`, `router`, `store`, `i18n`, `api`
- **SDK Packages**: `caching`, `extensions`, `logging`, `messaging`, `orm`, `resilience`
- **Infra**: `deploy`, `docker`, `ci`, `docs`

### Examples

```bash
feat(admin): add user management page
fix(gateway): fix rate limit bypass issue
test(identity): add OAuth flow integration tests
chore(deploy): update Docker base image
docs(sdk): update integration guide
```

## Pre-commit Checks (Backend)

```bash
cd apps/gateway
dotnet build --nologo -v q
dotnet test --nologo
```

## Pre-commit Checks (Frontend)

```bash
cd apps/admin
pnpm lint
pnpm type-check
```

