using System;

namespace NMSD.Cronus.Core
{
    public delegate ShouldRetry RetryPolicy();

    public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);

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
