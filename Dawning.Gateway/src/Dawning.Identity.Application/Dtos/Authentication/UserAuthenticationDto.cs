namespace Dawning.Identity.Application.Dtos.Authentication
{
    /// <summary>
    /// 用户认证模型
    /// </summary>
    public class UserAuthenticationDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 角色列表（从user_roles表加载）
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
