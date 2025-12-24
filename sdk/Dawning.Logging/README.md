# Dawning.Logging

Serilog 结构化日志库。

## 安装

```bash
dotnet add package Dawning.Logging
```

## 功能

- **Serilog 集成** - 预配置的日志设置
- **多输出** - 控制台、文件、Seq
- **日志丰富** - 请求上下文自动添加

## 使用

### 注册日志

```csharp
builder.AddDawningLogging(options =>
{
    options.ApplicationName = "MyService";
    options.EnableConsole = true;
    options.EnableFile = true;
    options.FilePath = "logs/app-.log";
    options.SeqServerUrl = "http://localhost:5341"; // 可选
});
```

### 使用日志丰富中间件

```csharp
app.UseDawningLogEnrichment();
```

### 配置选项

| 选项 | 说明 | 默认值 |
|------|------|--------|
| `ApplicationName` | 应用名称 | - |
| `EnableConsole` | 启用控制台输出 | true |
| `EnableFile` | 启用文件输出 | false |
| `FilePath` | 日志文件路径 | logs/app-.log |
| `SeqServerUrl` | Seq 服务器地址 | null |
