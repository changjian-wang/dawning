using Dawning.Identity.Application.Interfaces;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// YARP configuration provider - loads route and cluster configuration from database
/// </summary>
public class DatabaseProxyConfigProvider : IProxyConfigProvider, IDisposable, IAsyncDisposable
{
    private readonly IGatewayConfigService _configService;
    private readonly ILogger<DatabaseProxyConfigProvider> _logger;
    private DatabaseProxyConfig _config;
    // 可空仅服务于 Dispose 语义：构造函数会立即赋非空值，运行期间始终非空。
    // null 只表示「已 Dispose 或已被 SwapConfig/Dispose 通过 Interlocked.Exchange 取走」，
    // 不代表存在未初始化窗口。
    private CancellationTokenSource? _changeTokenSource;
    private readonly object _lock = new();
    // 串行化整个「读取 routes/clusters → UpdateConfig」流程，保证同一时刻只有一次加载-应用，
    // 避免并发 ReloadConfigAsync 时后完成的旧数据库快照覆盖较新的快照（lost update / stale overwrite）。
    //
    // 不变量（禁止破坏）：本类全程不得访问 _loadGate.AvailableWaitHandle。
    // DisposeLoadGateAsync 在等待许可超时时会「放弃 Dispose、交由 GC 回收」该信号量；
    // 该路径仅在 SemaphoreSlim 未持有内核句柄时才不泄漏，而 AvailableWaitHandle 的首次访问
    // 会惰性创建一个需要显式 Dispose 才能释放的内核事件句柄。一旦任何代码（含派生/调试用途）
    // 访问了 AvailableWaitHandle，跳过 Dispose 即会泄漏该句柄。
    // 修改前请全局搜索 `AvailableWaitHandle` 确认本不变量仍成立。
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    // 实例级生命周期 token：Dispose 时取消，用于主动中断进行中的数据库调用，
    // 避免 Dispose 的 _loadGate.Wait() 被各自独立的 cancellationToken 长时间阻塞。
    private readonly CancellationTokenSource _lifetimeCts = new();
    private bool _disposed;


    public DatabaseProxyConfigProvider(
        IGatewayConfigService configService,
        ILogger<DatabaseProxyConfigProvider> logger
    )
    {
        _configService = configService;
        _logger = logger;
        _changeTokenSource = new CancellationTokenSource();
        _config = new DatabaseProxyConfig(
            Array.Empty<RouteConfig>(),
            Array.Empty<ClusterConfig>(),
            new CancellationChangeToken(_changeTokenSource.Token)
        );
    }

    public IProxyConfig GetConfig()
    {
        // 首次配置由 ProxyConfigInitializer 在宿主启动时预加载，
        // 这里仅在锁内返回当前快照，避免 sync-over-async 阻塞/死锁。
        lock (_lock)
        {
            // Dispose 后 _changeTokenSource 已被取消并释放。YARP 的 ConfigChange 监听链会在
            // Dispose 触发的 Cancel() 后同步回调 GetConfig() 重新读取配置：此时若抛
            // ObjectDisposedException，异常可能逃逸到 YARP 配置加载管线（行为随 YARP 版本而异），
            // 在正常关闭期间产生意外错误日志甚至使反向代理瞬时不可用。
            // 故改为返回一个空 routes/clusters、且 ChangeToken 已立即触发的良性快照，
            // 让 YARP 拿到「无路由、需立即重载」的状态而非异常。此处用一次性已取消的
            // CancellationTokenSource 构造 ChangeToken（构造后立即取消并释放），
            // 不写回 _changeTokenSource 字段，避免引入额外的 CTS 泄漏。
            if (_disposed)
            {
                using var disposedTokenSource = new CancellationTokenSource();
                disposedTokenSource.Cancel();
                return new DatabaseProxyConfig(
                    Array.Empty<RouteConfig>(),
                    Array.Empty<ClusterConfig>(),
                    new CancellationChangeToken(disposedTokenSource.Token)
                );
            }

            return _config;
        }
    }


