using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// OpenTelemetry 配置扩展
    /// 提供分布式追踪和指标收集功能
    /// </summary>
    public static class OpenTelemetryConfiguration
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public const string ServiceName = "Dawning.Identity.Api";

        /// <summary>
        /// 服务版本
        /// </summary>
        public static readonly string ServiceVersion =
            typeof(OpenTelemetryConfiguration).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        /// <summary>
        /// ActivitySource 用于创建追踪 Span
        /// </summary>
        public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

        /// <summary>
        /// Meter 用于创建自定义指标
        /// </summary>
        public static readonly Meter Meter = new(ServiceName, ServiceVersion);

        // 自定义指标
        private static readonly Counter<long> _requestCounter = Meter.CreateCounter<long>(
            "dawning_http_requests_total",
            description: "Total number of HTTP requests"
        );

        private static readonly Histogram<double> _requestDuration = Meter.CreateHistogram<double>(
            "dawning_http_request_duration_seconds",
            unit: "s",
            description: "HTTP request duration in seconds"
        );

        private static readonly Counter<long> _authSuccessCounter = Meter.CreateCounter<long>(
            "dawning_auth_success_total",
            description: "Total number of successful authentications"
        );

        private static readonly Counter<long> _authFailureCounter = Meter.CreateCounter<long>(
            "dawning_auth_failure_total",
            description: "Total number of failed authentications"
        );

        private static readonly Counter<long> _dbQueryCounter = Meter.CreateCounter<long>(
            "dawning_db_queries_total",
            description: "Total number of database queries"
        );

        /// <summary>
        /// 添加 OpenTelemetry 服务配置
        /// </summary>
        public static IServiceCollection AddOpenTelemetryConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var serviceName = configuration["OpenTelemetry:ServiceName"] ?? ServiceName;
            var enableTracing = configuration.GetValue("OpenTelemetry:EnableTracing", true);
            var enableMetrics = configuration.GetValue("OpenTelemetry:EnableMetrics", true);

            var otelBuilder = services.AddOpenTelemetry();

            // 配置资源信息
            otelBuilder.ConfigureResource(resource =>
                resource
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: ServiceVersion,
                        serviceInstanceId: Environment.MachineName
                    )
                    .AddAttributes(
                        new Dictionary<string, object>
                        {
                            ["deployment.environment"] =
                                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                ?? "Production",
                            ["host.name"] = Environment.MachineName,
                        }
                    )
            );

            // 配置追踪
            if (enableTracing)
            {
                otelBuilder.WithTracing(tracing =>
                {
                    tracing
                        .AddSource(ServiceName)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            // 过滤健康检查等噪音请求
                            options.Filter = httpContext =>
                            {
                                var path = httpContext.Request.Path.Value ?? "";
                                return !path.Contains("/health", StringComparison.OrdinalIgnoreCase)
                                    && !path.Contains(
                                        "/metrics",
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                    && !path.Contains(
                                        "/swagger",
                                        StringComparison.OrdinalIgnoreCase
                                    );
                            };

                            // 丰富 Span 信息
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity.SetTag(
                                    "http.client_ip",
                                    request.HttpContext.Connection.RemoteIpAddress?.ToString()
                                );
                                activity.SetTag(
                                    "http.user_agent",
                                    request.Headers.UserAgent.ToString()
                                );
                            };

                            options.EnrichWithHttpResponse = (activity, response) =>
                            {
                                activity.SetTag(
                                    "http.response_content_length",
                                    response.ContentLength
                                );
                            };

                            options.RecordException = true;
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                        });

                    // 如果配置了 OTLP 导出端点，添加 OTLP 导出器
                    var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        // 需要额外安装 OpenTelemetry.Exporter.OpenTelemetryProtocol 包
                        // tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                    }
                });
            }

            // 配置指标
            if (enableMetrics)
            {
                otelBuilder.WithMetrics(metrics =>
                {
                    metrics
                        .AddMeter(ServiceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        // Prometheus 导出
                        .AddPrometheusExporter();
                });
            }

            return services;
        }

        /// <summary>
        /// 配置 OpenTelemetry 中间件
        /// </summary>
        public static IApplicationBuilder UseOpenTelemetryConfiguration(
            this IApplicationBuilder app
        )
        {
            // Prometheus 指标端点
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            return app;
        }

        #region 自定义指标记录方法

        /// <summary>
        /// 记录 HTTP 请求
        /// </summary>
        public static void RecordRequest(string method, string path, int statusCode)
        {
            _requestCounter.Add(
                1,
                new KeyValuePair<string, object?>("method", method),
                new KeyValuePair<string, object?>("path", NormalizePath(path)),
                new KeyValuePair<string, object?>("status_code", statusCode)
            );
        }

        /// <summary>
        /// 记录请求耗时
        /// </summary>
        public static void RecordRequestDuration(
            string method,
            string path,
            int statusCode,
            double durationSeconds
        )
        {
            _requestDuration.Record(
                durationSeconds,
                new KeyValuePair<string, object?>("method", method),
                new KeyValuePair<string, object?>("path", NormalizePath(path)),
                new KeyValuePair<string, object?>("status_code", statusCode)
            );
        }

        /// <summary>
        /// 记录认证成功
        /// </summary>
        public static void RecordAuthSuccess(string authType = "password")
        {
            _authSuccessCounter.Add(1, new KeyValuePair<string, object?>("auth_type", authType));
        }

        /// <summary>
        /// 记录认证失败
        /// </summary>
        public static void RecordAuthFailure(
            string authType = "password",
            string reason = "invalid_credentials"
        )
        {
            _authFailureCounter.Add(
                1,
                new KeyValuePair<string, object?>("auth_type", authType),
                new KeyValuePair<string, object?>("reason", reason)
            );
        }

        /// <summary>
        /// 记录数据库查询
        /// </summary>
        public static void RecordDbQuery(string operation, string table)
        {
            _dbQueryCounter.Add(
                1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("table", table)
            );
        }

        /// <summary>
        /// 创建追踪 Span
        /// </summary>
        public static Activity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal
        )
        {
            return ActivitySource.StartActivity(name, kind);
        }

        /// <summary>
        /// 规范化路径（移除动态参数）
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";

            // 移除 GUID 参数
            var normalized = System.Text.RegularExpressions.Regex.Replace(
                path,
                @"/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
                "/{id}"
            );

            // 移除数字 ID 参数
            normalized = System.Text.RegularExpressions.Regex.Replace(
                normalized,
                @"/\d+(?=/|$)",
                "/{id}"
            );

            return normalized;
        }

        #endregion
    }
}
