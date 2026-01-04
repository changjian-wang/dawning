---
description: Git 提交代码并遵循 Conventional Commits 规范
---

# Git 提交

遵循 Conventional Commits 规范提交代码。

## Commit Message 格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

## Type 类型

| Type | 说明 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(user): add user import feature` |
| `fix` | Bug 修复 | `fix(auth): fix token refresh failure` |
| `docs` | 文档更新 | `docs(sdk): update integration guide` |
| `style` | 代码格式 | `style: format code with prettier` |
| `refactor` | 重构 | `refactor(core): optimize caching strategy` |
| `perf` | 性能优化 | `perf(db): add query indexes` |
| `test` | 测试 | `test(user): add user service tests` |
| `chore` | 构建/工具 | `chore(deps): upgrade dependencies` |
| `ci` | CI/CD | `ci: add auto deployment workflow` |

## Scope 范围

- `admin` - 前端管理后台
- `gateway` - 网关服务
- `identity` - 身份认证服务
- `sdk` - SDK 包
- `core` - 核心模块
- `auth` - 认证授权
- `user` - 用户管理
- `deploy` - 部署配置
- `deps` - 依赖更新

## 提交流程

### 1. 查看变更
```bash
git status
git diff
```

### 2. 暂存文件
```bash
# 暂存所有变更
git add .

# 或暂存特定文件
git add src/path/to/file.ts
```

### 3. 提交
```bash
git commit -m "<type>(<scope>): <subject>"
```

### 4. 推送
```bash
git push origin <branch>
```

## 示例

### 单行提交
```bash
git commit -m "feat(admin): add user management page"
git commit -m "fix(gateway): fix rate limit bypass issue"
git commit -m "docs(sdk): update Dawning.Core README"
git commit -m "chore(deps): upgrade to .NET 8.0.2"
```

### 多行提交
```bash
git commit -m "feat(admin): add request logs monitoring page

- Add request log list with pagination
- Add statistics charts (response time, status codes)
- Add search and filter functionality
- Add i18n support (zh-CN, en-US)

Closes #123"
```

## 错误示例

```bash
# ❌ 缺少类型
git commit -m "add user page"

# ❌ 描述不具体
git commit -m "feat: fix"

# ❌ 大写开头
git commit -m "Feat(admin): Add user page"

# ❌ 中文（保持一致性，建议用英文或统一用中文）
git commit -m "feat(admin): 添加用户页面"  # 可以，但要团队统一
```

## Breaking Changes

```bash
git commit -m "feat(api)!: change user endpoint response format

BREAKING CHANGE: The user list endpoint now returns a paged response
instead of an array. Update your API clients accordingly.

Before:
GET /api/users → UserDto[]

After:
GET /api/users → { items: UserDto[], total: number, page: number }"
```

## 常用命令

```bash
# 修改最后一次提交信息
git commit --amend -m "new message"

# 查看提交历史
git log --oneline -10

# 取消暂存
git reset HEAD <file>

# 撤销工作区修改
git checkout -- <file>
```
