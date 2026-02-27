---
description: "Markdown formatting rules for Dawning project documentation. Trigger: markdown, 写文档, README, API docs, documentation, 文档格式"
---

# Markdown Formatting Skill

## 目标

定义项目中所有 Markdown 文档和 XML 注释的格式化规范。

## 触发条件

- **关键词**：markdown, 写文档, README, API docs, documentation, 文档格式, 注释规范
- **文件模式**：`*.md`, `docs/**`
- **用户意图**：编写文档、格式化 Markdown、编写 XML 注释

## 编排

- **前置**：无
- **后续**：无

---

## Core Rules

1. **Headings** — blank lines before and after
2. **Code Blocks** — always specify language (```csharp, ```bash, ```json, ```vue, ```typescript)
3. **Lists** — consistent markers (- or 1.), blank line for nested content
4. **Tables** — aligned columns
5. **Links** — descriptive text, not "click here"
6. **Emphasis** — use sparingly, `code` for inline code references
7. **Line Length** — wrap at ~100 characters
8. **Inline Code** — backticks for class/method/variable names
9. **Blockquotes** — for notes and warnings
10. **GitHub Alerts** — `> [!NOTE]`, `> [!WARNING]`

## XML Documentation (C#)

```csharp
/// <summary>
/// Gets user by ID.
/// </summary>
/// <param name="id">The user's unique identifier.</param>
/// <returns>User DTO or null if not found.</returns>
/// <exception cref="ArgumentException">Thrown when id is empty.</exception>
public async Task<UserDto?> GetByIdAsync(Guid id);
```

## Bilingual Documentation

Dawning 项目使用双语文档模式：
- `README.md` — English
- `README.zh-CN.md` — 中文
- `docs/GUIDE.md` + `docs/GUIDE.zh-CN.md`

修改一种语言版本后，另一种语言版本也需要同步更新。

## 验收场景

- **输入**："帮我写一下这个功能的文档"
- **预期**：agent 按照格式规范输出 Markdown，代码块指定语言，提醒双语同步
- **上次验证**：2026-02-27
