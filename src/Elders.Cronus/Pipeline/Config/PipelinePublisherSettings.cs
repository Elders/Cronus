using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelinePublisherSettings<TContract> : IPipelinePublisherSettings<TContract> where TContract : IMessage
    {
        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            builder.Container.RegisterSingleton<IPublisher<TContract>>(() =>
                new PipelinePublisher<TContract>(builder.Container.Resolve<IPipelineTransport>(builder.Name), builder.Container.Resolve<ISerializer>(builder.Name)), builder.Name);
        }

        //Lazy<IPublisher<TContract>> ISettingsBuilder<IPublisher<TContract>>.Build()
        //{
        //    IPipelinePublisherSettings<TContract> settings = this as IPipelinePublisherSettings<TContract>;
        //    return new Lazy<IPublisher<TContract>>(() => new PipelinePublisher<TContract>(settings.Transport.Value, settings.Serializer));
        //}
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