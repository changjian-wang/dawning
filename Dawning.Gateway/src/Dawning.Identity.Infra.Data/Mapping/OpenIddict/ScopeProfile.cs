using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Infra.Data.PersistentObjects.OpenIddict;

namespace Dawning.Identity.Infra.Data.Mapping.OpenIddict
{
    public class ScopeProfile : Profile
    {
        public ScopeProfile()
        {
            CreateMap<ScopeEntity, Scope>(MemberList.Destination)
                .ForMember(
                    dest => dest.Resources,
                    opt => opt.MapFrom(src => DeserializeList(src.ResourcesJson))
                )
                .ForMember(
                    dest => dest.Properties,
                    opt => opt.MapFrom(src => DeserializeDictionary(src.PropertiesJson))
                )
                .ReverseMap()
                .ForMember(
                    dest => dest.ResourcesJson,
                    opt => opt.MapFrom(src => SerializeList(src.Resources))
                )
                .ForMember(
                    dest => dest.PropertiesJson,
                    opt => opt.MapFrom(src => SerializeDictionary(src.Properties))
                );
        }

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

        private static string? SerializeList(List<string>? list)
        {
            if (list == null || list.Count == 0)
                return null;

            return JsonSerializer.Serialize(list);
        }

        private static string? SerializeDictionary(Dictionary<string, string>? dict)
        {
            if (dict == null || dict.Count == 0)
                return null;

            return JsonSerializer.Serialize(dict);
        }
    }
}
