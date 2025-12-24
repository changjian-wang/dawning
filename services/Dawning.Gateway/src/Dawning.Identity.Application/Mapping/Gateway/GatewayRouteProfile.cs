using AutoMapper;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Aggregates.Gateway;

namespace Dawning.Identity.Application.Mapping.Gateway
{
    /// <summary>
    /// 网关路由 AutoMapper 配置
    /// </summary>
    public class GatewayRouteProfile : Profile
    {
        public GatewayRouteProfile()
        {
            // GatewayRoute <-> GatewayRouteDto
            CreateMap<GatewayRoute, GatewayRouteDto>();
            CreateMap<GatewayRouteDto, GatewayRoute>();

            // CreateGatewayRouteDto -> GatewayRoute
            CreateMap<CreateGatewayRouteDto, GatewayRoute>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            // UpdateGatewayRouteDto -> GatewayRoute
            CreateMap<UpdateGatewayRouteDto, GatewayRoute>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}
