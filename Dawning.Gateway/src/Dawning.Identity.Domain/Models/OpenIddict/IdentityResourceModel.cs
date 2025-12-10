namespace Dawning.Identity.Domain.Models.OpenIddict
{
    /// <summary>
    /// 身份资源查询模型
    /// </summary>
    public class IdentityResourceModel
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
        /// 是否必须同意
        /// </summary>
        public bool? Required { get; set; }
    }
}
