---
description: |
  Use when: Writing or formatting Markdown docs, XML doc comments for C#, or bilingual documentation (en + zh-CN)
  Don't use when: Writing C# code (use code-patterns), generating changelogs (use changelog)
  Inputs: Documentation content to write or format
  Outputs: Well-formatted Markdown following 10 core rules, or XML documentation
  Success criteria: Documentation follows formatting rules, bilingual docs are consistent
---

# Markdown Formatting Skill

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

