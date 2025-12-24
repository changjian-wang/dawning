using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.Caching;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Interfaces.UoW;
using Dawning.Identity.Domain.Models.Administration;

namespace Dawning.Identity.Application.Services.Administration
{
    /// <summary>
    /// 系统配置服务接口
    /// </summary>
    public interface ISystemConfigService
    {
        /// <summary>
        /// 获取配置值
        /// </summary>
        Task<string?> GetValueAsync(string group, string key);

        /// <summary>
        /// 获取配置值，带默认值
        /// </summary>
        Task<T> GetValueAsync<T>(string group, string key, T defaultValue);

        /// <summary>
        /// 设置配置值
        /// </summary>
        Task<bool> SetValueAsync(
            string group,
            string key,
            string value,
            string? description = null
        );

        /// <summary>
        /// 获取分组下的所有配置
        /// </summary>
        Task<IEnumerable<SystemConfigItemDto>> GetByGroupAsync(string group);

        /// <summary>
        /// 获取所有配置分组
        /// </summary>
        Task<IEnumerable<string>> GetGroupsAsync();

        /// <summary>
        /// 批量更新配置
        /// </summary>
        Task<bool> BatchUpdateAsync(IEnumerable<SystemConfigItemDto> configs);

        /// <summary>
        /// 删除配置
        /// </summary>
        Task<bool> DeleteAsync(string group, string key);

        /// <summary>
        /// 获取配置更新时间戳（用于热更新检测）
        /// </summary>
        Task<long> GetLastUpdateTimestampAsync();
    }

    /// <summary>
    /// 系统配置项 DTO
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
    /// 系统配置服务实现
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
            // 使用缓存（如果可用）
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
                    CacheEntryOptions.Medium, // 15分钟缓存
                    TimeSpan.FromMinutes(2) // 空值缓存2分钟
                );
            }

            // 无缓存时直接查询
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

                // 更新后使缓存失效
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

                // 新增后使缓存失效
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

                // 删除后使缓存失效
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
