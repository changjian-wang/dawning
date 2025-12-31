using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// OpenTelemetry configuration extension
    /// Provides distributed tracing and metrics collection functionality
    /// </summary>
    public static class OpenTelemetryConfiguration
    {
        /// <summary>
        /// Service name
        /// </summary>
        public const string ServiceName = "Dawning.Identity.Api";

        /// <summary>
        /// Service version
        /// </summary>
        public static readonly string ServiceVersion =
            typeof(OpenTelemetryConfiguration).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        /// <summary>
        /// ActivitySource for creating trace Spans
        /// </summary>
        public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

        /// <summary>
        /// Meter for creating custom metrics
        /// </summary>
        public static readonly Meter Meter = new(ServiceName, ServiceVersion);

        // Custom metrics
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
        /// Add OpenTelemetry service configuration
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

            // Configure resource information
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

            // Configure tracing
            if (enableTracing)
            {
                otelBuilder.WithTracing(tracing =>
                {
                    tracing
                        .AddSource(ServiceName)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            // Filter noisy requests like health checks
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

                            // Enrich Span information
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

                    // If OTLP export endpoint is configured, add OTLP exporter
                    var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        // Requires additional installation of OpenTelemetry.Exporter.OpenTelemetryProtocol package
                        // tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                    }
                });
            }

            // Configure metrics
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
                        // Prometheus export
                        .AddPrometheusExporter();
                });
            }

            return services;
        }

        /// <summary>
        /// Configure OpenTelemetry middleware
        /// </summary>
        public static IApplicationBuilder UseOpenTelemetryConfiguration(
            this IApplicationBuilder app
        )
        {
            // Prometheus metrics endpoint
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            return app;
        }

        #region Custom metrics recording methods

        /// <summary>
        /// Record HTTP request
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
        /// Record request duration
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
        /// Record authentication success
        /// </summary>
        public static void RecordAuthSuccess(string authType = "password")
        {
            _authSuccessCounter.Add(1, new KeyValuePair<string, object?>("auth_type", authType));
        }

        /// <summary>
        /// Record authentication failure
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
        /// Record database query
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
        /// Create trace Span
        /// </summary>
        public static Activity? StartActivity(
            string name,
            ActivityKind kind = ActivityKind.Internal
        )
        {
            return ActivitySource.StartActivity(name, kind);
        }

        /// <summary>
        /// Normalize path (remove dynamic parameters)
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";

            // Remove GUID parameters
            var normalized = System.Text.RegularExpressions.Regex.Replace(
                path,
                @"/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
                "/{id}"
            );

            // Remove numeric ID parameters
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
