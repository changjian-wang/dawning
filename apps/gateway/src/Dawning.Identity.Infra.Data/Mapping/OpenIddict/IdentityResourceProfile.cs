using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    /// <summary>
    /// IdentityResource entity and domain model mapping configuration
    /// </summary>
    public class IdentityResourceProfile : Profile
    {
        public IdentityResourceProfile()
        {
            // Entity -> Domain Model (basic properties only, claims handled separately)
            CreateMap<IdentityResourceEntity, IdentityResource>()
                .ForMember(dest => dest.Properties, 
                    opt => opt.MapFrom(src => ParseJsonObject(src.Properties)))
                .ForMember(dest => dest.UserClaims, opt => opt.Ignore());

            // Domain Model -> Entity
            CreateMap<IdentityResource, IdentityResourceEntity>()
                .ForMember(dest => dest.Properties, 
                    opt => opt.MapFrom(src => SerializeJsonObject(src.Properties)))
                .ForMember(dest => dest.Timestamp, 
                    opt => opt.MapFrom(_ => DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
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
