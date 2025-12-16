using Dapper;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Infra.Data.PersistentObjects.Gateway;
using MySql.Data.MySqlClient;

namespace Dawning.Identity.Api.Services
{
    public interface IRateLimitService
    {
        // Rate Limit Policies
        Task<IEnumerable<RateLimitPolicyDto>> GetAllPoliciesAsync();
        Task<RateLimitPolicyDto?> GetPolicyByIdAsync(Guid id);
        Task<RateLimitPolicyDto?> GetPolicyByNameAsync(string name);
        Task<Guid> CreatePolicyAsync(CreateRateLimitPolicyDto dto);
        Task<bool> UpdatePolicyAsync(UpdateRateLimitPolicyDto dto);
        Task<bool> DeletePolicyAsync(Guid id);

        // IP Access Rules
        Task<(IEnumerable<IpAccessRuleDto> Items, int Total)> GetIpRulesPagedAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        );
        Task<IpAccessRuleDto?> GetIpRuleByIdAsync(Guid id);
        Task<IEnumerable<IpAccessRuleDto>> GetActiveRulesByTypeAsync(string ruleType);
        Task<Guid> CreateIpRuleAsync(CreateIpAccessRuleDto dto, string? createdBy);
        Task<bool> UpdateIpRuleAsync(UpdateIpAccessRuleDto dto);
        Task<bool> DeleteIpRuleAsync(Guid id);
        Task<bool> IsIpBlacklistedAsync(string ipAddress);
        Task<bool> IsIpWhitelistedAsync(string ipAddress);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly string _connectionString;

        public RateLimitService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        #region Rate Limit Policies

        public async Task<IEnumerable<RateLimitPolicyDto>> GetAllPoliciesAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            var entities = await connection.QueryAsync<RateLimitPolicyEntity>(
                "SELECT * FROM rate_limit_policies ORDER BY created_at DESC"
            );

