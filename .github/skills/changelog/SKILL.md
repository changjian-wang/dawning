---
description: "CHANGELOG management for Dawning project. Trigger: changelog, 变更日志, release notes, 文档生成"
---

# Changelog Skill

## 目标

管理 Dawning 项目的 CHANGELOG 和 release notes。

## 触发条件

- **关键词**：changelog, 变更日志, release notes, 发布说明
- **文件模式**：`CHANGELOG.md`
- **用户意图**：更新变更日志、编写发布说明

## 编排

- **前置**：`git-workflow`（提交后整理变更日志）
- **后续**：无

---

## CHANGELOG.md Format

遵循 [Keep a Changelog](https://keepachangelog.com/) 格式：

```markdown
## [Unreleased]

### Added
- New feature description

### Changed
- Changed behavior description

### Fixed
- Bug fix description

### Removed
- Removed feature description
```

## Scopes

按组件分区记录变更：
- **Gateway**: API 网关相关
- **Admin**: 前端管理面板
- **Identity**: OAuth/OpenID Connect
- **SDK**: NuGet 包变更

## 验收场景

- **输入**："更新一下变更日志"
- **预期**：agent 查看最近提交，按格式生成变更日志条目
- **上次验证**：2026-02-27
