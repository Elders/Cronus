using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelinePublisherSettings<TContract> : SettingsBuilder, IPipelinePublisherSettings<TContract> where TContract : IMessage
    {
        public PipelinePublisherSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder) { }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            builder.Container.RegisterSingleton<IPublisher<TContract>>(() =>
                new PipelinePublisher<TContract>(builder.Container.Resolve<IPipelineTransport>(builder.Name), builder.Container.Resolve<ISerializer>(builder.Name)), builder.Name);
        }
    }

    public class CommandPipelinePublisherSettings : PipelinePublisherSettings<ICommand>
    {
        public CommandPipelinePublisherSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder)
        {
            this.WithCommandPipelinePerApplication();
            this.WithCommandHandlerEndpointPerBoundedContext();
        }
    }

    public class EventPipelinePublisherSettings : PipelinePublisherSettings<IEvent>
    {
        public EventPipelinePublisherSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder)
        {
            this.WithEventPipelinePerApplication();
            this.WithProjectionEndpointPerBoundedContext();
        }
    }

}