    /// <summary>
    /// Load configuration from database
    /// </summary>
    public async Task LoadConfigAsync(CancellationToken cancellationToken = default, bool throwOnFailure = false)
    {
        // Dispose 之后（含后台触发的配置重载）直接返回，避免对已释放的 SemaphoreSlim
        // 调用 WaitAsync 抛出 ObjectDisposedException。
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }
        }

        // 串行化加载-应用，确保并发重载不会用较旧的数据库快照覆盖较新的快照。
        // 信号量仅覆盖「数据库读取 → 构造并写回快照」；变更通知（Cancel 触发同步回调）
        // 在释放信号量后再触发，避免持有 _loadGate 期间驱动 YARP 重建配置/回调而形成重入或死锁。
        var acquired = false;
        CancellationTokenSource? oldTokenSource = null;
        try
        {
            try
            {
                await _loadGate.WaitAsync(cancellationToken).ConfigureAwait(false);
                acquired = true;
            }
            catch (ObjectDisposedException)
            {
                // 检查通过后到此处之间发生 Dispose 的极端竞态：信号量已释放，放弃本次加载。
                return;
            }
            catch (OperationCanceledException)
            {
                // 调用方在等待信号量期间取消：与本类「加载失败降级、保留旧快照」的策略一致，
                // 静默放弃本次加载而非向上冒泡。此时尚未获取信号量（acquired 仍为 false），无需 Release。
                return;
            }

            _logger.LogInformation("Loading YARP configuration from database...");

            // 将传入 token 与生命周期 token 合并，使 Dispose 能主动取消进行中的数据库调用。
            // 通过 _disposed 检查、获取信号量后到此处之间可能发生 Dispose：先在锁内基于确定性的
            // _disposed 标志重新判定，若已置位则直接放弃本次加载，避免依赖「访问已 Dispose 的
            // _lifetimeCts.Token 是否抛 ObjectDisposedException」这一在 .NET 各版本间并不一致的行为。
            // 放弃加载同样避免在 throwOnFailure=true 的启动路径被误报为加载失败；finally 仍会归还
            // 已获取的信号量。极端竞态下（锁内检查通过后到下方 CreateLinkedTokenSource 之间发生 Dispose）
            // 保留的 try/catch 仅作幂等兜底。
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }
            }

            CancellationTokenSource linkedCts;
            try
            {
                linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _lifetimeCts.Token
                );
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            using var _ = linkedCts;
            var loadToken = linkedCts.Token;

            // 在同一只读事务/一致性快照内同时读取 routes 与 clusters，
            // 避免两次独立查询之间被其他请求修改配置而读到内部不一致的快照
            // （如 route 引用了尚未返回的 cluster），导致 YARP 校验拒绝该 route。
            var (routes, clusters) = await _configService.GetConfigSnapshotAsync(loadToken).ConfigureAwait(false);

            // 取消后不应应用其结果：若 cancellationToken 或 _lifetimeCts 在 await 期间被取消、
            // 而下游未抛 OperationCanceledException 而是正常返回，仍可能用过期数据覆盖较新的快照
            // （并发重载下的 lost update）。在写回前显式检查取消状态，被取消则放弃本次交换；
            // 抛出的 OperationCanceledException 会被下方 catch 按「保留旧快照」策略降级。
            loadToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Loaded {RouteCount} routes and {ClusterCount} clusters from database",
                routes.Count,
                clusters.Count
            );

            // 仅在锁内完成快照交换并取出旧 CTS；不在此处触发变更通知。
            // 取消检查在 SwapConfig 的 lock 内与 _disposed 并列重新判断，
            // 使「是否应用本次快照」与原子交换处于同一临界区，关闭检查到交换之间的窗口。
            oldTokenSource = SwapConfig(routes, clusters, loadToken);
        }
        catch (Exception ex)
        {
            // 启动路径（throwOnFailure=true）首次加载失败时 fail-fast：向上抛出以中断宿主启动，
            // 避免网关以空 routes/clusters 静默上线后对外提供服务而仅留下一条 LogError。
            // 后台重载路径（throwOnFailure=false）仍按原策略降级：保留当前快照，待 ReloadConfigAsync 重试恢复。
            _logger.LogError(ex, "Failed to load YARP configuration from database; keeping existing configuration snapshot");
            if (throwOnFailure)
            {
                throw;
            }
        }
        finally
        {
            if (acquired)
            {
                // 约束：此 Release 必须是 finally 中对 _loadGate 的最后一次访问。
                // DisposeLoadGateAsync 依赖「持有者 Release 后不再触碰 _loadGate」：一旦此处归还许可，
                // 后台 DisposeLoadGateAsync 可能立即被该许可唤醒并执行 _loadGate.Dispose()。
                // 切勿在 Release 之后再追加任何对 _loadGate 的访问（Wait/Release/CurrentCount 等），
                // 否则可能对已释放的 SemaphoreSlim 调用而抛 ObjectDisposedException。
                //
                // 与 DisposeLoadGateAsync 的契约：DisposeLoadGateAsync 仅在「WaitAsync 成功取出唯一许可」
                // 后才 Dispose，而此处的 Release() 正是它取出该许可的前提（SemaphoreSlim 在 Release 完成后
                // 才唤醒等待者），故正常路径下 Dispose() 必然发生在本 Release() 之后，二者不并发；超时路径
                // (acquired==false) 在 DisposeLoadGateAsync 中绝不 Dispose（仅记录告警并交由 GC 回收），
                // 因此在当前实现下本 Release() 永远不会遇到已释放的信号量——下方的 catch 在现有控制流中
                // 实为不可达分支。保留 try/catch 仅作为面向未来变更/极端竞态的幂等防御兜底，
                // 若后续维护重新引入「超时强制 Dispose」等改动，它可继续保证本 Release 幂等安全。
                try
                {
                    _loadGate.Release();
                }
                catch (ObjectDisposedException)
                {
                    // 当前实现下不可达（DisposeLoadGateAsync 仅在取出唯一许可后才 Dispose）；
                    // 仅作面向未来变更的幂等防御，忽略。
                }
            }
        }

        // 释放 _loadGate 之后再触发变更通知：Cancel() 默认会同步执行 IChangeToken 回调
        // （YARP 据此重建配置，可能回调 GetConfig 乃至 ReloadConfigAsync），
        // 移出信号量持有范围可缩小重入面、消除潜在死锁。
        NotifyChange(oldTokenSource);
    }

    /// <summary>
    /// Reload configuration (triggered after configuration update)
    /// </summary>
    public async Task ReloadConfigAsync()
    {
        await LoadConfigAsync();
        _logger.LogInformation("YARP configuration reloaded successfully");
    }

    /// <summary>
    /// 在锁内原子地交换配置快照与 ChangeToken，返回需要被取消/释放的旧 CTS（可能为 null）。
    /// 不在此处触发变更通知；通知由调用方在释放 _loadGate 之后通过 <see cref="NotifyChange"/> 完成。
    /// </summary>
    private CancellationTokenSource? SwapConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        CancellationToken cancellationToken
    )
    {
        lock (_lock)
        {
            // 在临界区内与 _disposed 并列重新检查取消：调用方在持锁前的 ThrowIfCancellationRequested()
            // 之后到此交换之间仍可能被取消，唯有在 lock 内决策才能保证不会用已过期快照覆盖较新快照。
            if (_disposed || cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // 先用局部变量构造新 CTS 与对应 ChangeToken，最后才写回字段，
            // 保证字段 _changeTokenSource 与 _config.ChangeToken 始终引用同一实例，
            // 避免与 Dispose 形成竞态时取消/释放到 YARP 正在监听的新实例。
            //
            // 顺序约束：newTokenSource 的创建必须保持在上面的 _disposed 检查之后。
            // 若调整顺序把它移到 _disposed 检查之前，则 Dispose 已置位时仍会 new 出 CTS，
            // 而后续 _disposed 分支直接 return，导致该 CTS 永远得不到 Dispose 的资源泄漏。
            var newTokenSource = new CancellationTokenSource();

            _config = new DatabaseProxyConfig(
                routes,
                clusters,
                new CancellationChangeToken(newTokenSource.Token)
            );
            // 不变量（禁止破坏）：从上面写回 _config 到下面的 Interlocked.Exchange 之间不得出现任何 await
            // 或可让出线程的异步调用。该「写回快照 → 交换字段」必须保持为同一临界区内的同步原子序列，
            // 才能保证字段 _changeTokenSource 与 _config.ChangeToken 始终引用同一 CancellationTokenSource
            // 实例；一旦插入 await，与并发 Dispose（其 Exchange 同样在锁内、由同一 _lock 串行化）之间可能
            // 出现字段与快照 ChangeToken 引用不同实例的窗口，导致 Cancel/Dispose 作用到 YARP 正在监听的另一实例。
            // 若后续维护需将 SwapConfig 改为异步，请勿在此区间引入 await。
            // 用 Exchange 取出旧实例并写回新实例，确保同一 CTS 只被一条路径取出，
            // 与 Dispose 并发时不会对同一实例双重 Cancel/Dispose。
            return Interlocked.Exchange(ref _changeTokenSource, newTokenSource);
        }
    }

    /// <summary>
    /// 在不持有任何锁的情况下触发旧 ChangeToken 的取消（同步回调驱动 YARP 重建配置），并释放旧 CTS。
    /// </summary>
    private static void NotifyChange(CancellationTokenSource? oldTokenSource)
    {
        if (oldTokenSource is null)
        {
            // 已被 Dispose 抢先取走并释放，或本次未发生交换，无需处理。
            return;
        }

        try
        {
            oldTokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 极端竞态下旧 CTS 可能已被 Dispose 并发释放，忽略以保持幂等。
        }
        finally
        {
            oldTokenSource.Dispose();
        }
    }

    public void Dispose()
    {
        CancellationTokenSource? tokenSource;
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            // 用 Exchange 取出当前实例并置 null，确保该 CTS 只被 Dispose 这一条路径取出，
            // 与并发的 UpdateConfig 不会重复 Cancel/Dispose 同一实例。
            tokenSource = Interlocked.Exchange(ref _changeTokenSource, null);
        }

        // 主动取消生命周期 token，中断任何进行中的数据库读取，缩短下方 _loadGate.Wait() 的阻塞。
        try
        {
            _lifetimeCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 幂等：极端竞态下可能已被释放，忽略。
        }
        catch (Exception ex)
        {
            // Cancel() 默认同步执行注册在 _lifetimeCts.Token 上的回调（如 CreateLinkedTokenSource
            // 的取消传播）。若任一回调抛出非 ObjectDisposedException 异常，Cancel() 会以 AggregateException
            // 形式抛出。同步 Dispose() 通常由 GC 终结器/容器在不可控上下文调用，异常逃逸可能导致进程级问题，
            // 故在此观察并记录，保持 Dispose 不抛出。
            _logger.LogError(ex, "Failed to cancel lifetime token during DatabaseProxyConfigProvider disposal");
        }
        finally
        {
            // 释放 CTS 持有的注册回调与计时器关联资源；对已释放实例再次 Dispose 是安全的幂等操作。
            _lifetimeCts.Dispose();
        }

        if (tokenSource is null)
        {
            // UpdateConfig 已抢先换走并负责释放该实例，这里只需保证 Dispose 幂等。
            GC.SuppressFinalize(this);
            return;
        }

        // 在锁外触发取消：Cancel() 默认会同步执行 IChangeToken 回调，
        // 回调链路可能访问其他资源，持锁触发会增加重入/死锁面并延长锁持有时间。
        try
        {
            tokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 极端竞态下该 CTS 可能已被并发释放，忽略即可保证 Dispose 幂等。
        }
        finally
        {
            tokenSource.Dispose();
        }

        // 同步 Dispose() 不阻塞等待进行中的 LoadConfigAsync。本类所有 await（含 LoadConfigAsync 的
        // WaitAsync、GetConfigSnapshotAsync 以及 DisposeLoadGateAsync）均使用 ConfigureAwait(false)，
        // 其续体不会回到 Dispose 调用线程捕获的 SynchronizationContext。因此即便在带同步上下文的线程
        // （如 UI/同步上下文）上同步 Wait()，在途加载续体也不会因「需回到原同步上下文」而被阻塞死锁。
        // 但同步 sync-over-async 等待仍会占用并阻塞当前线程长达约 30s（DisposeLoadGateAsync 的等待上限），
        // 在 UI/请求线程上代价高昂。故同步 Dispose() 采用「fire-and-forget」：_disposed 已置位会在 WaitAsync
        // 前拒绝新加载，_lifetimeCts 已取消会尽快结束在途调用，待当前持有者释放许可后由后台任务 Dispose 信号量，
        // 不在当前线程上同步阻塞等待。
        //
        // 注意：同步 Dispose() 返回后，_loadGate 在最坏情况下仍可能存活约 30s（DisposeLoadGateAsync 的等待上限）。
        // 若调用方（宿主关闭、健康检查、诊断等）需要「关闭返回即信号量已释放」的确定性语义，
        // 请改用 DisposeAsync() 并 await 之，使关闭路径可确定性等待 _loadGate 释放。
        _ = DisposeLoadGateAsync();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 可 await 的异步释放：执行与同步 Dispose 相同的取消/释放语义，并确定性等待
    /// DisposeLoadGateAsync 完成（即等待 _loadGate 被取出唯一许可后 Dispose 或超时放弃），
    /// 使宿主在关闭路径能确定性回收信号量，而不像同步 Dispose 那样让后台任务在最长约 30s 内存活。
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeCoreAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeCoreAsync()
    {
        CancellationTokenSource? tokenSource;
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            // 用 Exchange 取出当前实例并置 null，确保该 CTS 只被这一条路径取出，
            // 与并发的 UpdateConfig 不会重复 Cancel/Dispose 同一实例。
            tokenSource = Interlocked.Exchange(ref _changeTokenSource, null);
        }

        // 主动取消生命周期 token，中断任何进行中的数据库读取，缩短下方 DisposeLoadGateAsync 的等待。
        try
        {
            _lifetimeCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 幂等：极端竞态下可能已被释放，忽略。
        }
        catch (Exception ex)
        {
            // Cancel() 默认同步执行注册在 _lifetimeCts.Token 上的回调（如 CreateLinkedTokenSource
            // 的取消传播）。若任一回调抛出非 ObjectDisposedException 异常，Cancel() 会以 AggregateException
            // 形式抛出。在此观察并记录，保持 DisposeAsync 路径不被回调异常打断。
            _logger.LogError(ex, "Failed to cancel lifetime token during DatabaseProxyConfigProvider disposal");
        }
        finally
        {
            // 释放 CTS 持有的注册回调与计时器关联资源；对已释放实例再次 Dispose 是安全的幂等操作。
            _lifetimeCts.Dispose();
        }

        if (tokenSource is null)
        {
            // UpdateConfig 已抢先换走并负责释放该实例，这里只需保证幂等。
            return;
        }

        // 在锁外触发取消：Cancel() 默认会同步执行 IChangeToken 回调，
        // 回调链路可能访问其他资源，持锁触发会增加重入/死锁面并延长锁持有时间。
        try
        {
            tokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 极端竞态下该 CTS 可能已被并发释放，忽略即可保证幂等。
        }
        finally
        {
            tokenSource.Dispose();
        }

        // 与同步 Dispose 不同，这里 await DisposeLoadGateAsync()：确定性等待在途加载释放 _loadGate
        // 后回收信号量（或在 30s 超时后按既有策略放弃 Dispose、交由 GC 回收）。
        await DisposeLoadGateAsync().ConfigureAwait(false);
    }

    private async Task DisposeLoadGateAsync()
    {
        // 该方法既被同步 Dispose() 以 fire-and-forget 方式调用，也被 DisposeCoreAsync() await，
        // 顶层 catch 兜底确保任何异常都被观察并记录，不会成为未观察的任务异常。
        var acquired = false;
        try
        {
            try
            {
                // 仅当取出唯一许可（确认无在途持有者会再 Release）后才 Dispose，确保 Dispose() 永不与
                // 持有者 finally 中的 Release() 并发访问同一 SemaphoreSlim。使用带超时的 WaitAsync，避免在途
                // 持有者（其 await 续体长期无法调度等）导致此处永久挂起。
                acquired = await _loadGate.WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                // 极端竞态下信号量可能已被释放，忽略以保持幂等。
                return;
            }

            if (!acquired)
            {
                // 超时仍未拿到许可：存在长时间未释放 _loadGate 的在途加载（其 await 续体可能被长期阻塞/死锁）。
                // 记录告警以暴露潜在死锁/泄漏，但不强制 Dispose 信号量——SemaphoreSlim.Dispose() 与在途
                // 持有者的 Release() 并非线程安全（可能抛出非 ObjectDisposedException 的异常并逃逸为未观察任务
                // 异常）。改为放弃 Dispose，让该信号量由 GC 回收；在未使用 AvailableWaitHandle 的路径下
                // SemaphoreSlim 不持有计时器/句柄，不 Dispose 不会造成资源泄漏。
                // 注意：本超时分支绝不 Dispose 信号量，因此在途持有者 LoadConfigAsync finally 中的
                // Release() 不会遇到已释放的 SemaphoreSlim。后续维护若在此恢复「超时强制 Dispose」，
                // 将重新引入 Dispose() 与在途 Release() 的竞态——请勿这样做。
                _logger.LogWarning(
                    "Timed out (30s) waiting for in-flight YARP configuration load to release _loadGate during DatabaseProxyConfigProvider disposal; skipping semaphore disposal to avoid a race with the in-flight load's Release(). This may indicate a stalled/deadlocked load."
                );
                return;
            }

            // 已取出唯一许可，无在途持有者会再 Release，可安全释放信号量。
            _loadGate.Dispose();
        }
        catch (Exception ex)
        {
            // 兜底：WaitAsync 因取消语义变更或框架行为等抛出的其它异常在此被观察并记录，
            // 避免成为未观察的任务异常而难以诊断。
            // 仅在已确认取出唯一许可（acquired==true）后才 Dispose：此时无在途持有者会再 Release，
            // 不会与持有者 finally 中的 Release() 并发，避免触发 SemaphoreSlim.Dispose() 与 Release()
            // 的非线程安全竞态（恰是本类注释声称要避免的情形）。若尚未取出许可则放弃 Dispose，
            // 与超时分支保持一致地交由 GC 回收（未访问 AvailableWaitHandle 时不持有内核句柄，不会泄漏）。
            _logger.LogError(ex, "Failed to dispose load gate semaphore during DatabaseProxyConfigProvider disposal");
            if (acquired)
            {
                try
                {
                    _loadGate.Dispose();
                }
                catch (Exception disposeEx)
                {
                    _logger.LogError(disposeEx, "Failed to dispose load gate semaphore after acquiring permit during DatabaseProxyConfigProvider disposal");
                }
            }
        }
    }
}

/// <summary>
/// Hosted service that loads the initial YARP configuration during host startup,
/// so that GetConfig() never has to block waiting for async initialization.
/// Remember to register it: services.AddHostedService&lt;ProxyConfigInitializer&gt;();
/// </summary>
public class ProxyConfigInitializer : IHostedService
{
    private readonly DatabaseProxyConfigProvider _provider;

    public ProxyConfigInitializer(DatabaseProxyConfigProvider provider)
    {
        _provider = provider;
    }

    // 首次加载使用 fail-fast：数据库不可用时抛出以中断宿主启动，避免网关以空配置静默上线。
    public Task StartAsync(CancellationToken cancellationToken) =>
        _provider.LoadConfigAsync(cancellationToken, throwOnFailure: true);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


/// <summary>
/// Database configuration snapshot
/// </summary>
public class DatabaseProxyConfig : IProxyConfig
{
    public DatabaseProxyConfig(
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        IChangeToken changeToken
    )
    {
        Routes = routes;
        Clusters = clusters;
        ChangeToken = changeToken;
    }

    public IReadOnlyList<RouteConfig> Routes { get; }
    public IReadOnlyList<ClusterConfig> Clusters { get; }
    public IChangeToken ChangeToken { get; }
}
