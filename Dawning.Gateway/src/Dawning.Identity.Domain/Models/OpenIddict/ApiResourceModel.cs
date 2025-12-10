namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// API资源查询模型
    /// </summary>
    public class ApiResourceModel
    {
        /// <summary>
        /// 资源名称(模糊匹配)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 显示名称(模糊匹配)
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// 关联的作用域(精确匹配)
        /// </summary>
        public string? Scope { get; set; }
    }
}
