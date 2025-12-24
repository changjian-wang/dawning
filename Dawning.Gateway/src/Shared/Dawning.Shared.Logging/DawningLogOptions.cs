using Serilog;

namespace Dawning.Shared.Logging;

/// <summary>
/// 日志配置选项
/// </summary>
public class DawningLogOptions
{
    /// <summary>
    /// 应用程序名称 (用于日志标识)
    /// </summary>
    public string ApplicationName { get; set; } = "DawningApp";

    /// <summary>
    /// 日志文件路径模板
    /// 默认: logs/{ApplicationName}-.log
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// 是否启用控制台日志
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// 是否启用文件日志
    /// </summary>
    public bool EnableFile { get; set; } = true;

    /// <summary>
    /// 最小日志级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// 日志文件保留天数
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 31;

    /// <summary>
    /// 单个日志文件最大大小 (MB)
    /// </summary>
    public int FileSizeLimitMb { get; set; } = 50;

    /// <summary>
    /// 是否滚动日志文件
    /// </summary>
    public bool RollOnFileSizeLimit { get; set; } = true;

    /// <summary>
    /// 是否使用 JSON 格式
    /// </summary>
    public bool UseJsonFormat { get; set; } = false;

    /// <summary>
    /// 控制台日志模板
    /// </summary>
    public string ConsoleOutputTemplate { get; set; } = 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// 文件日志模板
    /// </summary>
    public string FileOutputTemplate { get; set; } = 
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{TraceId}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// 覆盖特定命名空间的日志级别
    /// </summary>
    public Dictionary<string, LogLevel> OverrideMinimumLevels { get; set; } = new()
    {
        ["Microsoft"] = LogLevel.Warning,
        ["Microsoft.Hosting.Lifetime"] = LogLevel.Information,
        ["Microsoft.AspNetCore.Authentication"] = LogLevel.Warning,
        ["System"] = LogLevel.Warning
    };

    /// <summary>
    /// 获取完整的日志文件路径
    /// </summary>
    internal string GetLogFilePath()
    {
        return LogFilePath ?? $"logs/{ApplicationName}-.log";
    }

    /// <summary>
    /// 转换为 Serilog LogEventLevel
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
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}

/// <summary>
/// 日志级别 (与 Microsoft.Extensions.Logging 兼容)
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    None = 6
}
