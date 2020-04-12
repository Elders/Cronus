using System;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    internal static class CronusLogger
    {
        private static ILoggerFactory factory = new LoggerFactory(new ILoggerProvider[] { new FallbackLoggerProvider() });

        public static void Bootstrap(IServiceProvider serviceProvider)
        {
            factory = (serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory) ?? factory;
        }

        public static ILogger CreateLogger<T>() => factory.CreateLogger<T>();
        public static ILogger CreateLogger(Type type) => factory.CreateLogger(type);
        public static ILogger CreateLogger(string categoryName) => factory.CreateLogger(categoryName);

        public static bool IsTraceEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Trace);
        public static bool IsDebugEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Debug);
        public static bool IsInfoEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Information);
        public static bool IsWarningEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Warning);
        public static bool IsErrorEnabled(this ILogger logger) => logger.IsEnabled(LogLevel.Error);

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

        class FallbackLoggerProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => new FallbackLogger(categoryName);

            public void Dispose() { }

            class FallbackLogger : ILogger, IDisposable
            {
                private readonly string categoryName;

                public FallbackLogger(string categoryName)
                {
                    this.categoryName = categoryName;
                }

                public IDisposable BeginScope<TState>(TState state) => this;

                public void Dispose() { }

                public bool IsEnabled(LogLevel logLevel) => logLevel > LogLevel.Debug;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    var exceptionMessage = exception is null ? string.Empty : @$"
    Exception: {exception.GetType().Name} {exception.Message}
    {exception.StackTrace}

";
                    var message = @$"
!!! WARNING !!!
You are using internal Cronus logger '{nameof(FallbackLogger)}' because there are no other loggers registered for '{categoryName}';

    Level: {logLevel}
    {exceptionMessage}Message: {formatter(state, exception)}
!!! WARNING !!!";
                    System.Diagnostics.Debug.WriteLine(message);

                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ForegroundColor = color;
                }
            }
        }
    }
}