            return entities.Select(MapToDto);
        }

        public async Task<RateLimitPolicyDto?> GetPolicyByIdAsync(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var entity = await connection.QueryFirstOrDefaultAsync<RateLimitPolicyEntity>(
                "SELECT * FROM rate_limit_policies WHERE id = @Id",
                new { Id = id.ToString() }
            );

            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<RateLimitPolicyDto?> GetPolicyByNameAsync(string name)
        {
            using var connection = new MySqlConnection(_connectionString);
            var entity = await connection.QueryFirstOrDefaultAsync<RateLimitPolicyEntity>(
                "SELECT * FROM rate_limit_policies WHERE name = @Name",
                new { Name = name }
            );

            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<Guid> CreatePolicyAsync(CreateRateLimitPolicyDto dto)
        {
            var id = Guid.NewGuid();
            using var connection = new MySqlConnection(_connectionString);

            await connection.ExecuteAsync(
                @"INSERT INTO rate_limit_policies 
                (id, name, display_name, policy_type, permit_limit, window_seconds, 
                 segments_per_window, queue_limit, tokens_per_period, 
                 replenishment_period_seconds, is_enabled, description, created_at)
                VALUES 
                (@Id, @Name, @DisplayName, @PolicyType, @PermitLimit, @WindowSeconds,
                 @SegmentsPerWindow, @QueueLimit, @TokensPerPeriod,
                 @ReplenishmentPeriodSeconds, @IsEnabled, @Description, @CreatedAt)",
                new
                {
                    Id = id.ToString(),
                    dto.Name,
                    dto.DisplayName,
                    dto.PolicyType,
                    dto.PermitLimit,
                    dto.WindowSeconds,
                    dto.SegmentsPerWindow,
                    dto.QueueLimit,
                    dto.TokensPerPeriod,
                    dto.ReplenishmentPeriodSeconds,
                    dto.IsEnabled,
                    dto.Description,
                    CreatedAt = DateTime.UtcNow,
                }
            );

            return id;
        }

        public async Task<bool> UpdatePolicyAsync(UpdateRateLimitPolicyDto dto)
        {
            using var connection = new MySqlConnection(_connectionString);

            var affected = await connection.ExecuteAsync(
                @"UPDATE rate_limit_policies SET
                name = @Name, display_name = @DisplayName, policy_type = @PolicyType,
                permit_limit = @PermitLimit, window_seconds = @WindowSeconds,
                segments_per_window = @SegmentsPerWindow, queue_limit = @QueueLimit,
                tokens_per_period = @TokensPerPeriod, 
                replenishment_period_seconds = @ReplenishmentPeriodSeconds,
                is_enabled = @IsEnabled, description = @Description, updated_at = @UpdatedAt
                WHERE id = @Id",
                new
                {
                    Id = dto.Id.ToString(),
                    dto.Name,
                    dto.DisplayName,
                    dto.PolicyType,
                    dto.PermitLimit,
                    dto.WindowSeconds,
                    dto.SegmentsPerWindow,
                    dto.QueueLimit,
                    dto.TokensPerPeriod,
                    dto.ReplenishmentPeriodSeconds,
                    dto.IsEnabled,
                    dto.Description,
                    UpdatedAt = DateTime.UtcNow,
                }
            );

            return affected > 0;
        }

        public async Task<bool> DeletePolicyAsync(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var affected = await connection.ExecuteAsync(
                "DELETE FROM rate_limit_policies WHERE id = @Id",
                new { Id = id.ToString() }
            );
            return affected > 0;
        }

        #endregion

        #region IP Access Rules

        public async Task<(IEnumerable<IpAccessRuleDto> Items, int Total)> GetIpRulesPagedAsync(
            string? ruleType,
            bool? isEnabled,
            int page,
            int pageSize
        )
        {
            using var connection = new MySqlConnection(_connectionString);

            var whereClause = "WHERE 1=1";
            if (!string.IsNullOrEmpty(ruleType))
                whereClause += " AND rule_type = @RuleType";
            if (isEnabled.HasValue)
                whereClause += " AND is_enabled = @IsEnabled";

            var countSql = $"SELECT COUNT(*) FROM ip_access_rules {whereClause}";
            var total = await connection.ExecuteScalarAsync<int>(
                countSql,
                new { RuleType = ruleType, IsEnabled = isEnabled }
            );

            var sql =
                $@"SELECT * FROM ip_access_rules {whereClause} 
                   ORDER BY created_at DESC 
                   LIMIT @PageSize OFFSET @Offset";

            var entities = await connection.QueryAsync<IpAccessRuleEntity>(
                sql,
                new
                {
                    RuleType = ruleType,
                    IsEnabled = isEnabled,
                    PageSize = pageSize,
                    Offset = (page - 1) * pageSize,
                }
            );

            return (entities.Select(MapToDto), total);
        }

        public async Task<IpAccessRuleDto?> GetIpRuleByIdAsync(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var entity = await connection.QueryFirstOrDefaultAsync<IpAccessRuleEntity>(
                "SELECT * FROM ip_access_rules WHERE id = @Id",
                new { Id = id.ToString() }
            );

            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<IEnumerable<IpAccessRuleDto>> GetActiveRulesByTypeAsync(string ruleType)
        {
            using var connection = new MySqlConnection(_connectionString);
            var entities = await connection.QueryAsync<IpAccessRuleEntity>(
                @"SELECT * FROM ip_access_rules 
                  WHERE rule_type = @RuleType 
                  AND is_enabled = 1 
                  AND (expires_at IS NULL OR expires_at > @Now)",
                new { RuleType = ruleType, Now = DateTime.UtcNow }
            );

            return entities.Select(MapToDto);
        }

        public async Task<Guid> CreateIpRuleAsync(CreateIpAccessRuleDto dto, string? createdBy)
        {
            var id = Guid.NewGuid();
            using var connection = new MySqlConnection(_connectionString);

            await connection.ExecuteAsync(
                @"INSERT INTO ip_access_rules 
                (id, ip_address, rule_type, description, is_enabled, expires_at, created_at, created_by)
                VALUES 
                (@Id, @IpAddress, @RuleType, @Description, @IsEnabled, @ExpiresAt, @CreatedAt, @CreatedBy)",
                new
                {
                    Id = id.ToString(),
                    dto.IpAddress,
                    dto.RuleType,
                    dto.Description,
                    dto.IsEnabled,
                    dto.ExpiresAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy,
                }
            );

            return id;
        }

        public async Task<bool> UpdateIpRuleAsync(UpdateIpAccessRuleDto dto)
        {
            using var connection = new MySqlConnection(_connectionString);

            var affected = await connection.ExecuteAsync(
                @"UPDATE ip_access_rules SET
                ip_address = @IpAddress, rule_type = @RuleType,
                description = @Description, is_enabled = @IsEnabled,
                expires_at = @ExpiresAt, updated_at = @UpdatedAt
                WHERE id = @Id",
                new
                {
                    Id = dto.Id.ToString(),
                    dto.IpAddress,
                    dto.RuleType,
                    dto.Description,
                    dto.IsEnabled,
                    dto.ExpiresAt,
                    UpdatedAt = DateTime.UtcNow,
                }
            );

            return affected > 0;
        }

        public async Task<bool> DeleteIpRuleAsync(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var affected = await connection.ExecuteAsync(
                "DELETE FROM ip_access_rules WHERE id = @Id",
                new { Id = id.ToString() }
            );
            return affected > 0;
        }

        public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            using var connection = new MySqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM ip_access_rules 
                  WHERE rule_type = 'blacklist' 
                  AND is_enabled = 1 
                  AND (expires_at IS NULL OR expires_at > @Now)
                  AND (ip_address = @IpAddress OR @IpAddress LIKE CONCAT(REPLACE(ip_address, '*', '%')))",
                new { IpAddress = ipAddress, Now = DateTime.UtcNow }
            );
            return count > 0;
        }

        public async Task<bool> IsIpWhitelistedAsync(string ipAddress)
        {
            using var connection = new MySqlConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM ip_access_rules 
                  WHERE rule_type = 'whitelist' 
                  AND is_enabled = 1 
                  AND (expires_at IS NULL OR expires_at > @Now)
                  AND (ip_address = @IpAddress OR @IpAddress LIKE CONCAT(REPLACE(ip_address, '*', '%')))",
                new { IpAddress = ipAddress, Now = DateTime.UtcNow }
            );
            return count > 0;
        }

        #endregion

        #region Mappers

        private static RateLimitPolicyDto MapToDto(RateLimitPolicyEntity entity)
        {
            return new RateLimitPolicyDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                PolicyType = entity.PolicyType,
                PermitLimit = entity.PermitLimit,
                WindowSeconds = entity.WindowSeconds,
                SegmentsPerWindow = entity.SegmentsPerWindow,
                QueueLimit = entity.QueueLimit,
                TokensPerPeriod = entity.TokensPerPeriod,
                ReplenishmentPeriodSeconds = entity.ReplenishmentPeriodSeconds,
                IsEnabled = entity.IsEnabled,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        private static IpAccessRuleDto MapToDto(IpAccessRuleEntity entity)
        {
            return new IpAccessRuleDto
            {
                Id = entity.Id,
                IpAddress = entity.IpAddress,
                RuleType = entity.RuleType,
                Description = entity.Description,
                IsEnabled = entity.IsEnabled,
                ExpiresAt = entity.ExpiresAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedBy = entity.CreatedBy,
            };
        }

        #endregion
    }
}
