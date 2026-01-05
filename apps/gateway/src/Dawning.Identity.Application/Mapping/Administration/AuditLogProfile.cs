using System.Text.Json;
using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;
using Dawning.Identity.Domain.Constants;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// Audit log mapping profile
    /// </summary>
    public class AuditLogProfile : Profile
    {
        public AuditLogProfile()
        {
            // AuditLog -> AuditLogDto
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(
                    dest => dest.OldValues,
                    opt => opt.MapFrom(src => DeserializeJson(src.OldValues))
                )
                .ForMember(
                    dest => dest.NewValues,
                    opt => opt.MapFrom(src => DeserializeJson(src.NewValues))
                );

            // CreateAuditLogDto -> AuditLog
            CreateMap<CreateAuditLogDto, AuditLog>()
                .ForMember(
                    dest => dest.OldValues,
                    opt => opt.MapFrom(src => SerializeJson(src.OldValues))
                )
                .ForMember(
                    dest => dest.NewValues,
                    opt => opt.MapFrom(src => SerializeJson(src.NewValues))
                );
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
                return json; // If deserialization fails, return the original string
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

    /// <summary>
    /// AuditLog mappers using AutoMapper
    /// </summary>
    public static class AuditLogMappers
    {
        private static IMapper Mapper { get; }

        static AuditLogMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
                cfg.AddProfile<AuditLogProfile>()
            ).CreateMapper();
        }

        /// <summary>
        /// Convert AuditLog to AuditLogDto
        /// </summary>
        public static AuditLogDto ToDto(this AuditLog model)
        {
            return Mapper.Map<AuditLogDto>(model);
        }

        /// <summary>
        /// Convert AuditLog collection to AuditLogDto collection
        /// </summary>
        public static IEnumerable<AuditLogDto> ToDtos(this IEnumerable<AuditLog> models)
        {
            return Mapper.Map<IEnumerable<AuditLogDto>>(models);
        }

        /// <summary>
        /// Convert CreateAuditLogDto to AuditLog
        /// </summary>
        public static AuditLog ToEntity(this CreateAuditLogDto dto)
        {
            var entity = Mapper.Map<AuditLog>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        /// <summary>
        /// Create an audit log for entity creation
        /// </summary>
        public static AuditLog CreateAudit(
            string entityType,
            Guid entityId,
            string description,
            Guid? operatorId = null
        )
        {
            return new CreateAuditLogDto
            {
                Action = AuditAction.Create,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                StatusCode = 200,
                UserId = operatorId,
            }.ToEntity();
        }

        /// <summary>
        /// Create an audit log for entity update
        /// </summary>
        public static AuditLog UpdateAudit(
            string entityType,
            Guid entityId,
            string description,
            Guid? operatorId = null,
            object? oldValues = null,
            object? newValues = null
        )
        {
            return new CreateAuditLogDto
            {
                Action = AuditAction.Update,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues,
                StatusCode = 200,
                UserId = operatorId,
            }.ToEntity();
        }

        /// <summary>
        /// Create an audit log for entity deletion
        /// </summary>
        public static AuditLog DeleteAudit(
            string entityType,
            Guid entityId,
            string description,
            Guid? operatorId = null
        )
        {
            return new CreateAuditLogDto
            {
                Action = AuditAction.Delete,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                StatusCode = 200,
                UserId = operatorId,
            }.ToEntity();
        }

        /// <summary>
        /// Create a custom audit log
        /// </summary>
        public static AuditLog CustomAudit(
            string action,
            string entityType,
            Guid entityId,
            string description,
            Guid? operatorId = null
        )
        {
            return new CreateAuditLogDto
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                StatusCode = 200,
                UserId = operatorId,
            }.ToEntity();
        }
    }
}
