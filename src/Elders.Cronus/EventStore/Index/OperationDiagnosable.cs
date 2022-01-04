using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Elders.Cronus.EventStore.Index
{
    public interface ICanBeDiagnosable
    {
        public T Execute<T>(Func<T> action, string name);
    }

    public class Operation : ICanBeDiagnosable
    {
        public T Execute<T>(Func<T> action, string name)
        {
            return action.Invoke();
        }
    }

    public class OperationDiagnosable : ICanBeDiagnosable
    {
        ILogger logger;

        public TResult Execute<TResult>(Func<TResult> action, string name)
        {
            return LogElapsedTime(action, name);
        }

        private TResult LogElapsedTime<TResult>(Func<TResult> action, string operationName)
        {
            logger = logger ?? CronusLogger.CreateLogger(typeof(ILogger<OperationDiagnosable>));
            TResult result;
            long startTimestamp = 0;
            double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
            startTimestamp = Stopwatch.GetTimestamp();

            try
            {
                result = action();
            }
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Error when executing {operationName} after {GetElapsedTimeString()}. See inner exception"))
            {
                return default;
            }

            logger.Info(() => $"{operationName} completed after {GetElapsedTimeString()}");
            return result;

            string GetElapsedTimeString() => new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - startTimestamp))).ToString("c");
        }
    }
}
