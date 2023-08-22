using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Elders.Cronus.Diagnostics
{
    public static class CronusDiagnostics
    {
        private static ILogger logger;

        public const string Name = "Elders.Cronus.Diagnostics";

        public delegate TResult LogElapsedTimeWrapper<TResult>(Func<TResult> operation, string operationName) where TResult : new();

        public static TResult LogElapsedTime<TResult>(Func<TResult> operation, string operationName) where TResult : new()
        {
            logger ??= CronusLogger.CreateLogger(typeof(CronusDiagnostics));
            TResult result;
            long startTimestamp = 0;
            double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
            startTimestamp = Stopwatch.GetTimestamp();

            try
            {
                result = operation();
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Error when executing {operationName} after {GetElapsedTimeString()}. See inner exception"))
            {
                return new TResult();
            }

            logger.Info(() => "{cronus_OperationName} completed after {Elapsed}", operationName, GetElapsedTimeString());

            return result;

            string GetElapsedTimeString() => new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - startTimestamp))).ToString("c");
        }
    }
}
