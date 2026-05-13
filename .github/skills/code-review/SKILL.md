---
description: |
  Use when: Reviewing a change (PR / branch diff / WIP) against two independent axes — Standards (does the code conform to dawning conventions?) and Spec (does the code faithfully implement the originating issue / PRD / commit message?)
  Don't use when:
    - Writing or fixing code (use code-patterns or create-api)
    - Building (use build-project)
    - Creating tests (use create-tests)
    - Analyzing performance (use performance)
    - Diagnosing errors (use troubleshooting)
  Inputs: A fixed point (commit SHA, branch, tag, `main`, `HEAD~5`) + the originating issue / PRD / spec path or `#id`
  Outputs: Two parallel reports `## Standards` and `## Spec`, presented side-by-side without merging or reranking
  Success criteria: Both axes have findings sections, each finding cites the source it violates (standard doc or spec line), tooling-enforced rules are not re-checked, one-line summary at the bottom
---

# Code Review Skill

> **Review 不是"找问题"，而是两个独立轴并行检视——禁止把它们合并、禁止互相重排，否则一个轴会把另一个轴遮蔽。**

## 为什么是两轴

一个改动可以一个轴通过、另一个轴失败：

- **代码完全符合 dawning 规范，但实现的功能是错的** → Standards 通过、Spec 失败。
- **代码精确实现了 Issue 要求，但违反了 dawning 约定** → Spec 通过、Standards 失败。

把两者混在同一个评分里（"代码质量 7/10"），就会出现："9 维度满分 → review pass → 实际功能完全不对" 的死角。两轴并列展示强制让评审人**分别看到两个独立信号**。

## 流程

### 1. 锚定 fixed point（必填）

用户没说就**先问，不要自己挑**：

> "Review against what — a branch, a commit, or `main`?"

拿到后固定 diff 命令：

```bash
git diff <fixed-point>...HEAD          # 三点写法，对比 merge-base
git log <fixed-point>..HEAD --oneline  # 列出涉及的 commit
```

### 2. 锁定 Spec 源（必填）

按以下优先级找原始 spec：

1. commit message 里的 `#123` / `Closes #45` / "实现 PRD X" 等引用 → 拉对应 Issue / PRD 全文。
2. 用户传入的明确 spec 路径。
3. `docs/` 下与分支名 / 功能匹配的 PRD / spec 文件。
4. 找不到 → 问用户："这个改动的原始需求在哪？" 若无 spec，本次 review **Spec 轴跳过**，并在最终报告中注明"无 spec，无法做 Spec 轴检视"。

### 3. 锁定 Standards 源

dawning 的规范分布在以下位置，**先把列表收集好再开始读**：

- `.github/copilot-instructions.md`（项目级 instructions）
- `.github/skills/code-patterns/SKILL.md`（静态 Mapper / UnitOfWork / 异常 / 常量）
- `.github/skills/create-api/SKILL.md`、`create-database/SKILL.md`、`create-vue-page/SKILL.md`、`create-tests/SKILL.md`（脚手架约定）
- `docs/DEVELOPMENT_STANDARDS.md`
- `.editorconfig`、`*.sln` analyzer 配置（机器强制的不要重查，引用一下即可）

### 4. 两轴并行评审

两轴**互不交叉**，分别完成下面两个 sub-task，最后并列输出。如果有 sub-agent 工具，**两个 sub-task 必须并行派发**（context 互不污染）；如果没有，在主上下文里**先做一个轴写完再做另一个轴**，禁止边做边混。

#### Standards 轴（dawning 9 维度清单）

逐文件 / 逐 hunk 检视：

##### 1. 代码风格

