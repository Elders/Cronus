using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelinePublisherSettings<TContract> : IPipelinePublisherSettings<TContract> where TContract : IMessage
    {
        ISerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IPipelineTransport> IHaveTransport<IPipelineTransport>.Transport { get; set; }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        Lazy<IPublisher<TContract>> ISettingsBuilder<IPublisher<TContract>>.Build()
        {
            IPipelinePublisherSettings<TContract> settings = this as IPipelinePublisherSettings<TContract>;
            return new Lazy<IPublisher<TContract>>(() => new PipelinePublisher<TContract>(settings.Transport.Value, settings.Serializer));
        }
    }

    public class CommandPipelinePublisherSettings : PipelinePublisherSettings<ICommand>
    {
        public CommandPipelinePublisherSettings()
        {
            this.WithCommandPipelinePerApplication();
            this.WithCommandHandlerEndpointPerBoundedContext();
        }
    }

    public class EventPipelinePublisherSettings : PipelinePublisherSettings<IEvent>
    {
        public EventPipelinePublisherSettings()
        {
            this.WithEventPipelinePerApplication();
            this.WithProjectionEndpointPerBoundedContext();
        }
    }

}