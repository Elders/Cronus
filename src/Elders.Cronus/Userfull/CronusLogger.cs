using System;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    internal static class CronusLogger
    {
        private static ILoggerFactory factory = new LoggerFactory();

        public static void Bootstrap(IServiceProvider serviceProvider)
        {
            factory = (serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory) ?? factory;
        }

        public static ILogger<T> CreateLogger<T>() => factory.CreateLogger<T>();

        public static ILogger CreateLogger(Type type) => factory.CreateLogger(type);
    }
}
