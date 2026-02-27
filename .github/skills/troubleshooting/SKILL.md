---
description: "Debug and troubleshoot issues in Dawning: NullReference, DI errors, database connection, auth failures, API errors. Trigger: 调试, debug, 排错, troubleshoot, 报错, error, 问题, issue, 异常, exception"
---

# Troubleshooting Skill

## 目标

帮助定位和解决 Dawning 项目中的代码问题。

## 触发条件

- **关键词**：调试, debug, 排错, troubleshoot, 报错, error, 问题, issue, 异常, exception, 修复, fix bug
- **文件模式**：`*.cs`, `*.vue`, `*.ts`
- **用户意图**：调试问题、修复错误、排查异常

## 编排

- **前置**：无
- **后续**：`build-project`（修复后验证构建）

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} troubleshooting — {触发原因}`

---

## 调试流程

1. **收集信息**：错误信息、堆栈、复现步骤、预期 vs 实际行为
2. **分析问题**：定位发生位置、追踪数据流、检查依赖关系
3. **提出假设**：列出可能原因
4. **验证假设**：检查代码、运行测试
5. **解决 + 回归测试**

## 常见问题速查

### 空引用异常

```csharp
// 检查：对象是否初始化？查询是否返回 null？配置是否加载？DI 是否正确？
var user = await unitOfWork.User.GetByIdAsync(id);
if (user == null)
    throw new NotFoundException($"User {id} not found");
```

### 依赖注入错误

```
Unable to resolve service for type 'IXxxService'
```

检查：
1. 服务是否注册？`services.AddScoped<IXxxService, XxxService>()`
2. 生命周期是否正确？Singleton/Scoped/Transient
3. 是否有循环依赖？

### 数据库连接失败

```
Cannot open database / Connection refused
```

检查：连接字符串、数据库服务状态、防火墙、用户权限

### 认证/授权失败 (401/403)

检查：Token 是否过期、格式是否正确、用户是否有权限、CORS 配置

### API 请求失败（前端）

检查：URL 路径、HTTP 方法、请求体格式、请求头（Authorization）

### 常用调试命令

```bash
# 后端
dotnet run --urls="http://localhost:5000" --environment Development
grep -r "AddScoped\|AddTransient\|AddSingleton" --include="*.cs"

# 前端
pnpm dev
# 浏览器 F12 → Network 面板检查请求
```

## 验收场景

- **输入**："启动报错 Unable to resolve service for type 'IUserService'"
- **预期**：agent 检查 DI 注册、Service 构造函数、生命周期配置
- **上次验证**：2026-02-27 ✅
