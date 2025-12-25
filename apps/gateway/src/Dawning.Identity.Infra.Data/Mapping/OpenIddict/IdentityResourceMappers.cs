using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// 身份资源映射器
    /// </summary>
    public static class IdentityResourceMappers
    {
        /// <summary>
        /// Entity转Model
        /// </summary>
        public static IdentityResource ToModel(
            this IdentityResourceEntity entity,
            IEnumerable<IdentityResourceClaimEntity>? claims = null
        )
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return new IdentityResource
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                Description = entity.Description,
                Enabled = entity.Enabled,
                Required = entity.Required,
                Emphasize = entity.Emphasize,
                ShowInDiscoveryDocument = entity.ShowInDiscoveryDocument,
                UserClaims = claims?.Select(c => c.Type).ToList() ?? new List<string>(),
                Properties = ParseJsonObject(entity.Properties),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        /// <summary>
        /// Model转Entity
        /// </summary>
        public static IdentityResourceEntity ToEntity(this IdentityResource model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new IdentityResourceEntity
            {
                Id = model.Id,
                Name = model.Name,
                DisplayName = model.DisplayName,
                Description = model.Description,
                Enabled = model.Enabled,
                Required = model.Required,
                Emphasize = model.Emphasize,
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
        public static IEnumerable<IdentityResource> ToModels(
            this IEnumerable<IdentityResourceEntity> entities
        )
        {
            return entities?.Select(e => e.ToModel()) ?? Enumerable.Empty<IdentityResource>();
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

        private static string? SerializeJsonObject(Dictionary<string, string>? dict)
        {
            if (dict == null || !dict.Any())
                return null;
            return JsonSerializer.Serialize(dict);
        }
    }
}
