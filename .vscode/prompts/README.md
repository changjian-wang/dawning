# VS Code Copilot 配置使用指南

本项目配置了 GitHub Copilot 的项目级指令和可复用提示模板，帮助开发者更高效地使用 AI 辅助编码。

## 📁 配置文件结构

```
.github/
└── copilot-instructions.md    # 项目级全局指令（自动加载）

.vscode/
└── prompts/                   # 可复用提示模板
    ├── create-api.prompt.md
    ├── create-vue-page.prompt.md
    ├── create-database-table.prompt.md
    ├── create-sdk-feature.prompt.md
    ├── create-unit-tests.prompt.md
    ├── code-review.prompt.md
    ├── performance-analysis.prompt.md
    ├── debug-issue.prompt.md
    ├── git-commit.prompt.md
    └── create-docker-config.prompt.md
```

## 🚀 快速开始

### 前提条件

1. 安装 VS Code
2. 安装 [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) 扩展
3. 安装 [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) 扩展
4. 登录 GitHub 账号并激活 Copilot 订阅

### 验证配置生效

1. 打开 VS Code 命令面板 (`Cmd+Shift+P` / `Ctrl+Shift+P`)
2. 输入 `Copilot: Open Instructions`
3. 确认能看到 `.github/copilot-instructions.md` 已加载

## 📖 使用方式

### 方式一：自动应用项目指令

`.github/copilot-instructions.md` 会**自动加载**到所有 Copilot 对话中。

当你在项目中使用 Copilot Chat 时，它已经知道：
- 项目使用 .NET 8 + Vue 3 技术栈
- 代码风格规范（命名、格式等）
- 数据库设计必须包含 `timestamp` 字段
- API 返回格式必须使用 `ApiResult<T>`

**示例对话：**
```
你: 创建一个用户管理的 Controller
Copilot: (自动遵循项目规范生成代码)
```

### 方式二：使用 # 引用 Prompt 文件

在 Copilot Chat 中输入 `#` 并选择 prompt 文件：

```
你: #create-api 创建一个角色管理 API
```

**操作步骤：**
1. 打开 Copilot Chat 面板 (`Cmd+Shift+I` / `Ctrl+Shift+I`)
2. 输入 `#` 字符
3. 在弹出列表中选择需要的 prompt 文件
4. 输入你的具体需求

### 方式三：使用 @ 提及工作区

结合 `@workspace` 让 Copilot 理解更多上下文：

```
你: @workspace #create-vue-page 参考现有用户页面，创建角色管理页面
```

### 方式四：打开 Prompt 文件后发送

1. 在编辑器中打开某个 `.prompt.md` 文件
2. 直接在 Chat 中描述需求，Copilot 会参考当前打开的文件

## 📝 Prompt 模板说明

| 模板文件 | 用途 | 使用示例 |
|----------|------|----------|
| `create-api.prompt.md` | 创建后端 API | `#create-api 创建订单管理 CRUD 接口` |
| `create-vue-page.prompt.md` | 创建前端页面 | `#create-vue-page 创建日志查询页面` |
| `create-database-table.prompt.md` | 设计数据库表 | `#create-database-table 设计权限表` |
| `create-sdk-feature.prompt.md` | 开发 SDK 功能 | `#create-sdk-feature 添加 HTTP 重试扩展` |
| `create-unit-tests.prompt.md` | 生成单元测试 | `#create-unit-tests 为 UserService 写测试` |
| `code-review.prompt.md` | 代码审查 | `#code-review 审查这个 PR 的代码` |
| `performance-analysis.prompt.md` | 性能分析 | `#performance-analysis 分析这个查询性能` |
| `debug-issue.prompt.md` | 调试问题 | `#debug-issue 帮我排查这个空引用异常` |
| `git-commit.prompt.md` | Git 提交 | `#git-commit 生成这次修改的提交信息` |
| `create-docker-config.prompt.md` | Docker 配置 | `#create-docker-config 创建部署配置` |

## 💡 使用技巧

### 1. 组合使用多个上下文

```
你: @workspace #create-api #create-database-table 
    创建完整的文章管理功能，包括数据库设计和 API
```

### 2. 引用具体文件

```
你: #code-review 审查 #file:src/Services/UserService.cs
```

### 3. 使用 Agent 模式

在 Chat 输入框选择 "Agent" 模式，可以让 Copilot 直接执行操作：
- 创建/修改文件
- 运行终端命令
- 执行多步骤任务

```
你: (Agent 模式) #create-vue-page 创建系统设置页面并添加路由
```

### 4. 快捷键

| 操作 | macOS | Windows/Linux |
|------|-------|---------------|
| 打开 Copilot Chat | `Cmd+Shift+I` | `Ctrl+Shift+I` |
| 内联建议 | `Tab` 接受 | `Tab` 接受 |
| 打开命令面板 | `Cmd+Shift+P` | `Ctrl+Shift+P` |

## 🔧 自定义 Prompt

### 创建新的 Prompt 文件

1. 在 `.vscode/prompts/` 目录创建新文件，如 `my-task.prompt.md`
2. 使用以下模板：

```markdown
---
mode: agent
description: 简短描述（会显示在选择列表中）
tools: ["read_file", "create_file", "run_in_terminal"]
---

# 任务标题

详细的任务说明和步骤...

## 示例

代码示例...
```

### Frontmatter 字段说明

| 字段 | 说明 | 值 |
|------|------|-----|
| `mode` | 运行模式 | `agent`（可执行操作）/ `chat`（仅对话） |
| `description` | 触发描述 | 简短说明，用于匹配用户意图 |
| `tools` | 可用工具 | 指定 agent 可以使用的工具 |

## 📚 相关文档

- [项目开发规范](../../docs/DEVELOPMENT_STANDARDS.md)
- [开发者指南](../../docs/DEVELOPER_GUIDE.md)
- [GitHub Copilot 官方文档](https://docs.github.com/en/copilot)

## ❓ 常见问题

### Q: Prompt 文件没有出现在 # 列表中？
A: 确保文件位于 `.vscode/prompts/` 目录且扩展名为 `.prompt.md`

### Q: 项目指令没有生效？
A: 检查 `.github/copilot-instructions.md` 文件是否存在，并在 VS Code 设置中确认 `github.copilot.chat.codeGeneration.useInstructionFiles` 已启用

### Q: 如何查看当前加载的指令？
A: 使用命令面板执行 `Copilot: Open Instructions`

### Q: Prompt 太长导致上下文溢出？
A: 可以拆分成多个小的 prompt 文件，按需引用
