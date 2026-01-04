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
    /// ApiResource entity and domain model mapping configuration
    /// </summary>
    public class ApiResourceProfile : Profile
    {
        public ApiResourceProfile()
        {
            // Entity -> Domain Model (basic properties only, scopes/claims handled separately)
            CreateMap<ApiResourceEntity, ApiResource>()
                .ForMember(dest => dest.AllowedAccessTokenSigningAlgorithms, 
                    opt => opt.MapFrom(src => ParseJsonArray(src.AllowedAccessTokenSigningAlgorithms)))
                .ForMember(dest => dest.Properties, 
                    opt => opt.MapFrom(src => ParseJsonObject(src.Properties)))
                .ForMember(dest => dest.Scopes, opt => opt.Ignore())
                .ForMember(dest => dest.UserClaims, opt => opt.Ignore());

            // Domain Model -> Entity
            CreateMap<ApiResource, ApiResourceEntity>()
                .ForMember(dest => dest.AllowedAccessTokenSigningAlgorithms, 
                    opt => opt.MapFrom(src => SerializeJsonArray(src.AllowedAccessTokenSigningAlgorithms)))
                .ForMember(dest => dest.Properties, 
                    opt => opt.MapFrom(src => SerializeJsonObject(src.Properties)))
                .ForMember(dest => dest.Timestamp, 
                    opt => opt.MapFrom(_ => DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
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
