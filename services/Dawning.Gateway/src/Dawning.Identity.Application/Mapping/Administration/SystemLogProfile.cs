using AutoMapper;
using Dawning.Identity.Application.Dtos.Administration;
using Dawning.Identity.Domain.Aggregates.Administration;

namespace Dawning.Identity.Application.Mapping.Administration
{
    /// <summary>
    /// 系统日志AutoMapper配置
    /// </summary>
    public class SystemLogProfile : Profile
    {
        public SystemLogProfile()
        {
            // SystemLog <-> SystemLogDto
            CreateMap<SystemLog, SystemLogDto>();
            CreateMap<SystemLogDto, SystemLog>();

            // CreateSystemLogDto -> SystemLog
            CreateMap<CreateSystemLogDto, SystemLog>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
