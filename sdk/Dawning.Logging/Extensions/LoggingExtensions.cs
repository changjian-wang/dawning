using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dawning.Logging.Extensions;

/// <summary>
/// Logging extension methods
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Uses Dawning unified logging configuration
    /// </summary>
    /// <param name="builder">Host builder</param>
    /// <param name="applicationName">Application name</param>
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
    /// Uses Dawning unified logging configuration (with configuration options)
    /// </summary>
    /// <param name="builder">Host builder</param>
    /// <param name="configure">Configuration callback</param>
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
    /// Creates a bootstrap logger (for capturing startup errors)
    /// </summary>
    /// <param name="applicationName">Application name</param>
    /// <returns>Configured Logger</returns>
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
    /// Configures the logger
    /// </summary>
    private static void ConfigureLogger(
        LoggerConfiguration loggerConfiguration,
        DawningLogOptions options,
        IConfiguration configuration
    )
    {
        // Basic configuration
        loggerConfiguration
            .MinimumLevel.Is(options.ToSerilogLevel(options.MinimumLevel))
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", options.ApplicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId();

        // Configure namespace overrides
        foreach (var (ns, level) in options.OverrideMinimumLevels)
        {
            loggerConfiguration.MinimumLevel.Override(ns, options.ToSerilogLevel(level));
        }

        // Read additional configuration from config file (optional)
        loggerConfiguration.ReadFrom.Configuration(configuration);

        // Console output
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

        // File output
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
