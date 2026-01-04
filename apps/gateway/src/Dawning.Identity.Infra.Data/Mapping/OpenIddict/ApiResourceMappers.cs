using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// API resource entity and domain model mapper
    /// </summary>
    public static class ApiResourceMappers
    {
        private static IMapper Mapper { get; }

        static ApiResourceMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model with optional scopes and claims
        /// </summary>
        public static ApiResource ToModel(
            this ApiResourceEntity entity,
            IEnumerable<ApiResourceScopeEntity>? scopes = null,
            IEnumerable<ApiResourceClaimEntity>? claims = null
        )
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var model = Mapper.Map<ApiResource>(entity);
            model.Scopes = scopes?.Select(s => s.Scope).ToList() ?? new List<string>();
            model.UserClaims = claims?.Select(c => c.Type).ToList() ?? new List<string>();
            return model;
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static ApiResourceEntity ToEntity(this ApiResource model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return Mapper.Map<ApiResourceEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<ApiResource> ToModels(this IEnumerable<ApiResourceEntity> entities)
        {
            return entities?.Select(e => e.ToModel()) ?? Enumerable.Empty<ApiResource>();
        }
    }
}
