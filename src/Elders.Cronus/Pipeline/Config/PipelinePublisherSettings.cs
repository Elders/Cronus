using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelinePublisherSettings<TContract> : SettingsBuilder where TContract : IMessage
    {
        public PipelinePublisherSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            Func<ITransport> transport = () => builder.Container.Resolve<ITransport>(builder.Name);
            Func<ISerializer> serializer = () => builder.Container.Resolve<ISerializer>();
            builder.Container.RegisterSingleton<IPublisher<TContract>>(() =>
                new PipelinePublisher<TContract>(transport(), serializer()), builder.Name);
        }
    }

    public class CommandPipelinePublisherSettings : PipelinePublisherSettings<ICommand>
    {
        public CommandPipelinePublisherSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class EventPipelinePublisherSettings : PipelinePublisherSettings<IEvent>
    {
        public EventPipelinePublisherSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class SagaPipelinePublisherSettings : PipelinePublisherSettings<IScheduledMessage>
    {
        public SagaPipelinePublisherSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }
}
