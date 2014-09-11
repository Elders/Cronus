using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class ConsumerHostSettings<TContract> : IConsumerHostSettings<TContract>
        where TContract : IMessage
    {
        Lazy<IConsumer<TContract>> IHaveConsumer<TContract>.Consumer { get; set; }

        int IConsumerHostSettings.NumberOfWorkers { get; set; }

        Lazy<IPipelineTransport> IHaveTransport<IPipelineTransport>.Transport { get; set; }

        Lazy<IEndpointConsumerHost> ISettingsBuilder<IEndpointConsumerHost>.Build()
        {
            IConsumerHostSettings<TContract> settings = this as IConsumerHostSettings<TContract>;
            return new Lazy<IEndpointConsumerHost>(() => new EndpointConsumerHost<TContract>(settings.Transport.Value, settings.Consumer.Value, settings.Serializer, settings.MessageTreshold));
        }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        ISerializer IHaveSerializer.Serializer { get; set; }

        MessageThreshold IConsumerHostSettings.MessageTreshold { get; set; }
    }

    public class CommandConsumerHostSettings : ConsumerHostSettings<ICommand>
    {
        public CommandConsumerHostSettings()
        {
            this.SetMessageThreshold(100, 30);
            this.WithCommandPipelinePerApplication();
            this.WithCommandHandlerEndpointPerBoundedContext();
        }
    }

    public class ProjectionConsumerHostSettings : ConsumerHostSettings<IEvent>
    {
        public ProjectionConsumerHostSettings()
        {
            this.SetMessageThreshold(100, 30);
            this.WithEventPipelinePerApplication();
            this.WithProjectionEndpointPerBoundedContext();
        }
    }

    public class PortConsumerHostSettings : ConsumerHostSettings<IEvent>
    {
        public PortConsumerHostSettings()
        {
            this.SetMessageThreshold(100, 30);
            this.WithEventPipelinePerApplication();
            this.WithPortEndpointPerBoundedContext();
        }
    }

    public class EventStoreConsumerHostSettings : ConsumerHostSettings<DomainMessageCommit>
    {
        public EventStoreConsumerHostSettings()
        {
            this.SetMessageThreshold(100, 30);
            this.WithEventStorePipelinePerApplication();
            this.WithEventStoreEndpointPerBoundedContext();
        }
    }

    public static class ConsumerHostSettingsExtensions
    {
        public static T SetNumberOfConsumers<T>(this T self, int numberOfConsumers) where T : IConsumerHostSettings
        {
            self.NumberOfWorkers = numberOfConsumers;
            return self;
        }

        public static T SetMessageThreshold<T>(this T self, uint size, uint delay) where T : IConsumerHostSettings
        {
            self.MessageTreshold = new MessageThreshold(size, delay);
            return self;
        }

        public static T CommandConsumer<T>(this T self, Action<ConsumerSettings<ICommand>> configure = null)
            where T : IConsumerHostSettings<ICommand>
        {
            ConsumerSettings<ICommand> settings = new ConsumerSettings<ICommand>();
            self.CopySerializerTo(settings);
            settings.WithDefaultEndpointPostConsume(self);
            if (configure != null)
                configure(settings);
            self.Consumer = settings.GetInstanceLazy();
            return self;
        }

        public static T EventConsumer<T>(this T self, Action<ConsumerSettings<IEvent>> configure)
            where T : IConsumerHostSettings<IEvent>
        {
            ConsumerSettings<IEvent> settings = new ConsumerSettings<IEvent>();
            self.CopySerializerTo(settings);
            settings.WithDefaultEndpointPostConsume(self);
            if (configure != null)
                configure(settings);
            self.Consumer = settings.GetInstanceLazy();
            return self;
        }

        public static T EventStoreConsumer<T>(this T self, Action<ConsumerSettings<DomainMessageCommit>> configure)
            where T : IConsumerHostSettings<DomainMessageCommit>
        {
            ConsumerSettings<DomainMessageCommit> settings = new ConsumerSettings<DomainMessageCommit>();
            self.CopySerializerTo(settings);
            settings.WithDefaultEndpointPostConsume(self);
            if (configure != null)
                configure(settings);
            self.Consumer = settings.GetInstanceLazy();
            return self;
        }
    }
}