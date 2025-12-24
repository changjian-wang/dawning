---
_layout: landing
---

# Dawning SDK

欢迎使用 Dawning SDK 文档！Dawning SDK 是一套现代化的 .NET 8 类库，提供企业级应用开发所需的核心功能。

## 📦 包概览

| 包名 | 描述 |
|------|------|
| **Dawning.Core** | 核心类库：业务异常、API 响应、中间件 |
| **Dawning.Extensions** | 扩展方法：字符串、集合、日期、JSON、对象 |
| **Dawning.Identity** | 身份认证：JWT、用户上下文、Claims |
| **Dawning.Logging** | 日志组件：Serilog 集成、结构化日志 |
| **Dawning.ORM.Dapper** | ORM 扩展：Dapper CRUD 操作 |
| **Dawning.Resilience** | 弹性策略：重试、熔断、超时 (Polly) |

## 🚀 快速开始

### 安装

```bash
# 添加 GitHub Packages 源
dotnet nuget add source https://nuget.pkg.github.com/changjian-wang/index.json \
  --name github-dawning \
  --username YOUR_USERNAME \
  --password YOUR_GITHUB_TOKEN

# 安装包
dotnet add package Dawning.Core --version 1.1.0
dotnet add package Dawning.Extensions --version 1.1.0
```

### 基础使用

```csharp
using Dawning.Core;
using Dawning.Extensions;

// 使用 API 响应封装
var response = ApiResponse<User>.Success(user, "获取成功");

// 使用扩展方法
var email = "test@example.com";
if (email.IsValidEmail())
{
    var masked = email.Mask(); // "te**@example.com"
}

// 字符串转换
var className = "getUserInfo".ToPascalCase(); // "GetUserInfo"
```

## 📖 文档导航

- [快速入门指南](articles/getting-started.md) - 入门教程
- [示例代码](articles/samples.md) - 代码示例
- API 参考 - 完整的 API 文档（见顶部导航）

## 🔗 相关链接

- [GitHub 仓库](https://github.com/changjian-wang/dawning)
- [问题反馈](https://github.com/changjian-wang/dawning/issues)

## 📋 系统要求

- .NET 8.0 或更高版本
- Visual Studio 2022 / VS Code / Rider