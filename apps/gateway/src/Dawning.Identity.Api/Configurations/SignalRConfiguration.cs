using Dawning.Identity.Api.Adapters;
using Dawning.Identity.Api.Hubs;
using Dawning.Identity.Api.Services;
using Dawning.Identity.Application.Interfaces.Notification;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// SignalR real-time communication configuration
    /// </summary>
    public static class SignalRConfiguration
    {
        /// <summary>
        /// Add SignalR services
        /// </summary>
        public static IServiceCollection AddSignalRConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Add SignalR services
            var signalRBuilder = services.AddSignalR(options =>
            {
                // Enable detailed error messages (development only)
                options.EnableDetailedErrors = true;

                // Client timeout (default 30 seconds)
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);

                // Keep alive interval (default 15 seconds)
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);

                // Handshake timeout
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);

                // Maximum receive message size (1MB)
                options.MaximumReceiveMessageSize = 1024 * 1024;

                // Stream buffer capacity
                options.StreamBufferCapacity = 10;
            });

            // If Redis is configured, use Redis as backplane
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                signalRBuilder.AddStackExchangeRedis(
                    redisConnection,
                    options =>
                    {
                        options.Configuration.ChannelPrefix =
                            StackExchange.Redis.RedisChannel.Literal("Dawning:SignalR:");
                    }
                );
            }

            // Add message packet serialization options
            signalRBuilder.AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = System
                    .Text
                    .Json
                    .JsonNamingPolicy
                    .CamelCase;
                options.PayloadSerializerOptions.WriteIndented = false;
            });

            // Register notification service
            services.AddScoped<INotificationService, NotificationService>();

            // Register real-time notification adapter (for Application layer use)
            services.AddScoped<IRealTimeNotificationService, SignalRNotificationAdapter>();

            // Register log stream service (real-time log push)
            services.AddSingleton<ILogStreamService, LogStreamService>();

            return services;
        }

        /// <summary>
        /// Configure SignalR endpoints
        /// </summary>
        public static IApplicationBuilder UseSignalRConfiguration(this IApplicationBuilder app)
        {
            return app;
        }

        /// <summary>
        /// Map SignalR Hub endpoints
        /// </summary>
        public static IEndpointRouteBuilder MapSignalRHubs(this IEndpointRouteBuilder endpoints)
        {
            // Main notification Hub
            endpoints.MapHub<NotificationHub>(
                "/hubs/notification",
                options =>
                {
                    // Transport configuration
                    options.Transports =
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets
                        | Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents
                        | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;

                    // WebSocket configuration
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(5);

                    // Long polling configuration
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);

                    // Authorization requirement
                    options.AllowStatefulReconnects = true;
                }
            );

            return endpoints;
        }
    }
}
