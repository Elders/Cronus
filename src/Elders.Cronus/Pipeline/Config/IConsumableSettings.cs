using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerHostSettings
    {
        int NumberOfWorkers { get; set; }
        MessageThreshold MessageTreshold { get; set; }
    }

    public interface IConsumerHostSettings<TContract> : IHaveTransport<IPipelineTransport>, IHaveConsumer<TContract>, ISettingsBuilder<IEndpointConsumerHost>, IConsumerHostSettings, IHavePipelineSettings<TContract>, IHaveSerializer
        where TContract : IMessage
    {

    }
}