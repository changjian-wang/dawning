---
mode: agent
description: 代码审查 - 检查代码质量和规范
tools: ["read_file", "grep_search", "semantic_search"]
---

# 代码审查

对代码进行全面审查，检查是否符合项目规范和最佳实践。

## 审查维度

### 1. 代码风格
- [ ] 使用 file-scoped namespaces
- [ ] 使用 primary constructors (C# 12)
- [ ] 私有字段使用 `_camelCase` 命名
- [ ] 公共成员使用 PascalCase
- [ ] 添加 XML 文档注释

### 2. API 设计
- [ ] 使用 `ApiResult<T>` 统一返回格式
- [ ] 添加 `[Authorize]` 保护端点
- [ ] 添加 `[ProducesResponseType]` 声明响应类型
- [ ] 遵循 RESTful 命名规范
- [ ] 正确使用 HTTP 状态码

### 3. 数据库规范
- [ ] GUID 主键表有 `timestamp` 字段
- [ ] `timestamp` 字段有索引
- [ ] 表名使用 snake_case
- [ ] 布尔字段使用 `is_` 前缀

### 4. 安全性
- [ ] 不硬编码敏感信息
- [ ] 输入参数有验证
- [ ] SQL 使用参数化查询
- [ ] 敏感操作有权限检查

### 5. 性能
- [ ] 使用 async/await
- [ ] 避免 N+1 查询
- [ ] 合理使用缓存
- [ ] 避免大量数据内存加载

### 6. 异常处理
- [ ] 不吞掉异常
- [ ] 使用业务异常类
- [ ] 合理的日志记录
- [ ] 返回有意义的错误信息

## 审查输出格式

```markdown
## 审查结果

### ✅ 符合规范
- [符合的项目...]

### ⚠️ 建议改进
- **位置**: `文件:行号`
- **问题**: 描述问题
- **建议**: 改进建议
- **严重程度**: 低/中/高

### ❌ 必须修改
- **位置**: `文件:行号`
- **问题**: 描述问题
- **修改建议**: 具体修改代码示例

### 📊 总结
- 代码质量评分: X/10
- 主要问题: ...
- 改进优先级: ...
```

## C# 常见问题检查

### 命名问题
```csharp
// ❌ 错误
private ILogger logger;          // 应该用 _logger
public string username { get; }  // 应该用 Username

// ✅ 正确
private readonly ILogger _logger;
public string Username { get; }
```

### 缺少异步
```csharp
// ❌ 错误
public User GetById(Guid id) => _repo.GetById(id);

// ✅ 正确
public async Task<User> GetByIdAsync(Guid id) => await _repo.GetByIdAsync(id);
```

### 硬编码
```csharp
// ❌ 错误
var token = "sk-xxxxx";

// ✅ 正确
var token = _configuration["ApiKey"];
```

## Vue/TypeScript 常见问题检查

### 缺少类型
```typescript
// ❌ 错误
const data = ref([]);

// ✅ 正确
const data = ref<UserInfo[]>([]);
```

### 缺少错误处理
```typescript
// ❌ 错误
const fetchData = async () => {
  const result = await getList();
  data.value = result.items;
};

// ✅ 正确
const fetchData = async () => {
  loading.value = true;
  try {
    const result = await getList();
    data.value = result.items;
  } catch (error) {
    Message.error('加载失败');
  } finally {
    loading.value = false;
  }
};
```

### 缺少国际化
```typescript
// ❌ 错误
Message.success('删除成功');

// ✅ 正确
Message.success(t('common.deleteSuccess'));
```
