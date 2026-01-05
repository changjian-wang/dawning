---
description: 调试问题并找出根本原因
---

# 问题调试

帮助定位和解决代码中的问题。

## 调试流程

### 1. 收集信息
- 错误信息 / 异常堆栈
- 复现步骤
- 期望行为 vs 实际行为
- 相关日志

### 2. 分析问题
- 识别问题发生的位置
- 追踪数据流
- 检查依赖关系

### 3. 提出假设
- 基于收集的信息推测原因
- 列出可能的问题点

### 4. 验证假设
- 检查相关代码
- 运行测试
- 添加调试日志

### 5. 解决问题
- 修复根本原因
- 验证修复有效
- 添加测试防止回归

## 常见问题类型

### 空引用异常
```csharp
// 检查点
// 1. 对象是否正确初始化？
// 2. 数据库查询是否返回了 null？
// 3. 配置是否正确加载？
// 4. 依赖注入是否正确配置？

// ✅ 防御性编程
var user = await _repo.GetByIdAsync(id);
if (user == null)
{
    throw new NotFoundException($"User {id} not found");
}
```

### 依赖注入问题
```csharp
// 常见错误: Unable to resolve service for type 'IXxxService'
// 检查点:
// 1. 服务是否注册？ services.AddScoped<IXxxService, XxxService>();
// 2. 生命周期是否正确？ Singleton/Scoped/Transient
// 3. 是否有循环依赖？

// 调试: 检查 DI 配置
grep -r "AddScoped\|AddTransient\|AddSingleton" --include="*.cs"
```

### 数据库连接问题
```csharp
// 常见错误: Cannot open database / Connection refused
// 检查点:
// 1. 连接字符串是否正确？
// 2. 数据库服务是否运行？
// 3. 防火墙是否允许连接？
// 4. 用户权限是否足够？

// 调试: 测试连接
dotnet run --urls="http://localhost:5000" --environment Development
```

### 认证/授权问题
```csharp
// 常见错误: 401 Unauthorized / 403 Forbidden
// 检查点:
// 1. Token 是否过期？
// 2. Token 格式是否正确？
// 3. 用户是否有所需权限？
// 4. CORS 配置是否正确？

// 调试: 检查 JWT 内容
// https://jwt.io 解析 token
```

### API 请求失败
```typescript
// 检查点:
// 1. URL 是否正确？
// 2. 请求方法是否正确？ GET/POST/PUT/DELETE
// 3. 请求体格式是否正确？
// 4. 请求头是否包含必要信息？

// 调试: 检查网络请求
console.log('Request URL:', url);
console.log('Request Body:', JSON.stringify(data, null, 2));
```

## 日志分析

### 查找错误日志
```bash
# 搜索最近的错误
grep -r "ERROR\|Exception\|Failed" logs/

# 按时间过滤
grep "2024-01-15" app.log | grep "ERROR"

# 追踪请求
grep "trace-id-123" app.log
```

### 添加调试日志
```csharp
_logger.LogDebug("Starting operation: {OperationName} with params: {@Params}", 
    operationName, parameters);

try
{
    var result = await DoWorkAsync();
    _logger.LogDebug("Operation completed: {Result}", result);
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed: {OperationName}", operationName);
    throw;
}
```

## 调试输出格式

```markdown
## 问题诊断报告

### 问题描述
[简述问题现象]

### 根本原因
[分析后确定的原因]

### 相关代码
[问题代码位置和内容]

### 解决方案
[修复方案和代码]

### 验证步骤
1. [验证步骤1]
2. [验证步骤2]

### 预防措施
[如何防止类似问题再次发生]
```

## 常用调试命令

```bash
# .NET 项目
dotnet build              # 检查编译错误
dotnet test              # 运行测试
dotnet run               # 启动应用

# 查看日志
tail -f logs/app.log     # 实时查看日志

# 数据库
mysql -u root -p         # 连接数据库
SHOW PROCESSLIST;        # 查看当前连接

# 网络
curl -v http://localhost:5000/api/health  # 测试 API
```
