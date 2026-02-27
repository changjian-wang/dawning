---
description: "Code review checklist for Dawning project: code style, DI, mappers, API design, database, security, performance, error handling. Trigger: 审查, review, check code, 代码审查, code review, 检查代码"
---

# Code Review Skill

## 目标

对 Dawning 项目代码进行全面审查，检查是否符合项目规范和最佳实践。

## 触发条件

- **关键词**：审查, review, check code, 代码审查, code review, 检查代码, 质量检查
- **文件模式**：`*.cs`, `*.vue`, `*.ts`
- **用户意图**：审查代码质量、检查规范合规性

## 编排

- **前置**：无
- **后续**：`build-project`（修复后构建验证）

## Skill 使用日志

使用本 skill 后，在 `/memories/repo/skill-usage.md` 追加一行：`- {日期} code-review — {触发原因}`

---

## 审查维度（9 个）

### 1. 代码风格

- [ ] 使用 file-scoped namespaces
- [ ] 使用 primary constructors (C# 12)
- [ ] 私有字段使用 `_camelCase` 命名
- [ ] 公共成员使用 PascalCase
- [ ] 添加 XML 文档注释

### 2. 依赖注入一致性

- [ ] UnitOfWork 字段统一命名为 `_unitOfWork`
- [ ] 通过 `_unitOfWork.{Repository}` 访问 Repository
- [ ] 不要同时注入 Repository 和 UnitOfWork（冗余）
- [ ] 只读操作可单独注入 Repository

### 3. 对象映射规范

- [ ] 使用静态 Mapper 类，不注入 IMapper
- [ ] Mapper 类放在 `Mapping/{Module}/` 目录下
- [ ] 使用扩展方法：`entity.ToDto()`, `dto.ToEntity()`, `entity.ApplyUpdate(dto)`
- [ ] 硬编码字符串使用常量类

### 4. API 设计

- [ ] 使用 `ApiResult<T>` 统一返回格式
- [ ] 添加 `[Authorize]` 保护端点
- [ ] 添加 `[ProducesResponseType]` 声明响应类型
- [ ] 遵循 RESTful 命名规范
- [ ] 正确使用 HTTP 状态码

### 5. 数据库规范

- [ ] GUID 主键表有 `timestamp` 字段和索引
- [ ] 表名使用 snake_case
- [ ] 布尔字段使用 `is_` 前缀

### 6. 安全性

- [ ] 不硬编码敏感信息
- [ ] 输入参数有验证
- [ ] SQL 使用参数化查询
- [ ] 敏感操作有权限检查

### 7. 性能

- [ ] 使用 async/await
- [ ] 避免 N+1 查询
- [ ] 合理使用缓存
- [ ] 避免大量数据内存加载

### 8. 异常处理

- [ ] 不吞掉异常
- [ ] 使用业务异常类（`NotFoundException`, `BusinessException` 等）
- [ ] 合理的日志记录
- [ ] 返回有意义的错误信息

### 9. 单元测试规范

- [ ] 通过 `_unitOfWorkMock.Setup(x => x.{Repository})` 设置 Mock
- [ ] 不直接注入 Repository Mock 到 Service 构造函数
- [ ] 移除不再使用的 `IMapper` Mock
- [ ] 测试命名遵循 `方法名_场景_期望结果` 格式

## 审查输出格式

```markdown
## 审查结果

### ✅ 符合规范
- [符合项...]

### ⚠️ 建议改进
- **位置**: `文件:行号`
- **问题**: 描述
- **建议**: 改进方案
- **严重程度**: 低/中/高

### ❌ 必须修改
- **位置**: `文件:行号`
- **问题**: 描述
- **修改建议**: 具体代码

### 📊 总结
- 代码质量评分: X/10
- 主要问题: ...
- 改进优先级: ...
```

## 验收场景

- **输入**："审查一下 UserService 的代码"
- **预期**：agent 按 9 个维度逐项检查，输出结构化审查报告
- **上次验证**：2026-02-27 ✅
