using System.Collections.Generic;
using System.Linq;
using Dawning.Identity.Domain.Aggregates.Monitoring;
using Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

namespace Dawning.Identity.Infra.Data.Mapping.Monitoring;

/// <summary>
/// 请求日志映射扩展
/// </summary>
public static class RequestLogMapping
{
    public static RequestLog ToModel(this RequestLogEntity entity)
    {
        return new RequestLog
        {
            Id = entity.Id,
            RequestId = entity.RequestId,
            Method = entity.Method,
            Path = entity.Path,
            QueryString = entity.QueryString,
            StatusCode = entity.StatusCode,
            ResponseTimeMs = entity.ResponseTimeMs,
            ClientIp = entity.ClientIp,
            UserAgent = entity.UserAgent,
            UserId = entity.UserId,
            UserName = entity.UserName,
            RequestTime = entity.RequestTime,
            RequestBodySize = entity.RequestBodySize,
            ResponseBodySize = entity.ResponseBodySize,
            Exception = entity.Exception,
            AdditionalInfo = entity.AdditionalInfo,
        };
    }

    public static RequestLogEntity ToEntity(this RequestLog model)
    {
        return new RequestLogEntity
        {
            Id = model.Id,
            RequestId = model.RequestId,
            Method = model.Method,
            Path = model.Path,
            QueryString = model.QueryString,
            StatusCode = model.StatusCode,
            ResponseTimeMs = model.ResponseTimeMs,
            ClientIp = model.ClientIp,
            UserAgent = model.UserAgent,
            UserId = model.UserId,
            UserName = model.UserName,
            RequestTime = model.RequestTime,
            RequestBodySize = model.RequestBodySize,
            ResponseBodySize = model.ResponseBodySize,
            Exception = model.Exception,
            AdditionalInfo = model.AdditionalInfo,
        };
    }

    public static IEnumerable<RequestLog> ToModels(this IEnumerable<RequestLogEntity> entities)
    {
        return entities.Select(e => e.ToModel());
    }
}
