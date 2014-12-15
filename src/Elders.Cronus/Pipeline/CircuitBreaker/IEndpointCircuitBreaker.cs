using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public interface IEndpointCircuitBreaker
    {
        void PostConsume(IFeedResult mesages);
    }
}
