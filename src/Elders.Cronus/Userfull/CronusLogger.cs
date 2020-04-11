using System;
using Microsoft.Extensions.Logging;

namespace Elders.Cronus
{
    internal static class CronusLogger
    {
        private static ILoggerFactory factory = new LoggerFactory();

        public static void SetLoggerFactory(ILoggerFactory factory)
        {
            if (factory is null) throw new ArgumentNullException(nameof(factory));

            CronusLogger.factory = factory;
        }

        public static ILogger<T> CreateLogger<T>() => factory.CreateLogger<T>();
    }
}
