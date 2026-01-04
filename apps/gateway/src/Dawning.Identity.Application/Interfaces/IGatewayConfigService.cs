using System.Collections.Generic;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace Dawning.Identity.Application.Interfaces
{
    /// <summary>
    /// Gateway configuration service interface
    /// </summary>
    public interface IGatewayConfigService
    {
        /// <summary>
        /// Gets all enabled route configurations
        /// </summary>
        Task<IReadOnlyList<RouteConfig>> GetRoutesAsync();

        /// <summary>
        /// Gets all enabled cluster configurations
        /// </summary>
        Task<IReadOnlyList<ClusterConfig>> GetClustersAsync();
    }
}
