---
description: |
  Use when: Planning or executing any multi-step change that spans database, API, frontend, or tests; deciding commit boundaries; preventing horizontal scaffolding where one layer is fully written before the next layer starts
  Don't use when:
    - Single-layer change (only a Vue page tweak, only a SQL index add) — use the specific skill directly
    - Mechanical rename / format / dependency version bump (skill 例外 §1 applies)
    - Pure docs change with no code impact (use markdown)
    - Diagnosing a runtime bug (use troubleshooting)
    - Reviewing existing code (use code-review)
  Inputs: A feature request, ADR-equivalent decision, or "build this end-to-end" ask spanning ≥ 2 layers
  Outputs: A slice plan listing 1..N vertical slices (each end-to-end + independently verifiable + independently commit-able), with HITL/AFK labels and ordered serial dependency
  Success criteria: Each slice produces a working user-observable path on its own; no slice leaves a half-wired layer; commit body labels slice with `slice: HITL` or `slice: AFK`
---

# Vertical Slice Skill

> 任何能拆为多步的改动必须按"端到端薄切片"推进；禁止"先把所有层写完再串起来"的横向批量改动。

## 规则

### 1. 切片定义

一个**垂直切片**（vertical slice / tracer bullet）必须同时满足：

- **端到端**：覆盖所有相关层（migration / Entity / Repository / Service / Controller / Vue 页面 / 测试），不止单层。
- **可独立验证**：完成后能跑通一条用户可观察路径——`curl` 或 admin 页面任一即可。
- **可独立提交**：单次 commit 不依赖未完成的兄弟切片，且能通过 build + pre-commit。

### 2. 切片大小

- 优先**多个薄切片**而非"一个大切片"。
- 单切片必须能在不引入半成品的前提下合并；不能合并的不是切片，是阶段性进度。

### 3. HITL / AFK 标注（commit body 必填）

- **`slice: HITL`** — 切片包含需要人决策的节点（架构边界、新增 ADR 形态、跨模块契约变化），落地前必须停在该节点请确认。
- **`slice: AFK`** — 切片可被 agent 独立完成到合并/落地。
- 默认偏好 AFK；HITL 仅在确有不可代决策时使用。

### 4. dawning 串行落地顺序（强约束）

新增"端点 + 表 + 前端页"类切片，**必须按以下顺序串行**完成且每步立即跑测试 / 编译：

```
create-database  →  create-api  →  create-tests  →  create-vue-page
```

- 不允许跳步（"先写 controller 再回填 migration"）。
- 不允许并行铺底（"同时铺好 entity / DTO / 页面再串起来"）。
- 单切片内每一步都必须 build 通过；下一步才可以开始。

### 5. 反横向切片（禁止）

- 先把所有 Entity / DTO / migration 写完，再补 Service，再补 Controller，再补测试。
- 先写"未来 N 个切片都要用的"通用抽象（基类 builder / 泛型 generic mapper / 自研 framework），再落实第一切片。
- 把"功能开关"留半成品（写完代码但默认关闭）以拖到下个切片。
- Vue 端 EditModal / Drawer / 路由 / i18n 文件**全部铺完再去看接口能不能调通**。

### 6. 切片与 SDK 边界

- 单切片不得跨越 SDK 包边界——一次切片不同时改 `Dawning.Core` + `Dawning.Caching` + `Dawning.Identity` 三个包。
- SDK 类切片必须先发包再被消费方升级；不允许"先把 gateway 端引用改了再回头发包"。

## 正例

- 新增 `user-tag` 端点切片：`0012_create_user_tags.sql` → `UserTagEntity` + `IUserTagRepository` → `UserTagService` + Controller → xUnit + curl 验证 → admin 页面 `apps/admin/src/views/user-tag/`。每一步独立提交或在单 commit 内按顺序完成。
- ADR 类大特性拆为两次提交：**docs commit**（ADR + skill 反链）+ **impl commit**（migration + service + controller + 测试）；docs commit 单独可合并。
- SDK 升级切片：先发布 `Dawning.Core 1.x+1` → gateway 升级 `<PackageReference>` → gateway 内消费新 API。

## 反例

- 在 admin 页面铺好 `user-tag/index.vue` + `EditModal.vue` + `useUserTag.ts` + i18n + 路由，**然后**回头发现后端 `IUserTagRepository.GetByIdAsync` 还没写——违反规则 5（横向铺开）。
- 写一个"通用 enrichment pipeline 基类"覆盖 user/role/tenant 三种 enrichment，然后实现其中第一种——为不存在的痛点付维护成本，违反规则 2。
- 跳过 create-tests，先把 Controller 写完跑 e2e，"反正测试稍后补"——违反规则 4 串行顺序，也违反 dawning 项目 SDK >80% / gateway >60% 覆盖率门槛。
- 单 commit 同时改 `Dawning.Core` 的 `BusinessException` 形态与 `Dawning.Identity` 的 JWT claim 形态——违反规则 6。

## 例外

- **机械重命名 / 批量重格式化 / 依赖版本号 bump**：本身不引入行为变化，不强制端到端切片。
- **build / ci / 配置初始化**类一次性脚手架（scaffold）：允许铺成一整片，但需在 commit body 中明确 `slice: scaffold (no-behavior)`。
- **xml-doc 补全 / 注释翻译 / `// TODO` 清理**：纯文档维度，例外于切片定义。
- **被 ADR-equivalent skill 显式拆分为多 commit 的工作**（如 docs commit + impl commit）：每段仍须各自满足切片定义，不可视为对本规则的豁免。

## 与其他 skill 的关系

- **触发本 skill 后必走顺序**：本 skill 决定"切片如何切" → 切片内每一步调用对应 skill：
  - 表层 → [create-database](../create-database/SKILL.md)
  - 端点层 → [create-api](../create-api/SKILL.md)
  - 测试层 → [create-tests](../create-tests/SKILL.md)
  - 前端层 → [create-vue-page](../create-vue-page/SKILL.md)
- **commit 时**：组合 [git-workflow](../git-workflow/SKILL.md) 拼 conventional commit message，body 末行附 `slice: HITL` 或 `slice: AFK`。
- **审查时**：[code-review](../code-review/SKILL.md) 两轴的 Standards 轴会校验本规则。

## 与 ADR / PRD 的关系

- 单个 ADR / PRD 的实现可由多个切片组成。
- 切片自身不引入新 ADR 决策位（否则升级为 `slice: HITL`，且必须先写 ADR / 设计文档）。
- 切片不得跨越 ADR 边界——一次切片不同时落地两条 ADR 锁定的形态。
