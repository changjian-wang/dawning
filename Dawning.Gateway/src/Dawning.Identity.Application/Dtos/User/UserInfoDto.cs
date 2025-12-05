namespace Dawning.Identity.Application.Dtos.User
{
    /// <summary>
    /// 用户信息响应 DTO
    /// </summary>
    public class UserInfoDto
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
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 头像 URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }
    }
}
