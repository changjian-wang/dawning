---
description: |
  Use when: Writing or updating CHANGELOG.md for Gateway, Admin, Identity, or SDK changes
  Don't use when: Making code changes, building, or deploying
  Inputs: List of changes to document
  Outputs: Updated CHANGELOG.md entry following Keep a Changelog format with proper scopes
  Success criteria: CHANGELOG.md has properly formatted and scoped entries
---

# Changelog Skill

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

