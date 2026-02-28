---
description: |
  Use when: Diagnosing NullReference errors, DI registration failures, database connection issues, auth failures, or API errors
  Don't use when: Performing code audits (use code-review), writing new features (use create-api)
  Inputs: Error message, stack trace, or problem description
  Outputs: Root cause diagnosis and resolution steps
  Success criteria: Problem identified and resolved with a clear explanation
---

# Troubleshooting Skill

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

