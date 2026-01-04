using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Caching;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// System configuration service interface
    /// </summary>
    public interface ISystemConfigService
    {
        /// <summary>
        /// Get configuration value
        /// </summary>
        Task<string?> GetValueAsync(string group, string key);

        /// <summary>
        /// Get configuration value with default value
        /// </summary>
        Task<T> GetValueAsync<T>(string group, string key, T defaultValue);

        /// <summary>
        /// Set configuration value
        /// </summary>
        Task<bool> SetValueAsync(
            string group,
            string key,
            string value,
            string? description = null
        );

        /// <summary>
        /// Get all configurations under a group
        /// </summary>
        Task<IEnumerable<SystemConfigItemDto>> GetByGroupAsync(string group);

        /// <summary>
        /// Get all configuration groups
        /// </summary>
        Task<IEnumerable<string>> GetGroupsAsync();

        /// <summary>
        /// Batch update configurations
        /// </summary>
        Task<bool> BatchUpdateAsync(IEnumerable<SystemConfigItemDto> configs);

        /// <summary>
        /// Delete configuration
        /// </summary>
        Task<bool> DeleteAsync(string group, string key);

        /// <summary>
        /// Get configuration update timestamp (for hot reload detection)
        /// </summary>
        Task<long> GetLastUpdateTimestampAsync();
    }

    /// <summary>
    /// System configuration item DTO
    /// </summary>
    public class SystemConfigItemDto
    {
        public Guid Id { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Description { get; set; }
        public string? ValueType { get; set; } = "string"; // string, number, boolean, json
        public bool IsReadonly { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// System configuration service implementation
    /// </summary>
    public class SystemConfigService : ISystemConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService? _cacheService;

        public SystemConfigService(IUnitOfWork unitOfWork, ICacheService? cacheService = null)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<string?> GetValueAsync(string group, string key)
        {
            // Use cache (if available)
            if (_cacheService != null)
            {
                var cacheKey = CacheKeys.SystemConfig(group, key);
                return await _cacheService.GetOrSetWithNullProtectionAsync<string>(
                    cacheKey,
                    async _ =>
                    {
                        var model = new SystemConfigModel { Name = group, Key = key };
                        var result = await _unitOfWork.SystemConfig.GetPagedListAsync(model, 1, 1);
                        return result.Items.FirstOrDefault()?.Value;
                    },
                    CacheEntryOptions.Medium, // 15 minutes cache
                    TimeSpan.FromMinutes(2) // Null value cache 2 minutes
                );
            }

            // Query directly without cache
            var queryModel = new SystemConfigModel { Name = group, Key = key };
            var queryResult = await _unitOfWork.SystemConfig.GetPagedListAsync(queryModel, 1, 1);
            return queryResult.Items.FirstOrDefault()?.Value;
        }

        public async Task<T> GetValueAsync<T>(string group, string key, T defaultValue)
        {
            var value = await GetValueAsync(group, key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            try
            {
                var type = typeof(T);
                if (type == typeof(string))
                    return (T)(object)value;
                if (type == typeof(int))
                    return (T)(object)int.Parse(value);
                if (type == typeof(bool))
                    return (T)(object)bool.Parse(value);
                if (type == typeof(double))
                    return (T)(object)double.Parse(value);
                if (type == typeof(long))
                    return (T)(object)long.Parse(value);

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task<bool> SetValueAsync(
            string group,
            string key,
            string value,
            string? description = null
        )
        {
            var model = new SystemConfigModel { Name = group, Key = key };
            var result = await _unitOfWork.SystemConfig.GetPagedListAsync(model, 1, 1);
            var existing = result.Items.FirstOrDefault();

            if (existing != null)
            {
                existing.Value = value;
                existing.Updated = DateTime.UtcNow;
                existing.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (description != null)
                    existing.Description = description;

                var success = await _unitOfWork.SystemConfig.UpdateAsync(existing);

                // Invalidate cache after update
                if (success && _cacheService != null)
                {
                    await _cacheService.RemoveAsync(CacheKeys.SystemConfig(group, key));
                }

                return success;
            }
            else
            {
                var newItem = new SystemConfigAggregate
                {
                    Id = Guid.NewGuid(),
                    Name = group,
                    Key = key,
                    Value = value,
                    Description = description,
                    NonEditable = false,
                    Created = DateTime.UtcNow,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                };

                var success = await _unitOfWork.SystemConfig.InsertAsync(newItem) > 0;

                // Invalidate cache after insert
                if (success && _cacheService != null)
                {
                    await _cacheService.RemoveAsync(CacheKeys.SystemConfig(group, key));
                }

                return success;
            }
        }

        public async Task<IEnumerable<SystemConfigItemDto>> GetByGroupAsync(string group)
        {
            var model = new SystemConfigModel { Name = group };
            var result = await _unitOfWork.SystemConfig.GetPagedListAsync(model, 1, 1000);

            return result.Items.Select(x => new SystemConfigItemDto
            {
                Id = x.Id,
                Group = x.Name ?? string.Empty,
                Key = x.Key ?? string.Empty,
                Value = x.Value,
                Description = x.Description,
                IsReadonly = x.NonEditable,
                UpdatedAt = x.Updated,
            });
        }

        public async Task<IEnumerable<string>> GetGroupsAsync()
        {
            var result = await _unitOfWork.SystemConfig.GetPagedListAsync(
                new SystemConfigModel(),
                1,
                10000
            );
            return result
                .Items.Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name!)
                .Distinct()
                .OrderBy(x => x);
        }

        public async Task<bool> BatchUpdateAsync(IEnumerable<SystemConfigItemDto> configs)
        {
            foreach (var config in configs)
            {
                await SetValueAsync(
                    config.Group,
                    config.Key,
                    config.Value ?? string.Empty,
                    config.Description
                );
            }
            return true;
        }

        public async Task<bool> DeleteAsync(string group, string key)
        {
            var model = new SystemConfigModel { Name = group, Key = key };
            var result = await _unitOfWork.SystemConfig.GetPagedListAsync(model, 1, 1);
            var existing = result.Items.FirstOrDefault();

            if (existing != null)
            {
                var success = await _unitOfWork.SystemConfig.DeleteAsync(
                    new SystemConfigAggregate { Id = existing.Id }
                );

                // Invalidate cache after delete
                if (success && _cacheService != null)
                {
                    await _cacheService.RemoveAsync(CacheKeys.SystemConfig(group, key));
                }

                return success;
            }

            return false;
        }

        public async Task<long> GetLastUpdateTimestampAsync()
        {
            var result = await _unitOfWork.SystemConfig.GetPagedListAsync(
                new SystemConfigModel(),
                1,
                1
            );
            return result.Items.FirstOrDefault()?.Timestamp ?? 0;
        }
    }
}
