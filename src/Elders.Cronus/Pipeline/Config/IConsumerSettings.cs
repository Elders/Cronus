using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings
    {
        int NumberOfWorkers { get; set; }
        MessageThreshold MessageTreshold { get; set; }
    }

    public interface IConsumerSettings<TContract> : IHaveTransport<IPipelineTransport>, IHaveMessageProcessor<TContract>, ISettingsBuilder<IEndpointConsumer>, IConsumerSettings, IHaveCircuitBreaker, IHavePipelineSettings<TContract>, IHaveSerializer
        where TContract : IMessage
    {

    }
}