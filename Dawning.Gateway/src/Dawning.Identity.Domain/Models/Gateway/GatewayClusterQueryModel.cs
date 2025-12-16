namespace Dawning.Identity.Domain.Models.Gateway
{
    /// <summary>
    /// 网关集群查询模型
    /// </summary>
    public class GatewayClusterQueryModel
    {
        /// <summary>
        /// 集群ID（模糊搜索）
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// 集群名称（模糊搜索）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 负载均衡策略
        /// </summary>
        public string? LoadBalancingPolicy { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// 关键词（搜索集群ID、名称、描述）
        /// </summary>
        public string? Keyword { get; set; }
    }
}
