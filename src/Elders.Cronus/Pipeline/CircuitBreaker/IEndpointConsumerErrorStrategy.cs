namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public interface ICircuitBreakerErrorStrategy
    {
        bool Handle(CronusMessage errorMessage);
    }

    public interface ICircuitBreakerSuccessStrategy
    {
        bool Handle(CronusMessage successMessage);
    }

    public interface ICircuitBreakerRetryStrategy
    {
        bool Handle(CronusMessage retryMessage);

        bool AllowsProcessing(string origin, CronusMessage retryMessage);
    }
}