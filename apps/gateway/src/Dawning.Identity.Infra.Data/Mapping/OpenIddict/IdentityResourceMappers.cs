using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// Identity resource entity and domain model mapper
    /// </summary>
    public static class IdentityResourceMappers
    {
        private static IMapper Mapper { get; }

        static IdentityResourceMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceProfile>()).CreateMapper();
        }

        /// <summary>
        /// Convert entity to domain model with optional claims
        /// </summary>
        public static IdentityResource ToModel(
            this IdentityResourceEntity entity,
            IEnumerable<IdentityResourceClaimEntity>? claims = null
        )
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var model = Mapper.Map<IdentityResource>(entity);
            model.UserClaims = claims?.Select(c => c.Type).ToList() ?? new List<string>();
            return model;
        }

        /// <summary>
        /// Convert domain model to entity
        /// </summary>
        public static IdentityResourceEntity ToEntity(this IdentityResource model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return Mapper.Map<IdentityResourceEntity>(model);
        }

        /// <summary>
        /// Convert entity collection to domain model collection
        /// </summary>
        public static IEnumerable<IdentityResource> ToModels(this IEnumerable<IdentityResourceEntity> entities)
        {
            return entities?.Select(e => e.ToModel()) ?? Enumerable.Empty<IdentityResource>();
        }
    }
}
