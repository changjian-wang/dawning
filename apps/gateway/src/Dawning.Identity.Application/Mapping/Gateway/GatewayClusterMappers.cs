using AutoMapper;
using Dawning.Identity.Application.Dtos.Gateway;
using Dawning.Identity.Domain.Aggregates.Gateway;

namespace Dawning.Identity.Application.Mapping.Gateway;

/// <summary>
/// Gateway cluster mapper extensions using AutoMapper
/// </summary>
public static class GatewayClusterMappers
{
    private static IMapper Mapper { get; }

    static GatewayClusterMappers()
    {
        Mapper = new MapperConfiguration(cfg => cfg.AddProfile<GatewayClusterProfile>()).CreateMapper();
    }

    /// <summary>
    /// Convert GatewayCluster to GatewayClusterDto
    /// </summary>
    public static GatewayClusterDto ToDto(this GatewayCluster model) =>
        Mapper.Map<GatewayClusterDto>(model);

    /// <summary>
    /// Convert GatewayCluster to GatewayClusterDto (nullable)
    /// </summary>
    public static GatewayClusterDto? ToDtoOrNull(this GatewayCluster? model) =>
        model != null ? Mapper.Map<GatewayClusterDto>(model) : null;

    /// <summary>
    /// Convert GatewayCluster collection to GatewayClusterDto collection
    /// </summary>
    public static IEnumerable<GatewayClusterDto> ToDtos(this IEnumerable<GatewayCluster> models) =>
        Mapper.Map<IEnumerable<GatewayClusterDto>>(models);

    /// <summary>
    /// Convert CreateGatewayClusterDto to GatewayCluster entity
    /// </summary>
    public static GatewayCluster ToEntity(this CreateGatewayClusterDto dto)
    {
        var entity = Mapper.Map<GatewayCluster>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        return entity;
    }

    /// <summary>
    /// Apply UpdateGatewayClusterDto to existing GatewayCluster entity
    /// </summary>
    public static void ApplyUpdate(this GatewayCluster entity, UpdateGatewayClusterDto dto)
    {
        Mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
