using System;
using Dawning.Identity.Domain.Core.Interfaces;
using Dawning.Identity.Domain.Core.Security;

namespace Dawning.Identity.Domain.Aggregates.OpenIddict
{
    /// <summary>
    /// OpenIddict 应用程序聚合根
    /// </summary>
    public class Application : IAggregateRoot
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 客户端 ID
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// 客户端密钥（哈希后）
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 客户端类型（confidential, public）
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 同意类型（explicit, implicit, systematic）
        /// </summary>
        public string? ConsentType { get; set; }

        /// <summary>
        /// 权限列表（JSON 序列化）
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// 重定向 URI 列表
        /// </summary>
        public List<string> RedirectUris { get; set; } = new();

        /// <summary>
        /// 登出重定向 URI 列表
        /// </summary>
        public List<string> PostLogoutRedirectUris { get; set; } = new();

        /// <summary>
        /// 要求列表
        /// </summary>
        public List<string> Requirements { get; set; } = new();

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 业务方法：验证客户端密钥
        /// </summary>
        public bool ValidateClientSecret(string secret)
        {
            if (string.IsNullOrEmpty(ClientSecret))
            {
                return false;
            }

            // 使用 PBKDF2 验证密钥哈希
            return PasswordHasher.Verify(secret, ClientSecret);
        }

        /// <summary>
        /// 业务方法：检查是否有指定权限
        /// </summary>
        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}

