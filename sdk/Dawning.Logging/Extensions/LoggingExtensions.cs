using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dawning.Logging.Extensions;

/// <summary>
/// 日志扩展方法
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// 使用 Dawning 统一日志配置
    /// </summary>
    /// <param name="builder">Host builder</param>
    /// <param name="applicationName">应用程序名称</param>
    /// <returns></returns>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Host.UseDawningLogging("MyService");
    /// </code>
    /// </example>
    public static IHostBuilder UseDawningLogging(
        this IHostBuilder hostBuilder,
        string applicationName
    )
    {
        return hostBuilder.UseDawningLogging(options =>
        {
            options.ApplicationName = applicationName;
        });
    }

    /// <summary>
    /// 使用 Dawning 统一日志配置 (带配置选项)
    /// </summary>
    /// <param name="builder">Host builder</param>
    /// <param name="configure">配置回调</param>
    /// <returns></returns>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Host.UseDawningLogging(options =>
    /// {
    ///     options.ApplicationName = "MyService";
    ///     options.MinimumLevel = LogLevel.Debug;
    ///     options.EnableFile = true;
    /// });
    /// </code>
    /// </example>
    public static IHostBuilder UseDawningLogging(
        this IHostBuilder hostBuilder,
        Action<DawningLogOptions> configure
    )
    {
        var options = new DawningLogOptions();
        configure(options);

        return hostBuilder.UseSerilog(
            (context, services, loggerConfiguration) =>
            {
                ConfigureLogger(loggerConfiguration, options, context.Configuration);
            }
        );
    }

    /// <summary>
    /// 创建启动日志器 (用于捕获启动错误)
    /// </summary>
    /// <param name="applicationName">应用程序名称</param>
    /// <returns>配置好的 Logger</returns>
    /// <example>
    /// <code>
    /// Log.Logger = DawningLoggingExtensions.CreateBootstrapLogger("MyService");
    ///
    /// try
    /// {
    ///     var builder = WebApplication.CreateBuilder(args);
    ///     // ...
    /// }
    /// catch (Exception ex)
    /// {
    ///     Log.Fatal(ex, "Application start-up failed");
    /// }
    /// finally
    /// {
    ///     Log.CloseAndFlush();
    /// }
    /// </code>
    /// </example>
    public static ILogger CreateBootstrapLogger(string applicationName)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// 配置日志记录器
    /// </summary>
    private static void ConfigureLogger(
        LoggerConfiguration loggerConfiguration,
        DawningLogOptions options,
        IConfiguration configuration
    )
    {
        // 基础配置
        loggerConfiguration
            .MinimumLevel.Is(options.ToSerilogLevel(options.MinimumLevel))
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", options.ApplicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId();

        // 配置命名空间覆盖
        foreach (var (ns, level) in options.OverrideMinimumLevels)
        {
            loggerConfiguration.MinimumLevel.Override(ns, options.ToSerilogLevel(level));
        }

        // 从配置文件读取额外配置 (可选)
        loggerConfiguration.ReadFrom.Configuration(configuration);

        // 控制台输出
        if (options.EnableConsole)
        {
            if (options.UseJsonFormat)
            {
                loggerConfiguration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
            }
            else
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: options.ConsoleOutputTemplate);
            }
        }

        // 文件输出
        if (options.EnableFile)
        {
            var filePath = options.GetLogFilePath();
            var fileSizeBytes = options.FileSizeLimitMb * 1024 * 1024L;

            if (options.UseJsonFormat)
            {
                loggerConfiguration.WriteTo.File(
                    new Serilog.Formatting.Json.JsonFormatter(),
                    filePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    fileSizeLimitBytes: fileSizeBytes,
                    rollOnFileSizeLimit: options.RollOnFileSizeLimit
                );
            }
            else
            {
                loggerConfiguration.WriteTo.File(
                    filePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: options.FileOutputTemplate,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    fileSizeLimitBytes: fileSizeBytes,
                    rollOnFileSizeLimit: options.RollOnFileSizeLimit
                );
            }
        }
    }
}
