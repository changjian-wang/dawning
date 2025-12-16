using System;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Application.Interfaces.Security;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Dawning.Identity.Application.Services.Security
{
    /// <summary>
    /// 登录锁定服务实现
    /// </summary>
    public class LoginLockoutService : ILoginLockoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _connectionString;
        private LockoutSettings? _cachedSettings;
        private DateTime _settingsCacheTime;
        private static readonly TimeSpan SettingsCacheDuration = TimeSpan.FromMinutes(5);

        public LoginLockoutService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _connectionString = configuration.GetConnectionString("MySQL") 
                ?? throw new InvalidOperationException("MySQL connection string not found");
        }

        /// <summary>
        /// 检查用户是否被锁定
        /// </summary>
        public async Task<DateTime?> IsLockedOutAsync(string username)
        {
            var settings = await GetLockoutSettingsAsync();
            if (!settings.Enabled)
            {
                return null;
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT lockout_end 
                FROM users 
                WHERE username = @Username 
                  AND lockout_enabled = 1 
                  AND lockout_end IS NOT NULL 
                  AND lockout_end > @Now";

            var lockoutEnd = await connection.QueryFirstOrDefaultAsync<DateTime?>(sql, new 
            { 
                Username = username,
                Now = DateTime.UtcNow
            });

            return lockoutEnd;
        }

        /// <summary>
        /// 记录登录失败
        /// </summary>
        public async Task<(int failedCount, bool isLockedOut, DateTime? lockoutEnd)> RecordFailedLoginAsync(string username)
        {
            var settings = await GetLockoutSettingsAsync();
            if (!settings.Enabled)
            {
                return (0, false, null);
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 增加失败计数并检查是否需要锁定
            var sql = @"
                UPDATE users 
                SET failed_login_count = failed_login_count + 1,
                    lockout_end = CASE 
                        WHEN failed_login_count + 1 >= @MaxAttempts AND lockout_enabled = 1 
                        THEN DATE_ADD(@Now, INTERVAL @LockoutMinutes MINUTE)
                        ELSE lockout_end 
                    END,
                    updated_at = @Now
                WHERE username = @Username";

            await connection.ExecuteAsync(sql, new
            {
                Username = username,
                MaxAttempts = settings.MaxFailedAttempts,
                LockoutMinutes = settings.LockoutDurationMinutes,
                Now = DateTime.UtcNow
            });

            // 获取更新后的状态
            var statusSql = @"
                SELECT failed_login_count, lockout_end 
                FROM users 
                WHERE username = @Username";

            var status = await connection.QueryFirstOrDefaultAsync<(int FailedCount, DateTime? LockoutEnd)>(
                statusSql, new { Username = username });

            var isLockedOut = status.LockoutEnd.HasValue && status.LockoutEnd > DateTime.UtcNow;
            
            return (status.FailedCount, isLockedOut, status.LockoutEnd);
        }

        /// <summary>
        /// 重置登录失败计数
        /// </summary>
        public async Task ResetFailedCountAsync(string username)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                UPDATE users 
                SET failed_login_count = 0, 
                    lockout_end = NULL,
                    updated_at = @Now
                WHERE username = @Username";

            await connection.ExecuteAsync(sql, new
            {
                Username = username,
                Now = DateTime.UtcNow
            });
        }

        /// <summary>
        /// 解锁用户账户
        /// </summary>
        public async Task UnlockUserAsync(Guid userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                UPDATE users 
                SET failed_login_count = 0, 
                    lockout_end = NULL,
                    updated_at = @Now
                WHERE id = @UserId";

            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Now = DateTime.UtcNow
            });
        }

        /// <summary>
        /// 获取锁定配置
        /// </summary>
        public async Task<LockoutSettings> GetLockoutSettingsAsync()
        {
            // 使用简单缓存避免频繁查询数据库
            if (_cachedSettings != null && DateTime.UtcNow - _settingsCacheTime < SettingsCacheDuration)
            {
                return _cachedSettings;
            }

            var settings = new LockoutSettings();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT config_key, config_value 
                    FROM system_configs 
                    WHERE config_group = 'Security' 
                      AND config_key IN ('EnableLoginLockout', 'MaxFailedLoginAttempts', 'LockoutDurationMinutes')";

                var configs = await connection.QueryAsync<(string Key, string Value)>(sql);

                foreach (var config in configs)
                {
                    switch (config.Key)
                    {
                        case "EnableLoginLockout":
                            settings.Enabled = bool.TryParse(config.Value, out var enabled) && enabled;
                            break;
                        case "MaxFailedLoginAttempts":
                            if (int.TryParse(config.Value, out var maxAttempts))
                                settings.MaxFailedAttempts = maxAttempts;
                            break;
                        case "LockoutDurationMinutes":
                            if (int.TryParse(config.Value, out var duration))
                                settings.LockoutDurationMinutes = duration;
                            break;
                    }
                }
            }
            catch
            {
                // 如果读取配置失败，使用默认值
            }

            _cachedSettings = settings;
            _settingsCacheTime = DateTime.UtcNow;

            return settings;
        }
    }
}
