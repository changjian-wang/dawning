using Yarp.ReverseProxy.Configuration;

namespace Dawning.Gateway.Api.Configuration;

/// <summary>
/// 网关配置服务接口
/// </summary>
public interface IGatewayConfigService
{
    /// <summary>
    /// 获取所有启用的路由配置
    /// </summary>
    Task<IReadOnlyList<RouteConfig>> GetRoutesAsync();

    /// <summary>
    /// 获取所有启用的集群配置
    /// </summary>
    Task<IReadOnlyList<ClusterConfig>> GetClustersAsync();
}
