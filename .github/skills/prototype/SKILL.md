---
description: |
  Use when: A choice between 2-3 implementation candidates can only be settled by running real code (e.g. AutoMapper vs static mapper perf, Dapper vs raw ADO, Arco modal vs drawer UX, cache TTL tuning); a quick spike is needed before locking the decision in code
  Don't use when:
    - Choice can be settled by doc reading / vendor benchmark (do that first)
    - Candidates ≥ 4 — narrow first via tech-selection rubric / SDK doc review
    - Building the actual feature (use vertical-slice + create-api)
    - Permanent reference sample (use samples/ directory pattern, not prototype)
    - Bug investigation (use troubleshooting)
  Inputs: A pinned single question with 2-3 candidates and a cost-of-mistake estimate
  Outputs: A prototype that answers exactly that question, the answer recorded in ADR / commit body / NOTES.md, prototype code deleted or absorbed (no orphans in main branch)
  Success criteria: Question answered with measurable evidence; conclusion (including failure cases) captured in a durable location; prototype code path no longer exists by the next commit
---

# Prototype Skill

> 当某个决策"看文档定不下来、必须实测"时，写一次性 prototype 回答它。结论落进 ADR / commit / NOTES，prototype 代码必须删掉或吸收。

## 1. 何时启用 prototype

仅当满足全部条件时才动手：

- 选项已收敛到 **2-3 个候选**（> 3 先做选型收敛）。
- 候选之间存在**只能实测才能定夺**的差异（性能、内存、UX 直觉、第三方接口真实行为）。
- 实测成本（< 1 天）**显著低于**误判后的回退成本（migrate schema / 改 SDK 形态 / 改协议）。

不满足时先继续读 doc / vendor benchmark / 既有 ADR，不要急着写 prototype。

## 2. 一个 prototype = 一个问题

每个 prototype 必须有一句话写明它在回答**哪一个问题**：

- ✅ "在 1k 条 user 数据上，静态 Mapper 比 AutoMapper 反射 Profile 实测快几倍？"
- ✅ "Arco Modal 在 EditUser 场景比 Drawer 的对话感更强吗？"
- ❌ "试一下 user + role + permission 端到端怎么样。"（这是切片，不是 prototype）

## 3. 两条路径

| 类型 | 形态 | 例 |
|---|---|---|
| **业务 / 状态机 / 数据** | xUnit `[Fact]` / `[Theory]`、`dotnet run` 一次性 console、SQL 直跑脚本 | 比 Dapper vs 直接 `IDbConnection.Execute` 的事务回滚行为 |
| **UI / 直觉** | 同一路由下 2-3 个**截然不同**的变体，URL 参数或 dev toggle 切换 | Modal vs Drawer vs 独立路由 EditUser；不允许"先做一个再迭代"——会丢对比 |

## 4. 一次性纪律

prototype 代码必须：

- **从第一行就标明**：文件名 / 目录名 / 顶部注释三选一含 `prototype` 或 `prototype-<question>`（如 `MapperBenchPrototype.cs` / `user-edit-modal-prototype.vue`）。
- **靠近真实使用位置**：紧挨它将服务的模块或页面放（如 `apps/gateway/Tests/MapperBenchPrototype.cs` 而非根目录 `prototypes/`）。
- **一条命令可跑**：沿用 dawning 现有 task runner（`dotnet test --filter Category=Prototype` / `pnpm dev -- --route=/user/_prototype`）。
- **无持久化**：状态在内存；如必须落库，用 `prototype_` 前缀的临时表（如 `prototype_user_tags`）。
- **不写测试、不写错误处理、不写抽象**——这些是被 prototype **检查**的对象，不该被它依赖。
- **暴露状态**：每一步动作或每次变体切换后，打印 / 渲染相关状态（embedding 维度、耗时、命中数、Arco 组件层级）。

## 5. 完成后必须二选一

prototype 跑出结论后：

### 5a. 删除 + 记录结论

prototype 代码删除，**同时**把答案写进可持久位置：

