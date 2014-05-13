using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpointConsumable<out TContract> where TContract : IMessage
    {
        int NumberOfWorkers { get; }

        void Start(int? numberOfWorkers = null);

        void Stop();
    }
}