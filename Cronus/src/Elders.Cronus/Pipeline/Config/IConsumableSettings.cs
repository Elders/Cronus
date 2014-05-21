using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumableSettings
    {
        int NumberOfWorkers { get; set; }
        MessageThreshold MessageTreshold { get; set; }
    }

    public interface IConsumableSettings<TContract> : IHaveTransport<IPipelineTransport>, IHaveConsumer<TContract>, ISettingsBuilder<IEndpointConsumable>, IConsumableSettings, IHavePipelineSettings<TContract>, IHaveSerializer
        where TContract : IMessage
    {

    }
}