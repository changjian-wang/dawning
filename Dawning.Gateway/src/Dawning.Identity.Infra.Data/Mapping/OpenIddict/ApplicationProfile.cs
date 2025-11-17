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
            var listConverter = new JsonListConverter();
            var dictConverter = new JsonDictionaryConverter();

            CreateMap<ApplicationEntity, Application>(MemberList.Destination)
            .ForMember(dest => dest.Permissions,
                opt => opt.ConvertUsing(listConverter, src => src.PermissionsJson))
            .ForMember(dest => dest.RedirectUris,
                opt => opt.ConvertUsing(listConverter, src => src.RedirectUrisJson))
            .ForMember(dest => dest.PostLogoutRedirectUris,
                opt => opt.ConvertUsing(listConverter, src => src.PostLogoutRedirectUrisJson))
            .ForMember(dest => dest.Requirements,
                opt => opt.ConvertUsing(listConverter, src => src.RequirementsJson))
            .ForMember(dest => dest.Properties,
                opt => opt.ConvertUsing(dictConverter, src => src.PropertiesJson))
            .ReverseMap()
            .ForMember(dest => dest.PermissionsJson,
                opt => opt.MapFrom(src =>
                    src.Permissions != null && src.Permissions.Count > 0
                        ? JsonSerializer.Serialize(src.Permissions) : null))
            .ForMember(dest => dest.RedirectUrisJson,
                opt => opt.MapFrom(src =>
                    src.RedirectUris != null && src.RedirectUris.Count > 0
                        ? JsonSerializer.Serialize(src.RedirectUris) : null))
            .ForMember(dest => dest.PostLogoutRedirectUrisJson,
                opt => opt.MapFrom(src =>
                    src.PostLogoutRedirectUris != null && src.PostLogoutRedirectUris.Count > 0
                        ? JsonSerializer.Serialize(src.PostLogoutRedirectUris) : null))
            .ForMember(dest => dest.RequirementsJson,
                opt => opt.MapFrom(src =>
                    src.Requirements != null && src.Requirements.Count > 0
                        ? JsonSerializer.Serialize(src.Requirements) : null))
            .ForMember(dest => dest.PropertiesJson,
                opt => opt.MapFrom(src =>
                    src.Properties != null && src.Properties.Count > 0
                        ? JsonSerializer.Serialize(src.Properties) : null));
        }
    }

    /// <summary>
    /// JSON List 转换器
    /// </summary>
    public class JsonListConverter : IValueConverter<string?, List<string>>
    {
        public List<string> Convert(string? sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(sourceMember) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }

    /// <summary>
    /// JSON Dictionary 转换器
    /// </summary>
    public class JsonDictionaryConverter : IValueConverter<string?, Dictionary<string, string>>
    {
        public Dictionary<string, string> Convert(string? sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return new Dictionary<string, string>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(sourceMember)
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }
}

