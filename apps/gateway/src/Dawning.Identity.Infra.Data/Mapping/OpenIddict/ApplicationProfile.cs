using System;
using System.Text.Json;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<ApplicationEntity, Application>(MemberList.Destination)
                .ForMember(
                    dest => dest.Permissions,
                    opt => opt.MapFrom(src => DeserializeList(src.PermissionsJson))
                )
                .ForMember(
                    dest => dest.RedirectUris,
                    opt => opt.MapFrom(src => DeserializeList(src.RedirectUrisJson))
                )
                .ForMember(
                    dest => dest.PostLogoutRedirectUris,
                    opt => opt.MapFrom(src => DeserializeList(src.PostLogoutRedirectUrisJson))
                )
                .ForMember(
                    dest => dest.Requirements,
                    opt => opt.MapFrom(src => DeserializeList(src.RequirementsJson))
                )
                .ForMember(
                    dest => dest.Properties,
                    opt => opt.MapFrom(src => DeserializeDictionary(src.PropertiesJson))
                )
                .ReverseMap()
                .ForMember(
                    dest => dest.PermissionsJson,
                    opt => opt.MapFrom(src => SerializeList(src.Permissions))
                )
                .ForMember(
                    dest => dest.RedirectUrisJson,
                    opt => opt.MapFrom(src => SerializeList(src.RedirectUris))
                )
                .ForMember(
                    dest => dest.PostLogoutRedirectUrisJson,
                    opt => opt.MapFrom(src => SerializeList(src.PostLogoutRedirectUris))
                )
                .ForMember(
                    dest => dest.RequirementsJson,
                    opt => opt.MapFrom(src => SerializeList(src.Requirements))
                )
                .ForMember(
                    dest => dest.PropertiesJson,
                    opt => opt.MapFrom(src => SerializeDictionary(src.Properties))
                );
        }

        /// <summary>
        /// Deserialize JSON to List
        /// </summary>
        private static List<string> DeserializeList(string? json)
        {
            if (string.IsNullOrEmpty(json))
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

        /// <summary>
        /// Deserialize JSON to Dictionary
        /// </summary>
        private static Dictionary<string, string> DeserializeDictionary(string? json)
        {
            if (string.IsNullOrEmpty(json))
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

        /// <summary>
        /// Serialize List to JSON
        /// </summary>
        private static string? SerializeList(List<string>? list)
        {
            if (list == null || list.Count == 0)
                return null;

            return JsonSerializer.Serialize(list);
        }

        /// <summary>
        /// Serialize Dictionary to JSON
        /// </summary>
        private static string? SerializeDictionary(Dictionary<string, string>? dict)
        {
            if (dict == null || dict.Count == 0)
                return null;

            return JsonSerializer.Serialize(dict);
        }
    }
}
