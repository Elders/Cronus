using System;
using System.Threading;

namespace Elders.Cronus
{
    public delegate ShouldRetry RetryPolicy();

    public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);

    public static class RetryableOperation
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RetryableOperation));

        static RetryPolicy defaultExponentialRetryPolicy = RetryPolicyFactory.CreateExponentialRetryPolicy(5, new TimeSpan(0, 0, 3), new TimeSpan(0, 0, 90), new TimeSpan(0, 0, 6));
        static RetryPolicy defaultLinearRetryPolicy = RetryPolicyFactory.CreateLinearRetryPolicy(5, new TimeSpan(0, 0, 1));

        /// <summary>
        /// Number of retries: 5;
        ///       Min Backoff: 3 seconds;
        ///       Max Backoff: 90 seconds;
        ///              Step: 6 seconds;
        /// </summary>
        public static RetryPolicy DefaultExponentialRetryPolicy { get { return defaultExponentialRetryPolicy; } }

        /// <summary>
        /// Number of retries: 5;
        ///             Delay: 1 second;
        /// </summary>
        public static RetryPolicy DefaultLinearRetryPolicy { get { return defaultLinearRetryPolicy; } }

        public static T TryExecute<T>(Func<T> operation, RetryPolicy retryPolicy, string operationInfo = null)
        {
            var retry = retryPolicy();
            Exception exception;
            T operationResult = default(T);

            TimeSpan delay;
            for (int i = 0; i > -1; i++)
            {
                operationResult = InvokeTryExecuteInternal(operation, out exception);
                if (operationResult == null || operationResult.Equals(default(T)))
                {
                    if (retry(i, exception, out delay))
                    {
                        log.DebugFormat("Retry {0} after {1} ms. Operation Info: {2}", i, delay.TotalMilliseconds, operationInfo ?? operation.ToString());
                        Thread.Sleep(delay);
                    }
                    else
                    {
                        log.Debug("Maximum number of retries has been reached.");
                        throw exception;
                    }
                }
                else
                {
                    break;
                }
            }
            return operationResult;
        }

        private static T InvokeTryExecuteInternal<T>(Func<T> operation, out Exception exception)
        {
            exception = null;
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                exception = ex;
                return default(T);
            }
        }

        public class RetryPolicyFactory
        {
            public static RetryPolicy CreateExponentialRetryPolicy(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            {
                return () =>
                {
                    return (int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
                    {
                        if (currentRetryCount < retryCount)
                        {
                            Random rand = new Random();
                            int increment = (int)((Math.Pow(2, currentRetryCount) - 1) * rand.Next((int)(deltaBackoff.TotalMilliseconds * 0.8), (int)(deltaBackoff.TotalMilliseconds * 1.2)));
                            int timeToSleepMsec = (int)Math.Min(minBackoff.TotalMilliseconds + increment, maxBackoff.TotalMilliseconds);

                            retryInterval = TimeSpan.FromMilliseconds(timeToSleepMsec);

                            return true;
                        }

                        retryInterval = TimeSpan.Zero;
                        return false;
                    };
                };
            }

            public static RetryPolicy CreateLinearRetryPolicy(int retryCount, TimeSpan intervalBetweenRetries)
            {
                return () =>
                {
                    return (int currentRetryCount, Exception lastException, out TimeSpan delay) =>
                    {
                        delay = intervalBetweenRetries;
                        return currentRetryCount < retryCount;

                    };
                };
            }

            public static RetryPolicy CreateInfiniteLinearRetryPolicy(TimeSpan intervalBetweenRetries)
            {
                return () =>
                {
                    return (int currentRetryCount, Exception lastException, out TimeSpan delay) =>
                    {
                        delay = intervalBetweenRetries;
                        return true;

                    };
                };
            }
        }
    }
}
