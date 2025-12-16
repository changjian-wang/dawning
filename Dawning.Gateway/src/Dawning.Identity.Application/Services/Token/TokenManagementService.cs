using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dawning.Identity.Application.Interfaces.Token;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using OpenIddict.Abstractions;

namespace Dawning.Identity.Application.Services.Token
{
    /// <summary>
    /// Token 管理服务实现
    /// </summary>
    public class TokenManagementService : ITokenManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _connectionString;
        private readonly ITokenBlacklistService? _blacklistService;
        private LoginPolicySettings? _cachedPolicy;
        private DateTime _policyCacheTime;
        private static readonly TimeSpan PolicyCacheDuration = TimeSpan.FromMinutes(5);

        public TokenManagementService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ITokenBlacklistService? blacklistService = null)
        {
            _unitOfWork = unitOfWork;
            _connectionString = configuration.GetConnectionString("MySQL")
                ?? throw new InvalidOperationException("MySQL connection string not found");
            _blacklistService = blacklistService;
        }

        /// <summary>
        /// 撤销用户的所有令牌
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(Guid userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 获取用户的所有有效令牌
            var sql = @"
                SELECT id, expiration_date 
                FROM openiddict_tokens 
                WHERE subject = @UserId AND status = @ValidStatus";

            var tokens = await connection.QueryAsync<(Guid Id, DateTime? ExpirationDate)>(sql, new
            {
                UserId = userId.ToString(),
                ValidStatus = OpenIddictConstants.Statuses.Valid
            });

            var tokenList = tokens.ToList();
            if (tokenList.Count == 0)
                return 0;

            // 更新状态为已撤销
            var updateSql = @"
                UPDATE openiddict_tokens 
                SET status = @RevokedStatus, updated_at = @Now
                WHERE subject = @UserId AND status = @ValidStatus";

            var affected = await connection.ExecuteAsync(updateSql, new
            {
                UserId = userId.ToString(),
                RevokedStatus = OpenIddictConstants.Statuses.Revoked,
                ValidStatus = OpenIddictConstants.Statuses.Valid,
                Now = DateTime.UtcNow
            });

            // 将令牌加入黑名单（用于快速验证）
            if (_blacklistService != null)
            {
                foreach (var token in tokenList)
                {
                    var expiration = token.ExpirationDate ?? DateTime.UtcNow.AddDays(30);
                    await _blacklistService.AddToBlacklistAsync(token.Id.ToString(), expiration);
                }
            }

            return affected;
        }

        /// <summary>
        /// 撤销指定令牌
        /// </summary>
        public async Task<bool> RevokeTokenAsync(Guid tokenId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 获取令牌信息
            var selectSql = @"
                SELECT expiration_date 
                FROM openiddict_tokens 
                WHERE id = @TokenId AND status = @ValidStatus";

            var expirationDate = await connection.QueryFirstOrDefaultAsync<DateTime?>(selectSql, new
            {
                TokenId = tokenId,
                ValidStatus = OpenIddictConstants.Statuses.Valid
            });

            if (expirationDate == null)
                return false;

            // 更新状态
            var updateSql = @"
                UPDATE openiddict_tokens 
                SET status = @RevokedStatus, updated_at = @Now
                WHERE id = @TokenId";

            var affected = await connection.ExecuteAsync(updateSql, new
            {
                TokenId = tokenId,
                RevokedStatus = OpenIddictConstants.Statuses.Revoked,
                Now = DateTime.UtcNow
            });

            // 加入黑名单
            if (_blacklistService != null && affected > 0)
            {
                await _blacklistService.AddToBlacklistAsync(
                    tokenId.ToString(),
                    expirationDate ?? DateTime.UtcNow.AddDays(30));
            }

            return affected > 0;
        }

        /// <summary>
        /// 撤销指定设备的令牌
        /// </summary>
        public async Task<int> RevokeDeviceTokensAsync(Guid userId, string deviceId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 从会话表获取设备关联的令牌
            var selectSql = @"
                SELECT token_id, t.expiration_date
                FROM user_sessions s
                INNER JOIN openiddict_tokens t ON s.token_id = t.id
                WHERE s.user_id = @UserId 
                  AND s.device_id = @DeviceId 
                  AND t.status = @ValidStatus";

            var tokens = await connection.QueryAsync<(Guid TokenId, DateTime? ExpirationDate)>(selectSql, new
            {
                UserId = userId,
                DeviceId = deviceId,
                ValidStatus = OpenIddictConstants.Statuses.Valid
            });

            var tokenList = tokens.ToList();
            if (tokenList.Count == 0)
                return 0;

            // 撤销令牌
            var tokenIds = tokenList.Select(t => t.TokenId).ToList();
            var updateSql = @"
                UPDATE openiddict_tokens 
                SET status = @RevokedStatus, updated_at = @Now
                WHERE id IN @TokenIds";

            var affected = await connection.ExecuteAsync(updateSql, new
            {
                TokenIds = tokenIds,
                RevokedStatus = OpenIddictConstants.Statuses.Revoked,
                Now = DateTime.UtcNow
            });

            // 删除会话记录
            var deleteSessionSql = @"
                DELETE FROM user_sessions 
                WHERE user_id = @UserId AND device_id = @DeviceId";

            await connection.ExecuteAsync(deleteSessionSql, new
            {
                UserId = userId,
                DeviceId = deviceId
            });

            // 加入黑名单
            if (_blacklistService != null)
            {
                foreach (var token in tokenList)
                {
                    await _blacklistService.AddToBlacklistAsync(
                        token.TokenId.ToString(),
                        token.ExpirationDate ?? DateTime.UtcNow.AddDays(30));
                }
            }

            return affected;
        }

        /// <summary>
        /// 获取用户的活跃会话列表
        /// </summary>
        public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT 
                    s.id AS SessionId,
                    s.device_id AS DeviceId,
                    s.device_type AS DeviceType,
                    s.device_name AS DeviceName,
                    s.ip_address AS IpAddress,
                    s.login_time AS LoginTime,
                    s.last_active_time AS LastActiveTime
                FROM user_sessions s
                INNER JOIN openiddict_tokens t ON s.token_id = t.id
                WHERE s.user_id = @UserId 
                  AND t.status = @ValidStatus
                  AND (t.expiration_date IS NULL OR t.expiration_date > @Now)
                ORDER BY s.last_active_time DESC";

            var sessions = await connection.QueryAsync<UserSessionDto>(sql, new
            {
                UserId = userId,
                ValidStatus = OpenIddictConstants.Statuses.Valid,
                Now = DateTime.UtcNow
            });

            return sessions;
        }

        /// <summary>
        /// 记录用户登录会话
        /// </summary>
        public async Task RecordLoginSessionAsync(LoginSessionInfo session)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 检查是否已有相同设备的会话
            var existsSql = @"
                SELECT id FROM user_sessions 
                WHERE user_id = @UserId AND device_id = @DeviceId";

            var existingId = await connection.QueryFirstOrDefaultAsync<Guid?>(existsSql, new
            {
                session.UserId,
                session.DeviceId
            });

            if (existingId.HasValue)
            {
                // 更新现有会话
                var updateSql = @"
                    UPDATE user_sessions 
                    SET token_id = @TokenId,
                        device_name = @DeviceName,
                        ip_address = @IpAddress,
                        login_time = @Now,
                        last_active_time = @Now
                    WHERE id = @Id";

                await connection.ExecuteAsync(updateSql, new
                {
                    Id = existingId.Value,
                    session.TokenId,
                    session.DeviceName,
                    session.IpAddress,
                    Now = DateTime.UtcNow
                });
            }
            else
            {
                // 创建新会话
                var insertSql = @"
                    INSERT INTO user_sessions 
                    (id, user_id, token_id, device_id, device_type, device_name, ip_address, login_time, last_active_time)
                    VALUES 
                    (@Id, @UserId, @TokenId, @DeviceId, @DeviceType, @DeviceName, @IpAddress, @Now, @Now)";

                await connection.ExecuteAsync(insertSql, new
                {
                    Id = Guid.NewGuid(),
                    session.UserId,
                    session.TokenId,
                    session.DeviceId,
                    session.DeviceType,
                    session.DeviceName,
                    session.IpAddress,
                    Now = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 检查是否允许登录（基于设备策略）
        /// </summary>
        public async Task<(bool allowed, string? message)> CheckLoginPolicyAsync(Guid userId, string deviceId)
        {
            var policy = await GetLoginPolicyAsync();

            // 如果允许多设备且无限制，直接通过
            if (policy.AllowMultipleDevices && policy.MaxDevices == 0)
            {
                return (true, null);
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 获取当前活跃会话数
            var countSql = @"
                SELECT COUNT(*) FROM user_sessions s
                INNER JOIN openiddict_tokens t ON s.token_id = t.id
                WHERE s.user_id = @UserId 
                  AND t.status = @ValidStatus
                  AND (t.expiration_date IS NULL OR t.expiration_date > @Now)";

            var activeCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                UserId = userId,
                ValidStatus = OpenIddictConstants.Statuses.Valid,
                Now = DateTime.UtcNow
            });

            // 检查是否是已有设备
            var existsDeviceSql = @"
                SELECT COUNT(*) FROM user_sessions 
                WHERE user_id = @UserId AND device_id = @DeviceId";

            var isExistingDevice = await connection.ExecuteScalarAsync<int>(existsDeviceSql, new
            {
                UserId = userId,
                DeviceId = deviceId
            }) > 0;

            // 已有设备始终允许
            if (isExistingDevice)
            {
                return (true, null);
            }

            // 如果不允许多设备
            if (!policy.AllowMultipleDevices && activeCount > 0)
            {
                return policy.NewDevicePolicy switch
                {
                    "deny" => (false, "不允许在多个设备上同时登录"),
                    "kick_oldest" => (true, null), // 允许，但会踢掉最旧的会话
                    _ => (true, null)
                };
            }

            // 检查设备数量限制
            if (policy.MaxDevices > 0 && activeCount >= policy.MaxDevices)
            {
                return policy.NewDevicePolicy switch
                {
                    "deny" => (false, $"已达到最大设备数限制 ({policy.MaxDevices})"),
                    "kick_oldest" => (true, null),
                    _ => (true, null)
                };
            }

            return (true, null);
        }

        /// <summary>
        /// 获取登录策略设置
        /// </summary>
        public async Task<LoginPolicySettings> GetLoginPolicyAsync()
        {
            if (_cachedPolicy != null && DateTime.UtcNow - _policyCacheTime < PolicyCacheDuration)
            {
                return _cachedPolicy;
            }

            var policy = new LoginPolicySettings();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT config_key, config_value 
                    FROM system_configs 
                    WHERE config_group = 'Security' 
                      AND config_key LIKE '%Device%' OR config_key LIKE '%Token%'";

                var configs = await connection.QueryAsync<(string Key, string Value)>(sql);

                foreach (var config in configs)
                {
                    switch (config.Key)
                    {
                        case "AllowMultipleDevices":
                            policy.AllowMultipleDevices = bool.TryParse(config.Value, out var allow) && allow;
                            break;
                        case "MaxDevices":
                            if (int.TryParse(config.Value, out var maxDevices))
                                policy.MaxDevices = maxDevices;
                            break;
                        case "NewDevicePolicy":
                            policy.NewDevicePolicy = config.Value;
                            break;
                        case "RefreshTokenLifetimeDays":
                            if (int.TryParse(config.Value, out var refreshDays))
                                policy.RefreshTokenLifetimeDays = refreshDays;
                            break;
                        case "AccessTokenLifetimeMinutes":
                            if (int.TryParse(config.Value, out var accessMinutes))
                                policy.AccessTokenLifetimeMinutes = accessMinutes;
                            break;
                    }
                }
            }
            catch
            {
                // 使用默认值
            }

            _cachedPolicy = policy;
            _policyCacheTime = DateTime.UtcNow;

            return policy;
        }
    }
}
