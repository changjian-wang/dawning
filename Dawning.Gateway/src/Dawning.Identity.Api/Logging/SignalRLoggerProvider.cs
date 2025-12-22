using System.Collections.Concurrent;
using Dawning.Identity.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Logging
{
    /// <summary>
    /// SignalR 日志提供器 - 将日志推送到 SignalR Hub
    /// </summary>
    public class SignalRLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, SignalRLogger> _loggers = new();
        private readonly LogLevel _minLevel;

        public SignalRLoggerProvider(
            IServiceProvider serviceProvider,
            LogLevel minLevel = LogLevel.Information
        )
        {
            _serviceProvider = serviceProvider;
            _minLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(
                categoryName,
                name => new SignalRLogger(name, _serviceProvider, _minLevel)
            );
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    /// <summary>
    /// SignalR 日志记录器
    /// </summary>
    public class SignalRLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IServiceProvider _serviceProvider;
        private readonly LogLevel _minLevel;

        // 排除的类别（避免循环日志）
        private static readonly HashSet<string> ExcludedCategories = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "Microsoft.AspNetCore.SignalR",
            "Microsoft.AspNetCore.Http.Connections",
            "Dawning.Identity.Api.Services.LogStreamService",
            "Dawning.Identity.Api.Services.NotificationService",
            "Dawning.Identity.Api.Hubs.NotificationHub",
        };

        public SignalRLogger(
            string categoryName,
            IServiceProvider serviceProvider,
            LogLevel minLevel
        )
        {
            _categoryName = categoryName;
            _serviceProvider = serviceProvider;
            _minLevel = minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // 检查日志级别
            if (logLevel < _minLevel)
                return false;

            // 排除特定类别避免循环
            foreach (var excluded in ExcludedCategories)
            {
                if (_categoryName.StartsWith(excluded, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            if (!IsEnabled(logLevel))
                return;

            try
            {
                // 使用 fire-and-forget 模式推送日志
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var logStreamService =
                            scope.ServiceProvider.GetService<ILogStreamService>();

                        if (logStreamService != null)
                        {
                            var entry = new LogEntry
                            {
                                Timestamp = DateTime.UtcNow,
                                Level = logLevel.ToString(),
                                Message = formatter(state, exception),
                                Exception = exception?.ToString(),
                                Category = _categoryName,
                                RequestId = GetRequestId(),
                            };

                            await logStreamService.PushLogAsync(entry);
                        }
                    }
                    catch
                    {
                        // 忽略推送失败，避免影响正常日志流程
                    }
                });
            }
            catch
            {
                // 忽略任何错误
            }
        }

        private static string? GetRequestId()
        {
            // 尝试从 AsyncLocal 或 Activity 获取请求ID
            return System.Diagnostics.Activity.Current?.Id;
        }
    }

    /// <summary>
    /// SignalR 日志扩展方法
    /// </summary>
    public static class SignalRLoggerExtensions
    {
        /// <summary>
        /// 添加 SignalR 日志提供器
        /// </summary>
        /// <param name="builder">日志构建器</param>
        /// <param name="serviceProvider">服务提供器</param>
        /// <param name="minLevel">最小日志级别（默认 Information）</param>
        public static ILoggingBuilder AddSignalRLogger(
            this ILoggingBuilder builder,
            IServiceProvider serviceProvider,
            LogLevel minLevel = LogLevel.Information
        )
        {
            builder.AddProvider(new SignalRLoggerProvider(serviceProvider, minLevel));
            return builder;
        }
    }
}
