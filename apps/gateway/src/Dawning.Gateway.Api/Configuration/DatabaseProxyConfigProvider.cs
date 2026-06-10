using Dawning.Identity.Application.Interfaces;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// YARP configuration provider - loads route and cluster configuration from database
/// </summary>
public class DatabaseProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly IGatewayConfigService _configService;
    private readonly ILogger<DatabaseProxyConfigProvider> _logger;
    private DatabaseProxyConfig _config;
    private CancellationTokenSource? _changeTokenSource;
    private readonly object _lock = new();
    // 串行化整个「读取 routes/clusters → UpdateConfig」流程，保证同一时刻只有一次加载-应用，
    // 避免并发 ReloadConfigAsync 时后完成的旧数据库快照覆盖较新的快照（lost update / stale overwrite）。
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
            // Dispose 后 _changeTokenSource 已被取消并释放，继续返回旧快照会让
            // YARP 拿到一个已永久触发/已释放的 ChangeToken，故快速失败。
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseProxyConfigProvider));
            }

            return _config;
        }
    }


    /// <summary>
    /// Load configuration from database
    /// </summary>
    public async Task LoadConfigAsync(CancellationToken cancellationToken = default)
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
                await _loadGate.WaitAsync(cancellationToken);
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
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _lifetimeCts.Token
            );
            var loadToken = linkedCts.Token;

            var routes = await _configService.GetRoutesAsync(loadToken);
            var clusters = await _configService.GetClustersAsync(loadToken);

            _logger.LogInformation(
                "Loaded {RouteCount} routes and {ClusterCount} clusters from database",
                routes.Count,
                clusters.Count
            );

            // 仅在锁内完成快照交换并取出旧 CTS；不在此处触发变更通知。
            oldTokenSource = SwapConfig(routes, clusters);
        }
        catch (Exception ex)
        {
            // 初始化/重载失败时降级：保留当前（可能为空）配置快照，避免抛出导致宿主启动中断、网关整体不可用。
            // 后续可由 ReloadConfigAsync 重试恢复。
            _logger.LogError(ex, "Failed to load YARP configuration from database; keeping existing configuration snapshot");
        }
        finally
        {
            if (acquired)
            {
                _loadGate.Release();
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
        IReadOnlyList<ClusterConfig> clusters
    )
    {
        lock (_lock)
        {
            if (_disposed)
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

        // 等待任何进行中的 LoadConfigAsync 释放信号量后再 Dispose，避免持有者在其 finally
        // 中对已释放的 SemaphoreSlim 调用 Release() 抛出 ObjectDisposedException。
        // 此处已置位 _disposed，新的加载会在 WaitAsync 前被拒绝，故只需等待当前持有者完成。
        // 改为带超时的同步等待：若某次进行中的加载仍卡在下游数据库调用（各自的 cancellationToken
        // Dispose 无法主动取消），无限 Wait() 会让 Dispose 不可控地长时间阻塞甚至在带
        // SynchronizationContext 的环境下死锁。超时未获取到许可时记录警告并放弃 Dispose 该信号量
        // （其句柄量很小，交由 GC 回收），优先保证 Dispose 能及时返回。
        if (_loadGate.Wait(TimeSpan.FromSeconds(30)))
        {
            // 获取到的许可不再 Release，直接释放信号量。
            _loadGate.Dispose();
        }
        else
        {
            _logger.LogWarning(
                "Timed out waiting for in-flight LoadConfigAsync to release the load gate during Dispose; " +
                "skipping SemaphoreSlim disposal to avoid blocking shutdown."
            );
        }

        GC.SuppressFinalize(this);
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

    public Task StartAsync(CancellationToken cancellationToken) => _provider.LoadConfigAsync(cancellationToken);

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
