using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelinePublisherSettings<TContract> : IPipelinePublisherSettings<TContract> where TContract : IMessage
    {
        ProtoregSerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IPipelineTransport> IHaveTransport<IPipelineTransport>.Transport { get; set; }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        Lazy<IPublisher<TContract>> ISettingsBuilder<IPublisher<TContract>>.Build()
        {
            IPipelinePublisherSettings<TContract> settings = this as IPipelinePublisherSettings<TContract>;
            return new Lazy<IPublisher<TContract>>(() => new PipelinePublisher<TContract>(settings.Transport.Value.PipelineFactory, settings.Serializer));
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

    public class EventStorePipelinePublisherSettings : IPipelinePublisherSettings<DomainMessageCommit>
    {
        public EventStorePipelinePublisherSettings()
        {
            this.WithEventStorePipelinePerApplication();
            this.WithEventStoreEndpointPerBoundedContext();
        }

        ProtoregSerializer IHaveSerializer.Serializer { get; set; }

        Lazy<IPipelineTransport> IHaveTransport<IPipelineTransport>.Transport { get; set; }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        Lazy<IPublisher<DomainMessageCommit>> ISettingsBuilder<IPublisher<DomainMessageCommit>>.Build()
        {
            IPipelinePublisherSettings<DomainMessageCommit> settings = this as IPipelinePublisherSettings<DomainMessageCommit>;
            return new Lazy<IPublisher<DomainMessageCommit>>(() => new EventStorePublisher(settings.Transport.Value.PipelineFactory, settings.Serializer));
        }
    }
}