- 影响产品边界 / SDK 形态 → 新建 / 更新 ADR-equivalent 设计文档 + commit body 引用。
- 仅影响一次实现选择 → commit message body 或紧挨被验证模块的 `NOTES.md`。

### 5b. 吸收

把验证通过的部分清理后并入主代码：

- 补错误处理、单元测试（参见 [create-tests](../create-tests/SKILL.md)）、XML 文档；
- 移除 `prototype-` 前缀、调试 `Console.WriteLine` / `console.log`；
- 重命名变量符合 [naming conventions](../../copilot-instructions.md#naming-conventions)。

**禁止**"留着以后看"——未删未吸收的 prototype 必须在下一次切片前处理。

## 6. 失败结果也是结论

prototype "失败"（候选 A 不如 B / 都不行）同样是答案，必须明确写出：

- 写入 ADR-equivalent 的"被否决方案与理由"段；
- 或写一条 `wontfix` 性质的备忘到 NOTES，让下一个人不会重做同样实验。

## 正例

- **比 AutoMapper vs 静态 Mapper**：1000 个 `UserEntity → UserDto`，xUnit `[Theory]` + `Stopwatch`，跑一次拿到差异（如 6x），结论锁进 [code-patterns SKILL](../code-patterns/SKILL.md) 的 Mapper 章节，prototype 测试删除。
- **Arco Modal vs Drawer 编辑形态**：`apps/admin/src/views/user/_prototype/` 下放两个变体，dev toggle 切换，截图 + 团队 5 分钟评审定 Modal，prototype 目录删除，决定写进 admin/docs。
- **Redis cache TTL 调优**：3 个候选 TTL（30s / 5min / 1h），1000 次模拟请求测命中率与 fan-out 抖动，最佳值写入 `CacheKeyConstants` 默认值（含来源注释 `// PROTOTYPE 2026-05-13: TTL=5min wins`），prototype 脚本删除。

## 反例

- 因为"还没想清楚到底要 Dapper 还是 EF Core"，先在 `Dawning.ORM.Dapper` 和一个新 `Dawning.ORM.EFCore` 里都把 repo 实现写完，再"以后选一个"——这是 [vertical-slice](../vertical-slice/SKILL.md) 反例 + 本 skill 反例双重违反，违反 §2 / §5。
- 把 prototype 留在 `apps/gateway/Playground/`，commit 进 main 说"以后参考"——违反 §5。
- 一个 prototype 同时回答"Mapper 选哪个 + Repo 用什么模式 + Controller 返回什么"——违反 §2，是切片不是 prototype。
- prototype 跑完发现候选 A 不如 B，删了 prototype 但没在任何持久位置记一笔 A 为什么淘汰——下次有人提"为什么没考虑 A"时无凭无据，违反 §6。

## 例外

- **教学 / 示意代码**（明确写"this is illustrative" 的 SDK README sample）：不受本规则约束。
- **第三方 NuGet 包 spike**（验证"这个包能跑通某调用"）：允许保留为 sample 项目，但必须放 `sdk/samples/` 而非 `apps/`。
- **基准对照组**：若 prototype 同时充当回归基准（`benchmarks/` 下的 BenchmarkDotNet 工程），允许长期保留，但必须改名去掉 `prototype-` 前缀并补 README。

## 与其他 skill 的关系

- 上游：[grill-before-scaffold](../grill-before-scaffold/SKILL.md) 把请求 grill 清楚后，若仍有"实测才能定"的候选 → 进入本 skill。
- 下游：prototype 结论落地后 → [vertical-slice](../vertical-slice/SKILL.md) 切片化正式实现。
- 与 [code-patterns](../code-patterns/SKILL.md)、[performance](../performance/SKILL.md) 互补：性能类问题常需 prototype 量化。
- prototype 删除时若涉及 commit history，遵守 [git-workflow](../git-workflow/SKILL.md)：`chore(prototype): remove user-edit-modal-prototype after deciding on Modal`。
