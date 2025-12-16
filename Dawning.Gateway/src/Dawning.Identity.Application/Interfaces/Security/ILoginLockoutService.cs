using System;
using System.Threading.Tasks;

namespace Dawning.Identity.Application.Interfaces.Security
{
    /// <summary>
    /// 登录锁定服务接口
    /// </summary>
    public interface ILoginLockoutService
    {
        /// <summary>
        /// 检查用户是否被锁定
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>如果被锁定，返回锁定结束时间；否则返回 null</returns>
        Task<DateTime?> IsLockedOutAsync(string username);

        /// <summary>
        /// 记录登录失败
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>返回当前失败次数和是否被锁定</returns>
        Task<(int failedCount, bool isLockedOut, DateTime? lockoutEnd)> RecordFailedLoginAsync(
            string username
        );

        /// <summary>
        /// 重置登录失败计数（登录成功后调用）
        /// </summary>
        /// <param name="username">用户名</param>
        Task ResetFailedCountAsync(string username);

        /// <summary>
        /// 解锁用户账户
        /// </summary>
        /// <param name="userId">用户ID</param>
        Task UnlockUserAsync(Guid userId);

        /// <summary>
        /// 获取锁定配置
        /// </summary>
        Task<LockoutSettings> GetLockoutSettingsAsync();
    }

    /// <summary>
    /// 锁定配置
    /// </summary>
    public class LockoutSettings
    {
        /// <summary>
        /// 是否启用登录锁定
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 最大失败次数
        /// </summary>
        public int MaxFailedAttempts { get; set; } = 5;

        /// <summary>
        /// 锁定时长（分钟）
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;
    }
}
