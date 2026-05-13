---
description: |
  Use when: Diagnosing hard bugs or regressions — NullReference errors, DI registration failures, database connection issues, auth failures, API 4xx/5xx, intermittent failures, performance regressions
  Don't use when:
    - Performing code audits (use code-review)
    - Writing new features (use create-api)
    - Creating tests (use create-tests)
    - Building the project (use build-project)
    - Pure performance tuning without observed regression (use performance)
  Inputs: Error message, stack trace, failing scenario, or "this used to work" complaint
  Outputs: Reproduction harness + ranked hypotheses + targeted probes + minimal fix + regression test + clean-up summary
  Success criteria: Original repro no longer fires, regression test exists, all `[DEBUG-xxxx]` probes removed, root-cause hypothesis recorded in commit body
---

# Troubleshooting Skill

> 调试硬 bug 必须按"反馈回路 → 复现 → 假设 → 探针 → 修复+回归 → 清理"六段推进。**没有 < 30s 确定性反馈回路前，禁止开始猜原因。**

## 核心原则

1. **反馈回路是这条 skill 本身**——不是流程的第一步，是流程的前提；建不出来就停手。
2. **假设必须可证伪**——写得出"若 X 是因，改 Y 会让 bug 消失"的预测才算假设，否则是直觉，丢弃。
3. **探针留下唯一痕迹**——所有调试日志带 `[DEBUG-xxxx]` 前缀（4 位随机或事件 ID），完成时一次 grep 清理。
4. **修复不夹带重构**——"修复 + 回归"和"顺手起名调整"必须分两个 commit。

---

## 六段流程

### 1. 建立反馈回路（< 30s，确定性，agent 可独立运行）

按优先级尝试以下形态，**直到能拿到 pass/fail 信号**：

| 形态 | 何时用 | dawning 落地范例 |
|---|---|---|
| 失败的 xUnit 测试 | bug 在 service / repo / domain 层 | `dotnet test --filter FullyQualifiedName~UserServiceTests.GetById_BugScenario` |
| HTTP 脚本 / curl | bug 在 controller / 中间件 / 鉴权 | 写 `pwsh` 脚本一次发起 `Invoke-RestMethod`，断言状态码 + 响应体 |
| 前端 e2e（Playwright） | bug 在 admin UI 或交互流 | `pnpm playwright test --grep "bug-scenario"`，已有 `playwright.config.ts` |
| 真实流量重放 | bug 偶发 / 难构造输入 | 把一次真实请求保存到 `*.http` / `*.json`，离线重放 |
| 最小 console app | bug 在 SDK 包 / 跨进程协议 | 在 `sdk/samples/` 下建临时 console，写完即删 |
| `BenchmarkDotNet` | 性能回归 | 跑基线 vs 当前，看 P99 / 内存分配 |
| `git bisect run <script>` | bug 出现在两个已知状态间 | 把"启动 → 检查 → 退码"自动化为脚本 |

**硬约束**：

- **30 秒上限**：反馈回路全程跑完 > 30s 视为退化，必须先压缩。
- **确定性**：固定时间、固定 seed、固定 fixture；非确定 bug 先把复现率拉到 > 50% 再继续。
- **造不出回路就停手**：明确告诉用户"我需要 X 才能继续"，不要硬猜。

### 2. 复现

在反馈回路下复现 bug。必须确认：

- 复现的是**用户描述的同一故障**，不是附近的别的故障。
- 多次运行稳定，或已拉到可调试复现率。
- 已捕获**精确症状**（错误信息 / 错值 / 时延），供后续验证修复。

未复现不得进入下一段。

### 3. 排序假设（3-5 条，每条带可证伪预测）

```
假设 A（最可能）：UnitOfWork 的 DbContext 生命周期错位
  预测：若 A 是因，把注册从 Scoped 改 Transient 会让 bug 消失
假设 B：JWT 在 OpenIddict 刷新令牌时 claim 缺 sub
  预测：若 B 是因，把刷新流程换成新令牌会让 bug 消失
假设 C：MySQL utf8mb4 排序规则与 .NET StringComparison 不一致
  预测：若 C 是因，对同一查询用 BINARY 比较结果不同
```

