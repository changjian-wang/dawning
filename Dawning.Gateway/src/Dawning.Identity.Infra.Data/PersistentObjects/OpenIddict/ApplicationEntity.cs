using System;
using Dawning.Shared.Dapper.Contrib;

namespace Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict
{
    /// <summary>
    /// OpenIddict 应用程序持久化对象
    /// </summary>
    [Table("openiddict_applications")]
    public class ApplicationEntity
    {
        [ExplicitKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("client_id")]
        public string? ClientId { get; set; }

        [Column("client_secret")]
        public string? ClientSecret { get; set; }

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("consent_type")]
        public string? ConsentType { get; set; }

        /// <summary>
        /// 权限列表（JSON 格式存储）
        /// </summary>
        [Column("permissions")]
        public string? PermissionsJson { get; set; }

        /// <summary>
        /// 重定向 URI（JSON 格式存储）
        /// </summary>
        [Column("redirect_uris")]
        public string? RedirectUrisJson { get; set; }

        /// <summary>
        /// 登出重定向 URI（JSON 格式存储）
        /// </summary>
        [Column("post_logout_redirect_uris")]
        public string? PostLogoutRedirectUrisJson { get; set; }

        [Column("requirements")]
        public string? RequirementsJson { get; set; }

        [Column("properties")]
        public string? PropertiesJson { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [Column("timestamp")]
        [IgnoreUpdate]
        [DefaultSortName]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        [Column("created_at")]
        [IgnoreUpdate]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

