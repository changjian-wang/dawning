using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// API资源映射器
    /// </summary>
    public static class ApiResourceMappers
    {
        /// <summary>
        /// Entity转Model
        /// </summary>
        public static ApiResource ToModel(
            this ApiResourceEntity entity,
            IEnumerable<ApiResourceScopeEntity>? scopes = null,
            IEnumerable<ApiResourceClaimEntity>? claims = null
        )
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new ApiResource
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                Enabled = entity.Enabled,
                AllowedAccessTokenSigningAlgorithms = ParseJsonArray(
                    entity.AllowedAccessTokenSigningAlgorithms
                ),
                ShowInDiscoveryDocument = entity.ShowInDiscoveryDocument,
                Scopes = scopes?.Select(s => s.Scope).ToList() ?? new List<string>(),
                UserClaims = claims?.Select(c => c.Type).ToList() ?? new List<string>(),
                Properties = ParseJsonObject(entity.Properties),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        /// <summary>
        /// Model转Entity
        /// </summary>
        public static ApiResourceEntity ToEntity(this ApiResource model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new ApiResourceEntity
            {
                Id = model.Id,
                Name = model.Name,
                DisplayName = model.DisplayName,
                Description = model.Description,
                Enabled = model.Enabled,
                AllowedAccessTokenSigningAlgorithms = SerializeJsonArray(
                    model.AllowedAccessTokenSigningAlgorithms
                ),
                ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                Properties = SerializeJsonObject(model.Properties),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
            };
        }

        /// <summary>
        /// 批量Entity转Model
        /// </summary>
        public static IEnumerable<ApiResource> ToModels(
            this IEnumerable<ApiResourceEntity> entities
        )
        {
            return entities?.Select(e => e.ToModel()) ?? Enumerable.Empty<ApiResource>();
        }

        private static List<string> ParseJsonArray(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static Dictionary<string, string> ParseJsonObject(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private static string? SerializeJsonArray(List<string>? list)
        {
            if (list == null || !list.Any())
                return null;
            return JsonSerializer.Serialize(list);
        }

        private static string? SerializeJsonObject(Dictionary<string, string>? dict)
        {
            if (dict == null || !dict.Any())
                return null;
            return JsonSerializer.Serialize(dict);
        }
    }
}
