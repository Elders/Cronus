using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    public static class CronusLogger
    {
        private static ILoggerFactory factory = new LoggerFactory();
        private static ILogger startupLogger;

        public static IHostBuilder UseCronusStartupLogger(this IHostBuilder hostBuilder, ILogger startupLogger)
        {
            SetStartupLogger(startupLogger);
            return hostBuilder;
        }

        public static void SetStartupLogger(ILogger startupLogger) => CronusLogger.startupLogger = startupLogger;

        internal static void Configure(ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null && startupLogger is null == false)
                startupLogger.LogWarning($"An instance of {nameof(ILoggerFactory)} is nowhere to be found. Casting blinding spell... To break the spell, you must configure your application logging.");

            factory = loggerFactory ?? factory;
            startupLogger = null;
        }

        public static ILogger CreateLogger<T>() => startupLogger ?? factory.CreateLogger<T>();
        public static ILogger CreateLogger(Type type) => startupLogger ?? factory.CreateLogger(type);
        public static ILogger CreateLogger(string categoryName) => startupLogger ?? factory.CreateLogger(categoryName);

        public static bool IsTraceEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Trace);
        public static bool IsDebugEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Debug);
        public static bool IsInfoEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Information);
        public static bool IsWarningEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Warning);
        public static bool IsErrorEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Error);
        public static bool IsCriticalEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Critical);

        public static void Trace(this ILogger logger, Func<string> func)
        {
            if (logger.IsTraceEnabled())
                logger.LogTrace(func());
        }

        public static void Debug(this ILogger logger, Func<string> func)
        {
            if (logger.IsDebugEnabled())
                logger.LogDebug(func());
        }

        public static void Debug(this ILogger logger, string message)
        {
            if (logger.IsDebugEnabled())
                logger.LogDebug(message);
        }

        public static void Info(this ILogger logger, Func<string> func)
        {
            if (logger.IsInfoEnabled())
                logger.LogInformation(func());
        }

        public static void Info(this ILogger logger, string message)
        {
            if (logger.IsInfoEnabled())
                logger.LogInformation(message);
        }

        public static void Info(this ILogger logger, string message, params object[] args)
        {
            if (logger.IsInfoEnabled())
                logger.LogInformation(message, args);
        }

        public static void Warn(this ILogger logger, Func<string> func)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(func());
        }

        public static void Warn(this ILogger logger, string message)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(message);
        }

        public static void WarnException(this ILogger logger, string message, Exception ex)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(ex, message);
        }

        public static void Error(this ILogger logger, Func<string> func)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(func());
        }

        public static void Error(this ILogger logger, string message)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(message);
        }

        public static void ErrorException(this ILogger logger, string message, Exception ex)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(ex, message);
        }

        public static void Critical(this ILogger logger, string message)
        {
            if (logger.IsCriticalEnabled())
                logger.LogCritical(message);
        }

        public static void CriticalException(this ILogger logger, string message, Exception ex)
        {
            if (logger.IsCriticalEnabled())
                logger.LogCritical(ex, message);
        }
    }
}
