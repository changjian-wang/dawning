using System.Collections.Generic;
using AutoMapper;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

namespace Dawning.Identity.Infra.Data.Mapping.Monitoring;

/// <summary>
/// RequestLog entity and domain model mapper
/// </summary>
public static class RequestLogMapping
{
    private static IMapper Mapper { get; }

    static RequestLogMapping()
    {
        Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RequestLogProfile>()).CreateMapper();
    }

    /// <summary>
    /// Convert entity to domain model
    /// </summary>
    public static RequestLog ToModel(this RequestLogEntity entity)
    {
        return Mapper.Map<RequestLog>(entity);
    }

    /// <summary>
    /// Convert domain model to entity
    /// </summary>
    public static RequestLogEntity ToEntity(this RequestLog model)
    {
        return Mapper.Map<RequestLogEntity>(model);
    }

    /// <summary>
    /// Convert entity collection to domain model collection
    /// </summary>
    public static IEnumerable<RequestLog> ToModels(this IEnumerable<RequestLogEntity> entities)
    {
        return Mapper.Map<IEnumerable<RequestLog>>(entities);
    }
}