- [ ] 使用 file-scoped namespaces
- [ ] 使用 primary constructors (C# 12)
- [ ] 私有字段使用 `_camelCase` 命名
- [ ] 公共成员使用 PascalCase
- [ ] 添加 XML 文档注释

##### 2. 依赖注入一致性

- [ ] UnitOfWork 字段统一命名为 `_unitOfWork`
- [ ] 通过 `_unitOfWork.{Repository}` 访问 Repository
- [ ] 不同时注入 Repository 和 UnitOfWork（冗余）
- [ ] 只读操作可单独注入 Repository

##### 3. 对象映射规范

- [ ] 使用静态 Mapper 类，不注入 IMapper
- [ ] Mapper 类放在 `Mapping/{Module}/` 目录下
- [ ] 使用扩展方法：`entity.ToDto()`、`dto.ToEntity()`、`entity.ApplyUpdate(dto)`
- [ ] 硬编码字符串使用常量类

##### 4. API 设计

- [ ] 使用 `ApiResult<T>` 统一返回格式
- [ ] 添加 `[Authorize]` 保护端点
- [ ] 添加 `[ProducesResponseType]` 声明响应类型
- [ ] 遵循 RESTful 命名规范
- [ ] 正确使用 HTTP 状态码

##### 5. 数据库规范

- [ ] GUID 主键表有 `timestamp` 字段和索引
- [ ] 表名使用 snake_case
- [ ] 布尔字段使用 `is_` 前缀

##### 6. 安全性

- [ ] 不硬编码敏感信息
- [ ] 输入参数有验证
- [ ] SQL 使用参数化查询
- [ ] 敏感操作有权限检查

##### 7. 性能

- [ ] 使用 async/await
- [ ] 避免 N+1 查询
- [ ] 合理使用缓存
- [ ] 避免大量数据内存加载

##### 8. 异常处理

- [ ] 不吞掉异常
- [ ] 使用业务异常类（`NotFoundException` / `BusinessException` 等）
- [ ] 合理的日志记录
- [ ] 返回有意义的错误信息

##### 9. 单元测试规范

- [ ] 通过 `_unitOfWorkMock.Setup(x => x.{Repository})` 设置 Mock
- [ ] 不直接注入 Repository Mock 到 Service 构造函数
- [ ] 移除不再使用的 `IMapper` Mock
- [ ] 测试命名遵循 `方法名_场景_期望结果` 格式

**Standards 轴硬约束**：

- 每条 finding 必须**引用具体规范来源**（哪个 skill / instruction 哪一行），不写"凭感觉不对"。
- **跳过 analyzer / tooling 已经强制的规则**——`dotnet build` warning 0 已经覆盖的不再重列。
- 区分 **hard 违反**（必须改）和 **judgement call**（可商榷）。

#### Spec 轴

读完 Issue / PRD / commit body 后，针对 diff 检查三件事：

1. **缺失需求**：spec 要求了什么、diff 没做什么。每条引用 spec 原文一句。
2. **范围蔓延**（scope creep）：diff 做了什么、spec 没要求。每条引用 diff 行号。
3. **看似实现但实际错误**：方法 / 端点 / SQL 命名对得上 spec，但语义 / 边界 / 字段错。每条引用 spec + diff 双向。

**Spec 轴硬约束**：

- 每条 finding 必须**双向引用**：spec 哪一行 + diff 哪一行。
- **不评论代码风格**——风格归 Standards 轴。
- 找不到 spec 时输出"no spec available"，跳过本轴，不假装有 finding。

### 5. 输出格式（强制并列）

```markdown
## Standards

> 来源：copilot-instructions.md、code-patterns SKILL.md、DEVELOPMENT_STANDARDS.md

### 必须修改（hard）
- **位置**: `apps/gateway/Controllers/UserController.cs:42`
- **规则**: code-patterns §"静态 Mapper" — 禁止注入 `IMapper`
- **建议**: 改为 `user.ToDto()`
- **严重程度**: 高

### 建议改进（judgement）
- **位置**: `apps/gateway/Services/UserService.cs:88`
- **规则**: code-review §7 性能 — N+1 查询风险
- **建议**: 改为批量查询 `GetByUserIdsAsync`
- **严重程度**: 中

## Spec

> 来源：Issue #142、commit `abc1234` body

### 缺失需求
- **spec**: "管理员可以禁用用户" (#142 第 3 条)
- **diff**: 未实现 `PATCH /api/user/{id}/disable` 端点

### 范围蔓延
- **diff**: `apps/admin/src/views/user/index.vue:120-150` 引入"导出 CSV"按钮
- **spec**: Issue #142 未要求导出功能；如确有需求请开独立 Issue

### 实现错误
- **spec**: "新建用户默认 `is_active = true`" (#142 第 1 条)
- **diff**: `UserService.cs:35` 默认 `is_active = false`

## 总结

- Standards: 1 hard、1 judgement
- Spec: 1 缺失、1 蔓延、1 实现错误
- 最严重：Spec 实现错误 — `is_active` 默认值反了，建议先修这条
```

### 6. 一行总结（必填）

输出末尾一行：`Standards: X hard / Y judgement；Spec: A 缺失 / B 蔓延 / C 错误；最严重：<一句话>`。让看 review 的人 3 秒决定优先级。

---

## 关键不允许做的事

1. **不许把两轴合并打分**（如"代码质量 8/10"）——遮蔽 Spec 轴风险。
2. **不许在 Standards 轴评论功能正确性**——那是 Spec 轴的事。
3. **不许在 Spec 轴评论命名 / 风格**——那是 Standards 轴的事。
4. **不许跳过 fixed point 锚定**而靠"看上去最近的 commit"猜——结果是把已合并的旧改动也算进 review。
5. **不许跳过 spec 锁定**就开始 review——会变成"按 dawning 习惯重写代码"而不是"检视 PR 完成度"。

## 例外

- **机械重命名 / 配置 bump / lint fix 类 PR**：Spec 轴允许写 "no behavior change, spec axis N/A"，但 Standards 轴照常跑。
- **应急 hotfix**：允许两轴异步——hotfix merge 时只跑 Spec 轴（是否真的止血），Standards 轴 24h 内补做。

