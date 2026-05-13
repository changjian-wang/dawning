---
description: |
  Use when: Receiving an ambiguous feature request, spec, or "build this for me" ask where requirements / constraints / acceptance criteria are not yet pinned down — pause and grill before scaffolding
  Don't use when:
    - Requirements are already crisp (issue body + DTO contract + UI mock all present) — go straight to vertical-slice / create-api
    - Mechanical work (rename / format / version bump)
    - Bug fix with a reproduction (use troubleshooting)
    - Code review of existing work (use code-review)
    - Performance investigation (use performance)
  Inputs: A feature request, vague ask, or under-specified PRD/issue
  Outputs: A short numbered list of pinned constraints + an explicit list of open questions sent back to the human, OR a green-light statement that the spec is now concrete enough to scaffold
  Success criteria: Before any DTO / migration / Vue page is written, every "this could mean two things" ambiguity has either been pinned by the user or recorded as an assumption with a fallback plan
---

# Grill Before Scaffold Skill

> 收到模糊的"帮我做 X"请求时，先反问清楚需求与约束，再动手写代码。一旦 DTO / migration / 页面落地，回滚成本就远高于多问几句的成本。

## 触发判据

只要满足下面**任一条**，就先 grill，不要先 scaffold：

- 请求里出现"差不多"、"类似"、"先做个简单的"、"按以前那样"
- 没有给出受影响的实体名 / 端点路径 / 字段集
- "API + 表 + 前端页"被同时要求，但只给了一句话
- 引用了不存在或未链接的"之前那个需求/issue"
- 同一句话能解出 ≥ 2 种合法实现

## 反问的 7 个维度（按出现频率排序）

按顺序逐条过；用户答了的跳过，没答的列在"待确认"。

### 1. 实体形态（必问）

- 主键是 `Guid` 还是业务自然键？timestamp 字段是否必须（dawning GUID PK 强约束 timestamp）？
- 软删除还是硬删除？审计字段（`created_at` / `updated_at` / `created_by`）是否需要？
- 与既有实体的关系：1:N / N:N / 弱关联？是否走 `event_associations` 类旁挂表？

### 2. 唯一性 / 幂等 / 并发

- 是否要求业务唯一键（除主键外）？冲突时返回 409 还是按 last-write-wins？
- 重复创建幂等吗（同 payload 多次调用是否同一结果）？
- 是否有并发更新风险？需要 optimistic concurrency token（`row_version` / `etag`）吗？

### 3. 鉴权 / 多租户

- 端点 `[Authorize]` 哪个角色？租户隔离字段是哪个？
- "我能看到自己的"vs"管理员能看到所有人的"如何区分？
- 是否暴露给外部 SDK / 第三方？走哪条 auth path？

### 4. 分页 / 排序 / 筛选

- 列表端点：默认 page size？最大 page size？默认 sort？
- 哪些字段可筛选 / 可排序？是否需要全文搜索（影响是否要建 GIN）？

### 5. 错误语义

- 业务校验失败 → `BusinessException`（HTTP 400）还是 `NotFoundException`（404）？
- 失败重试策略：客户端重试 OK 吗（幂等？）还是必须人工介入？
- 外部依赖（LLM / 邮件 / 队列）失败时 fail-soft 还是 fail-fast？

### 6. 前端形态

- 列表 + EditModal 还是列表 + Drawer 还是独立路由页？
- 是否需要批量操作？导入 / 导出？
- i18n：必须 zh-CN + en 双语吗？

### 7. 切片边界

- 这次切片到哪里收口？（参见 [vertical-slice](../vertical-slice/SKILL.md)）
- 哪些后续功能**明确不做**？（写下"out of scope"）

## 输出格式

回复用户时严格如下：

```
### 已确认 (从你的描述里读到)
1. ...
2. ...

### 待确认（实现前必须定）
1. <问题>?
   - 默认假设: <假设>
   - 影响: <影响哪个字段/端点/页>
2. ...

### 我的预设默认（若不反对，按这个跑）
- ...
- ...
```

**不要**：

- 在用户回答前就开始 scaffold；
- 把"待确认"塞进 TODO 注释然后写代码；
- 假装"默认假设"是用户说过的话。

## 例外

- 一行字就能反推全部约束的请求（"把这个字段加一个 nullable"），直接做。
- 用户明确说"按你的判断"，记录假设、按假设走、commit body 写明假设位置。
- 紧急 hotfix：先修后补 spec；但补 spec 不能省。

## 反例

- 接到"加一个标签管理页"就直接 [create-api](../create-api/SKILL.md) → [create-vue-page](../create-vue-page/SKILL.md) 一条龙铺完，结果交付时被指出"标签应该是 N:N 不是 1:N" / "应该带颜色" / "应该按租户隔离"——整改成本远高于一开始多问 5 分钟。
- 把"待确认"塞进代码 `// TODO: confirm whether tags are unique per tenant`，然后还是按自己理解写完——下次复盘就成了既成事实。

## 与其他 skill 的关系

- 通过 grill 后，进入 [vertical-slice](../vertical-slice/SKILL.md) 切片规划。
- 与 [code-review](../code-review/SKILL.md) Spec 轴互为镜像：Spec 轴问"做的是不是说要做的"，本 skill 问"说的到底是要做什么"。
- 与 [plan-first](https://github.com/changjian-wang/lumen/blob/main/docs/pages/rules/plan-first-implementation.md) 类规则可叠加。
