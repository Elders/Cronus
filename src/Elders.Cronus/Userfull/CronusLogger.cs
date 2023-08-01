using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    public static class Log
    {
        public const string Tenant = "cronus_tenant";
        public const string AggregateId = "cronus_arid";
        public const string AggregateName = "cronus_arname";

        public const string JobName = "cronus_job_name";
        public const string JobData = "cronus_job_data";

        public const string ProjectionName = "cronus_projection_name";
        public const string ProjectionType = "cronus_projection_type";
        public const string ProjectionInstanceId = "cronus_projection_id";
        public const string ProjectionVersion = "cronus_projection_version";
    }

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

        public static void Trace(this ILogger logger, Func<string> func, params object[] args)
        {
            if (logger.IsTraceEnabled())
                logger.LogTrace(func(), args);
        }

        public static void Debug(this ILogger logger, Func<string> func)
        {
            if (logger.IsDebugEnabled())
                logger.LogDebug(func());
        }

        public static void Debug(this ILogger logger, Func<string> func, params object[] args)
        {
            if (logger.IsDebugEnabled())
                logger.LogDebug(func(), args);
        }

        public static void Info(this ILogger logger, Func<string> func)
        {
            if (logger.IsInfoEnabled())
                logger.LogInformation(func());
        }

        public static void Info(this ILogger logger, Func<string> func, params object[] args)
        {
            if (logger.IsInfoEnabled())
                logger.LogInformation(func(), args);
        }

        public static void Warn(this ILogger logger, Func<string> func)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(func());
        }

        public static void Warn(this ILogger logger, Func<string> func, params object[] args)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(func(), args);
        }

        public static bool WarnException(this ILogger logger, Exception ex, Func<string> func)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(ex, func());

            return true;
        }

        public static bool WarnException(this ILogger logger, Exception ex, Func<string> func, params object[] args)
        {
            if (logger.IsWarningEnabled())
                logger.LogWarning(ex, func(), args);

            return true;
        }

        public static void Error(this ILogger logger, Func<string> func)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(func());
        }

        public static void Error(this ILogger logger, Func<string> func, params object[] args)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(func(), args);
        }

        public static bool ErrorException(this ILogger logger, Exception ex, Func<string> func)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(ex, func());

            return true;
        }

        public static void ErrorException(this ILogger logger, Exception ex, Func<string> func, params object[] args)
        {
            if (logger.IsErrorEnabled())
                logger.LogError(ex, func(), args);
        }

        public static void Critical(this ILogger logger, Func<string> func)
        {
            if (logger.IsCriticalEnabled())
                logger.LogCritical(func());
        }

        public static void Critical(this ILogger logger, Func<string> func, params object[] args)
        {
            if (logger.IsCriticalEnabled())
                logger.LogCritical(func(), args);
        }

        public static bool CriticalException(this ILogger logger, Exception ex, Func<string> func)
        {
            if (logger.IsCriticalEnabled())
                logger.LogCritical(ex, func());

            return true;
        }

        public static bool CriticalException(this ILogger logger, Exception ex, Func<string> func, params object[] args)
        {
            if (logger.IsCriticalEnabled())
                logger.LogCritical(ex, func(), args);

            return true;
        }

        public static IDisposable BeginScope(this ILogger logger, string key, object value)
        {
            return logger.BeginScope(new Dictionary<string, object> { { key, value } });
        }

        public static IDisposable BeginScope(this ILogger logger, Action<Dictionary<string, object>> scope)
        {
            var loggerScope = new Dictionary<string, object>();
            scope(loggerScope);
            return logger.BeginScope(loggerScope);
        }

        public static Dictionary<string, object> AddScope(this Dictionary<string, object> scope, string key, object value)
        {
            scope.Add(key, value);
            return scope;
        }
    }
}