- 写不出预测的不是假设——丢弃或细化。
- 把列表展示给用户一次（成本低收益大），用户不在场可径直按序探针。
- **不要并行尝试 3 条假设的修复**——会污染反馈回路因果。

### 4. 探针（最小变量，定向日志）

- 优先**调试器断点 / REPL**，其次**关键边界处的定向日志**。
- 严禁"全量 `_logger.LogInformation` 撒满代码再 grep"。
- 调试日志格式：

  ```csharp
  _logger.LogInformation("[DEBUG-7a3c] UnitOfWork.User instance hash={Hash}, txId={TxId}",
      _unitOfWork.User.GetHashCode(), _currentTxId);
  ```

  `7a3c` 是 4 位随机 / 事件 ID，结束时 `grep -r "\[DEBUG-7a3c\]"` 一次清掉。
- 性能问题不靠日志，用 `Stopwatch` / `BenchmarkDotNet` / SQL `EXPLAIN` 建基线，再二分定位。

### 5. 修复 + 回归测试

1. 先把最小复现写成**正确接缝**上的 xUnit 测试，**看到它失败**。
2. 再写修复，**看到它通过**。
3. 修复后用第 1 段的反馈回路重跑原始（未最小化）场景。

若找不到"正确接缝"（测试不到真实 bug 路径），记录为发现，不强写假阳测试给伪信心；通常意味着架构有耦合，留作 P3 改进输入。

### 6. 清理 + 复盘

声明完成前必须确认：

- [ ] 原始复现已不再复现（重跑第 1 段反馈回路）。
- [ ] 回归测试通过，或"接缝缺失"作为发现已记录。
- [ ] 全部 `[DEBUG-xxxx]` 探针已 grep 清理：`grep -rn "\[DEBUG-" --include="*.cs" --include="*.ts" --include="*.vue"` 应无输出。
- [ ] 临时 console / `.http` / playwright spike 已删除或移入明确的 `samples/` 位置。
- [ ] 命中的假设 + 关键证据写入 commit body：

  ```
  fix(gateway): correct UnitOfWork DbContext lifetime

  Root cause: Scoped registration shared DbContext across nested service
  scopes when a background job span-ed both. Hypothesis A confirmed by
  matching DbContext hash across [DEBUG-7a3c] logs.
  ```

最后问一句："**如果这个 bug 不该发生，应当改什么？**" 若答案涉及架构（缺接缝、调用纠缠、隐藏耦合），把结论留给下一次 code-review 或 ADR；**不在本次修复中夹带架构改动**。

---

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

检查：连接字符串、数据库服务状态、防火墙、用户权限。

### 认证/授权失败 (401/403)

检查：Token 是否过期、格式是否正确、用户是否有权限、CORS 配置。

### API 请求失败（前端）

检查：URL 路径、HTTP 方法、请求体格式、请求头（Authorization）。

### 常用调试命令

```bash
# 后端
dotnet run --urls="http://localhost:5000" --environment Development
dotnet test --filter FullyQualifiedName~SomeTest --logger "console;verbosity=detailed"
grep -r "AddScoped\|AddTransient\|AddSingleton" --include="*.cs"

# 前端
pnpm dev
pnpm playwright test --grep "scenario"
# 浏览器 F12 → Network 面板检查请求

# 探针清理（六段法第 6 段必跑）
grep -rn "\[DEBUG-" --include="*.cs" --include="*.ts" --include="*.vue"
```

---

## 例外

- **5 分钟以内可独立修复的明显问题**（拼写错误、明显空引用、配置缺一行）：允许跳过反馈回路与假设排序，但必须仍写回归测试或解释为何不需要。
- **生产事故应急止血**：允许先以最小变更止血，但事后 24h 内必须按六段法补完调查与回归测试。

