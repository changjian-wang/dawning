using System.Collections.Concurrent;
using Dawning.Identity.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Api.Logging
{
    /// <summary>
    /// SignalR logger provider - Pushes logs to SignalR Hub
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
    /// SignalR logger
    /// </summary>
    public class SignalRLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IServiceProvider _serviceProvider;
        private readonly LogLevel _minLevel;

        // Excluded categories (to avoid circular logging)
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
            // Check log level
            if (logLevel < _minLevel)
                return false;

            // Exclude specific categories to avoid circular logging
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
                // Use fire-and-forget pattern to push logs
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
                        // Ignore push failures to avoid affecting normal logging flow
                    }
                });
            }
            catch
            {
                // Ignore any errors
            }
        }

        private static string? GetRequestId()
        {
            // Try to get request ID from AsyncLocal or Activity
            return System.Diagnostics.Activity.Current?.Id;
        }
    }

    /// <summary>
    /// SignalR logger extension methods
    /// </summary>
    public static class SignalRLoggerExtensions
    {
        /// <summary>
        /// Add SignalR logger provider
        /// </summary>
        /// <param name="builder">Logging builder</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="minLevel">Minimum log level (default Information)</param>
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
