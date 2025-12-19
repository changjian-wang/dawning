using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dawning.Identity.Infra.Data.PersistentObjects.Monitoring;

/// <summary>
/// 请求日志实体
/// </summary>
[Table("request_logs")]
public class RequestLogEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("request_id")]
    public string RequestId { get; set; } = string.Empty;

    [Column("method")]
    public string Method { get; set; } = string.Empty;

    [Column("path")]
    public string Path { get; set; } = string.Empty;

    [Column("query_string")]
    public string? QueryString { get; set; }

    [Column("status_code")]
    public int StatusCode { get; set; }

    [Column("response_time_ms")]
    public long ResponseTimeMs { get; set; }

    [Column("client_ip")]
    public string? ClientIp { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("user_name")]
    public string? UserName { get; set; }

    [Column("request_time")]
    public DateTime RequestTime { get; set; }

    [Column("request_body_size")]
    public long? RequestBodySize { get; set; }

    [Column("response_body_size")]
    public long? ResponseBodySize { get; set; }

    [Column("exception")]
    public string? Exception { get; set; }

    [Column("additional_info")]
    public string? AdditionalInfo { get; set; }
}
