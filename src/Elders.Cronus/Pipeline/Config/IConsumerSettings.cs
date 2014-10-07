using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport;

namespace Elders.Cronus.Pipeline.Config
{
    public interface IConsumerSettings
    {
        string Name { get; set; }
        int NumberOfWorkers { get; set; }
        MessageThreshold MessageTreshold { get; set; }
    }

    public interface IHaveEventStore
    {
        Lazy<IEventStore> EventStore { get; set; }
    }

    public interface IConsumerSettings<TContract> : IHaveContainer, IHaveTransport<IPipelineTransport>, IHaveMessageProcessor<TContract>, ISettingsBuilder<IEndpointConsumer>, IConsumerSettings, IHaveCircuitBreaker, IHavePipelineSettings<TContract>, IHaveSerializer
        where TContract : IMessage
    {

    }
}