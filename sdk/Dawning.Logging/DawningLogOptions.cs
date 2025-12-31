using Serilog;

namespace Dawning.Logging;

/// <summary>
/// Logging configuration options
/// </summary>
public class DawningLogOptions
{
    /// <summary>
    /// Application name (for log identification)
    /// </summary>
    public string ApplicationName { get; set; } = "DawningApp";

    /// <summary>
    /// Log file path template
    /// Default: logs/{ApplicationName}-.log
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// Whether to enable console logging
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Whether to enable file logging
    /// </summary>
    public bool EnableFile { get; set; } = true;

    /// <summary>
    /// Minimum log level
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Log file retention days
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 31;

    /// <summary>
    /// Maximum single log file size (MB)
    /// </summary>
    public int FileSizeLimitMb { get; set; } = 50;

    /// <summary>
    /// Whether to roll log files
    /// </summary>
    public bool RollOnFileSizeLimit { get; set; } = true;

    /// <summary>
    /// Whether to use JSON format
    /// </summary>
    public bool UseJsonFormat { get; set; } = false;

    /// <summary>
    /// Console log template
    /// </summary>
    public string ConsoleOutputTemplate { get; set; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// File log template
    /// </summary>
    public string FileOutputTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{TraceId}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Override log level for specific namespaces
    /// </summary>
    public Dictionary<string, LogLevel> OverrideMinimumLevels { get; set; } =
        new()
        {
            ["Microsoft"] = LogLevel.Warning,
            ["Microsoft.Hosting.Lifetime"] = LogLevel.Information,
            ["Microsoft.AspNetCore.Authentication"] = LogLevel.Warning,
            ["System"] = LogLevel.Warning,
        };

    /// <summary>
    /// Gets the complete log file path
    /// </summary>
    internal string GetLogFilePath()
    {
        return LogFilePath ?? $"logs/{ApplicationName}-.log";
    }

    /// <summary>
    /// Converts to Serilog LogEventLevel
    /// </summary>
    internal Serilog.Events.LogEventLevel ToSerilogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
            LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information,
        };
    }
}

/// <summary>
/// Log level (compatible with Microsoft.Extensions.Logging)
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6,
}
