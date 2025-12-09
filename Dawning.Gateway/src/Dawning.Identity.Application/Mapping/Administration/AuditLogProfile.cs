using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using System.Text.Json;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// 审计日志映射Profile
    /// </summary>
    public class AuditLogProfile : Profile
    {
        public AuditLogProfile()
        {
            // AuditLog -> AuditLogDto
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.OldValues, opt => opt.MapFrom(src =>
                    DeserializeJson(src.OldValues)))
                .ForMember(dest => dest.NewValues, opt => opt.MapFrom(src =>
                    DeserializeJson(src.NewValues)));

            // CreateAuditLogDto -> AuditLog
            CreateMap<CreateAuditLogDto, AuditLog>()
                .ForMember(dest => dest.OldValues, opt => opt.MapFrom(src =>
                    SerializeJson(src.OldValues)))
                .ForMember(dest => dest.NewValues, opt => opt.MapFrom(src =>
                    SerializeJson(src.NewValues)));
        }

        private static object? DeserializeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                return json; // 如果反序列化失败，返回原始字符串
            }
        }

        private static string? SerializeJson(object? obj)
        {
            if (obj == null)
                return null;

            if (obj is string str)
                return str;

            return JsonSerializer.Serialize(obj);
        }
    }
}
