using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public interface ICircuitBreakerErrorStrategy
    {
        bool Handle(TransportMessage errorMessage);
    }

    public interface ICircuitBreakerSuccessStrategy
    {
        bool Handle(TransportMessage successMessage);
    }

    public interface ICircuitBreakerRetryStrategy
    {
        bool Handle(TransportMessage retryMessage);
    }
}