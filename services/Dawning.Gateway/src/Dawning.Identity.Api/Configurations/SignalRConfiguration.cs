using Dawning.Identity.Api.Adapters;
using Dawning.Identity.Api.Hubs;
using Dawning.Identity.Api.Services;
using Dawning.Identity.Application.Interfaces.Notification;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// SignalR 实时通信配置
    /// </summary>
    public static class SignalRConfiguration
    {
        /// <summary>
        /// 添加 SignalR 服务
        /// </summary>
        public static IServiceCollection AddSignalRConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // 添加 SignalR 服务
            var signalRBuilder = services.AddSignalR(options =>
            {
                // 启用详细错误信息（仅开发环境）
                options.EnableDetailedErrors = true;

                // 客户端超时时间（默认30秒）
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);

                // 保持连接间隔（默认15秒）
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);

                // 握手超时时间
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);

                // 最大接收消息大小（1MB）
                options.MaximumReceiveMessageSize = 1024 * 1024;

                // 流缓冲区大小
                options.StreamBufferCapacity = 10;
            });

            // 如果配置了 Redis，使用 Redis 作为背板
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

            // 添加消息包序列化选项
            signalRBuilder.AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = System
                    .Text
                    .Json
                    .JsonNamingPolicy
                    .CamelCase;
                options.PayloadSerializerOptions.WriteIndented = false;
            });

            // 注册通知服务
            services.AddScoped<INotificationService, NotificationService>();

            // 注册实时通知适配器（供 Application 层使用）
            services.AddScoped<IRealTimeNotificationService, SignalRNotificationAdapter>();

            // 注册日志流服务（实时日志推送）
            services.AddSingleton<ILogStreamService, LogStreamService>();

            return services;
        }

        /// <summary>
        /// 配置 SignalR 端点
        /// </summary>
        public static IApplicationBuilder UseSignalRConfiguration(this IApplicationBuilder app)
        {
            return app;
        }

        /// <summary>
        /// 映射 SignalR Hub 端点
        /// </summary>
        public static IEndpointRouteBuilder MapSignalRHubs(this IEndpointRouteBuilder endpoints)
        {
            // 主通知 Hub
            endpoints.MapHub<NotificationHub>(
                "/hubs/notification",
                options =>
                {
                    // 传输方式配置
                    options.Transports =
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets
                        | Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents
                        | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;

                    // WebSocket 配置
                    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(5);

                    // 长轮询配置
                    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);

                    // 授权要求
                    options.AllowStatefulReconnects = true;
                }
            );

            return endpoints;
        }
    }
}
