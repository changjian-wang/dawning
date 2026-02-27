---
description: "Git operations for Dawning project with conventional commit messages. Trigger: git, commit, 提交, push, branch, 分支, merge, tag, pre-commit"
---

# Git Workflow Skill

## 目标

应用 Git 工作流和 Conventional Commits 规范。

## 触发条件

- **关键词**：git, commit, 提交, push, branch, 分支, merge, tag, 标签, pre-commit
- **文件模式**：`.git/**`, `.gitignore`
- **用户意图**：提交代码、推送分支、创建标签、查看历史

## 编排

- **前置**：格式化完成后提交
- **后续**：无

---

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

## 验收场景

- **输入**："提交这次修改"
- **预期**：agent 运行 pre-commit 检查，生成符合规范的 commit message，执行 `git add && git commit`
- **上次验证**：2026-02-27
