using AutoMapper;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Aggregates.Gateway;

namespace Dawning.Identity.Application.Mapping.Gateway
{
    /// <summary>
    /// 网关集群 AutoMapper 配置
    /// </summary>
    public class GatewayClusterProfile : Profile
    {
        public GatewayClusterProfile()
        {
            // GatewayCluster <-> GatewayClusterDto
            CreateMap<GatewayCluster, GatewayClusterDto>();
            CreateMap<GatewayClusterDto, GatewayCluster>();

            // CreateGatewayClusterDto -> GatewayCluster
            CreateMap<CreateGatewayClusterDto, GatewayCluster>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            // UpdateGatewayClusterDto -> GatewayCluster
            CreateMap<UpdateGatewayClusterDto, GatewayCluster>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}
