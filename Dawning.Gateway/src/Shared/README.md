# Dawning Shared Libraries (SDK)

Dawning 项目的公共组件库，提供统一的认证、日志、异常处理、数据访问等功能。

## 📦 可用包

| 包名 | 描述 | 版本 |
|------|------|------|
| `Dawning.Shared.Authentication` | JWT 认证集成、权限验证、用户上下文 | ![Version](https://img.shields.io/badge/version-1.0.0-blue) |
| `Dawning.Shared.Core` | 业务异常、统一响应、分页、异常处理中间件 | ![Version](https://img.shields.io/badge/version-1.0.0-blue) |
| `Dawning.Shared.Logging` | Serilog 集成、结构化日志、请求上下文富化 | ![Version](https://img.shields.io/badge/version-1.0.0-blue) |
| `Dawning.Shared.Dapper.Contrib` | Dapper 扩展、CRUD 操作、数据库映射、Attributes | ![Version](https://img.shields.io/badge/version-1.0.0-blue) |
| `Dawning.Shared.Resilience` | 重试策略、熔断器、超时处理 | ![Version](https://img.shields.io/badge/version-1.0.0-blue) |
| `Dawning.Shared.Utils` | 通用扩展方法、帮助类 | ![Version](https://img.shields.io/badge/version-1.0.0-blue) |

## 🚀 快速开始

### 1. 配置 NuGet 源

在项目根目录创建 `nuget.config`：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/changjian-wang/index.json" />
  </packageSources>

  <packageSourceCredentials>
    <github>
      <add key="Username" value="你的GitHub用户名" />
      <add key="ClearTextPassword" value="你的Personal Access Token" />
    </github>
  </packageSourceCredentials>

  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="github">
      <package pattern="Dawning.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

### 2. 创建 Personal Access Token (PAT)

1. 访问 [GitHub Settings > Developer settings > Personal access tokens](https://github.com/settings/tokens)
2. 点击 "Generate new token (classic)"
3. 选择权限：`read:packages`
4. 生成并保存 token

### 3. 安装包

```bash
# 认证库
dotnet add package Dawning.Shared.Authentication --source github

# 核心库
dotnet add package Dawning.Shared.Core --source github

# 日志库
dotnet add package Dawning.Shared.Logging --source github

# Dapper 扩展
dotnet add package Dawning.Shared.Dapper.Contrib --source github
```

### 4. 使用示例

```csharp
using Dawning.Shared.Authentication.Extensions;
using Dawning.Shared.Core.Extensions;
using Dawning.Shared.Logging.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 日志
builder.Host.UseDawningLogging(options =>
{
    options.ApplicationName = "MyService";
});

// 认证
builder.Services.AddDawningAuthentication(builder.Configuration);

var app = builder.Build();

// 异常处理
app.UseDawningExceptionHandling();

// 日志富化
app.UseDawningLoggingEnrichment();

app.Run();
```

## 📋 发布新版本

### 自动发布（推荐）

推送 tag 触发自动发布：

```bash
git tag sdk-v1.0.0
git push origin sdk-v1.0.0
```

### 手动发布

在 GitHub Actions 页面手动触发 "Publish NuGet Packages" workflow。

## 📖 文档

- [认证集成指南](docs/AUTHENTICATION_INTEGRATION.md)
- [共享组件指南](docs/SHARED_COMPONENTS_GUIDE.md)

## 📁 项目结构

```
Dawning.Gateway/src/Shared/
├── Dawning.Shared.Authentication/    # 认证库
├── Dawning.Shared.Core/              # 核心库
├── Dawning.Shared.Logging/           # 日志库
├── Dawning.Shared.Dapper.Contrib/    # Dapper 扩展
├── Dawning.Shared.Dapper.Contrib.Attributes/
├── Dawning.Shared.Resilience/        # 弹性库
├── Dawning.Shared.Utils/             # 工具库
├── Directory.Build.props             # 统一版本管理
└── docs/                             # 文档
```

## 🔧 开发

### 本地构建

```bash
cd Dawning.Gateway/src/Shared
dotnet build
```

### 本地打包测试

```bash
dotnet pack -c Release -o ./nupkgs
```

## 📝 License

MIT License
