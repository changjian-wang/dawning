namespace Dawning.Identity.Domain.Models.Gateway
{
    /// <summary>
    /// 网关路由查询模型
    /// </summary>
    public class GatewayRouteQueryModel
    {
        /// <summary>
        /// 路由ID（模糊搜索）
        /// </summary>
        public string? RouteId { get; set; }

        /// <summary>
        /// 路由名称（模糊搜索）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 关联的集群ID
        /// </summary>
        public string? ClusterId { get; set; }

        /// <summary>
        /// 匹配路径（模糊搜索）
        /// </summary>
        public string? MatchPath { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// 关键词（搜索路由ID、名称、描述、匹配路径）
        /// </summary>
        public string? Keyword { get; set; }
    }
}
