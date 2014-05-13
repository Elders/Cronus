using Elders.Cronus.DomainModelling;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IEndpointConsumerBuilder<out TContract>
        where TContract : IMessage
    {
        IEndpointConsumable<TContract> Build();
    }
}