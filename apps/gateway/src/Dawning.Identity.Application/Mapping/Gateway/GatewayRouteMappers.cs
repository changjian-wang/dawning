using AutoMapper;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Aggregates.Gateway;

namespace Dawning.Identity.Application.Mapping.Gateway;

/// <summary>
/// Gateway route mapper extensions using AutoMapper
/// </summary>
public static class GatewayRouteMappers
{
    private static IMapper Mapper { get; }

    static GatewayRouteMappers()
    {
        Mapper = new MapperConfiguration(cfg => cfg.AddProfile<GatewayRouteProfile>()).CreateMapper();
    }

    /// <summary>
    /// Convert GatewayRoute to GatewayRouteDto
    /// </summary>
    public static GatewayRouteDto ToDto(this GatewayRoute model) =>
        Mapper.Map<GatewayRouteDto>(model);

    /// <summary>
    /// Convert GatewayRoute to GatewayRouteDto (nullable)
    /// </summary>
    public static GatewayRouteDto? ToDtoOrNull(this GatewayRoute? model) =>
        model != null ? Mapper.Map<GatewayRouteDto>(model) : null;

    /// <summary>
    /// Convert GatewayRoute collection to GatewayRouteDto collection
    /// </summary>
    public static IEnumerable<GatewayRouteDto> ToDtos(this IEnumerable<GatewayRoute> models) =>
        Mapper.Map<IEnumerable<GatewayRouteDto>>(models);

    /// <summary>
    /// Convert CreateGatewayRouteDto to GatewayRoute entity
    /// </summary>
    public static GatewayRoute ToEntity(this CreateGatewayRouteDto dto)
    {
        var entity = Mapper.Map<GatewayRoute>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        return entity;
    }

    /// <summary>
    /// Apply UpdateGatewayRouteDto to existing GatewayRoute entity
    /// </summary>
    public static void ApplyUpdate(this GatewayRoute entity, UpdateGatewayRouteDto dto)
    {
        Mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
