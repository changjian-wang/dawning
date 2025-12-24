# 快速入门指南

本指南将帮助您快速开始使用 Dawning SDK。

## 前提条件

- .NET 8.0 SDK 或更高版本
- Visual Studio 2022 / VS Code / JetBrains Rider

## 安装 SDK

### 1. 配置 GitHub Packages 源

首先，您需要配置 NuGet 以访问 GitHub Packages：

```bash
dotnet nuget add source https://nuget.pkg.github.com/changjian-wang/index.json \
  --name github-dawning \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN
```

> **注意**: `YOUR_GITHUB_TOKEN` 需要具有 `read:packages` 权限。

### 2. 安装所需包

根据您的需求安装相应的包：

```bash
# 核心包（推荐）
dotnet add package Dawning.Core
dotnet add package Dawning.Extensions

# 身份认证
dotnet add package Dawning.Identity

# 日志
dotnet add package Dawning.Logging

# 数据访问
dotnet add package Dawning.ORM.Dapper

# 弹性策略
dotnet add package Dawning.Resilience
```

## 基础配置

### ASP.NET Core 项目配置

```csharp
using Dawning.Core;
using Dawning.Identity;
using Dawning.Logging;
using Dawning.Resilience;

var builder = WebApplication.CreateBuilder(args);

// 添加 Serilog 日志
builder.AddDawningSerilog();

// 添加 JWT 认证
builder.Services.AddDawningJwtAuthentication(builder.Configuration);

// 添加用户上下文
builder.Services.AddCurrentUser();

// 添加弹性 HTTP 客户端
builder.Services.AddResilientHttpClient("external-api", options =>
{
    options.BaseAddress = new Uri("https://api.example.com");
    options.RetryCount = 3;
    options.CircuitBreakerThreshold = 5;
    options.TimeoutSeconds = 30;
});

var app = builder.Build();

// 使用全局异常处理
app.UseGlobalExceptionHandler();

app.Run();
```

### 配置文件 (appsettings.json)

```json
{
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "your-app",
    "Audience": "your-app-users",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

## 下一步

- 查看 [示例代码](samples.md) 了解更多用法
- 阅读 API 参考获取完整 API 文档
