# NuGet 版本下架与隐藏策略

本文规定 Dawning SDK 系列 NuGet 包（`Dawning.Core`、`Dawning.Extensions`、
`Dawning.Identity`、`Dawning.Caching`、`Dawning.Logging`、`Dawning.Messaging`、
`Dawning.ORM.Dapper`、`Dawning.Resilience` 等）历史版本的处置方式。

## TL;DR

- nuget.org **永久不允许**真正删除已发布的版本。
- 唯一可控的动作是把版本**取消列出（unlist）**，使其从搜索 / IDE 的版本下拉中
  消失，但已经把它锁在 `<PackageReference>` 里的下游依然能 `restore` 成功。
- 因此发布策略必须先于"事后清理"——**一旦发布就视为永久存在**。

## 平台硬约束

| 行为 | nuget.org 是否允许 | 说明 |
| --- | --- | --- |
| 物理删除某个版本 | ❌ 不允许 | 只有违反 [Package Policies](https://docs.microsoft.com/nuget/policies/package-policies) 的包，由 Microsoft 客服在极端情况下才会下架 |
| Unlist（取消列出） | ✅ 允许 | 从搜索结果、官方 feed 列表、Visual Studio 的版本选择器里隐藏，但 `dotnet add package <id> --version <unlisted>` 仍可成功安装 |
| 重新 List | ✅ 允许 | unlist 是可逆动作 |
| 替换同版本号的内容 | ❌ 不允许 | 二进制不可变；想发新二进制必须升版本号 |

> CLI 子命令名虽然叫 `dotnet nuget delete`，但服务端实际只做 unlist，**不会**
> 真删。这是 NuGet 协议规定的行为，不是 bug。

## 何时应当 Unlist 一个版本

满足以下任一条件即可 unlist：

1. 该版本存在已确认的**正确性 bug**（运行时异常、数据损坏、协议不兼容等），且
   已有更高的修复版本可用。例：`Dawning.ORM.Dapper 1.3.0` 之前的所有版本都
   带有 `Get<T> / GetAsync<T>` 的 `RuntimeBinderException` bug，已在
   `1.3.1` 修复。
2. 该版本存在已披露的**安全漏洞（CVE）**，且已有修复版本。
3. 该版本是**预览 / RC**，且对应正式版已发布；为避免新用户误装预览版。
4. 该版本是**误发布**（版本号搞错、TFM 错、漏文件等），且当时已立刻发出新版
   覆盖。

不要 unlist 仅仅因为"老"或"丑"——稳定的旧版本是下游可重复构建的保障。

## 何时**不应**当 Unlist

- 当前 workspace 内任何项目仍把该版本写在 `<PackageReference>` 里。
- 已知有外部用户依赖该版本，且修复版本是 SemVer 上的 **breaking change**
  （major bump）——他们没有便捷的升级路径。
- 你只是想"清理列表外观"。如果包没有真正问题，留着即可。

## 操作方式

### 方法 1：网页 UI（推荐，最直观）

1. 登录 https://www.nuget.org/
2. 头像 → **Manage Packages** → 选择目标包
3. 点击具体版本号 → 取消勾选 **Listed** → **Save**
4. 对每个待隐藏的版本重复上述步骤

### 方法 2：CLI（批量更高效）

```powershell
# 单版本 unlist；--non-interactive 跳过确认
dotnet nuget delete <PackageId> <Version> `
    --source https://api.nuget.org/v3/index.json `
    --api-key <YOUR_API_KEY> `
    --non-interactive
```

API key 必须具备目标包的 **Unlist** 权限（NuGet.org → API Keys → 至少勾选
`Unlist package` 这一 scope）。

```powershell
# 批量 unlist 一组旧版本（举例）
$pkg = "Dawning.ORM.Dapper"
$key = $env:NUGET_API_KEY
"1.0.0", "1.1.0", "1.2.0" | ForEach-Object {
    dotnet nuget delete $pkg $_ `
        --source https://api.nuget.org/v3/index.json `
        --api-key $key `
        --non-interactive
}
```

### 方法 3：恢复一个被 unlist 的版本

只能用网页 UI（CLI 不支持 re-list）：
**Manage Packages → 选包 → 选版本 → 勾选 Listed → Save**。

## 版本治理决策表

发布完成后逐项核对：

| 状态 | 动作 |
| --- | --- |
| 当前推荐版（最新 stable） | **Listed**，加 README 中的 `Version="x.y.*"` 示例 |
| 上一个 stable 版本 | **Listed**，保留至少一个 minor 周期，方便下游灰度 |
| 含已修复 bug 的旧版 | **Unlisted**，CHANGELOG 中说明替代版本 |
| 含 CVE 的旧版 | **Unlisted**，并提交 [GitHub Security Advisory](https://github.com/advisories) |
| 预览版 / RC | 对应 stable 发布后 7 天内 **Unlisted** |
| 实验性 / 私下推送的版本 | 立即 **Unlisted** |

## 记录义务

每次 unlist 必须在以下两处留痕：

1. 该包的 `CHANGELOG.md`，新增条目：

   ```markdown
   ## [Yanked] 1.2.0 - 2026-05-07

   - Reason: Get<T>/GetAsync<T> 抛出 RuntimeBinderException
   - Replacement: 1.3.1
   - Notes: 旧版本仍可拉取，但已从搜索结果中隐藏
   ```

2. Git commit message：

   ```
   chore(sdk): unlist Dawning.ORM.Dapper 1.2.x on nuget.org

   Reason: Get<T>/GetAsync<T> dynamic CallSite throws RuntimeBinderException.
   Replacement: 1.3.1+. Old versions remain installable for locked downstream
   consumers but are hidden from search.
   ```

## 当前快照（2026-05-07）

| 包 | 版本 | 状态 | 备注 |
| --- | --- | --- | --- |
| `Dawning.ORM.Dapper` | `1.3.1` | **Listed** | 推荐版本；修复 Get/GetAsync 动态 CallSite bug |
| `Dawning.ORM.Dapper` | `1.3.0` | **Listed** | 暂不 unlist；`dawning-agent-os` 仍锁定此版本 |
| `Dawning.ORM.Dapper` | `1.2.x` 及更早 | **建议 Unlist** | 含 GetAsync bug，已无任何已知下游 |

> 若开始全面切到 `1.3.1`+，可以将 `1.3.0` 一并 unlist。当前保留是为了给
> `dawning-agent-os` 等下游留出灰度窗口。

## 参考

- [NuGet Package Policies](https://docs.microsoft.com/nuget/policies/package-policies)
- [Deleting packages on nuget.org](https://docs.microsoft.com/nuget/nuget-org/policies/deleting-packages)
- [`dotnet nuget delete`](https://docs.microsoft.com/dotnet/core/tools/dotnet-nuget-delete)
