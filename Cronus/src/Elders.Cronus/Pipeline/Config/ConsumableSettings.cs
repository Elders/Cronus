using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Transport;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class ConsumableSettings<TContract> : IConsumableSettings<TContract>
        where TContract : IMessage
    {
        Lazy<IConsumer<TContract>> IHaveConsumer<TContract>.Consumer { get; set; }

        int IConsumableSettings.NumberOfWorkers { get; set; }

        Lazy<IPipelineTransport> IHaveTransport<IPipelineTransport>.Transport { get; set; }

        Lazy<IEndpointConsumable> ISettingsBuilder<IEndpointConsumable>.Build()
        {
            IConsumableSettings<TContract> settings = this as IConsumableSettings<TContract>;
            return new Lazy<IEndpointConsumable>(() => new EndpointConsumable<TContract>(settings.Transport.Value.EndpointFactory, settings.Consumer.Value, settings.Serializer, 100));
        }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        ProtoregSerializer IHaveSerializer.Serializer { get; set; }
    }

    public class CommandConsumableSettings : ConsumableSettings<ICommand>
    {
        public CommandConsumableSettings()
        {
            this.WithCommandPipelinePerApplication();
            this.WithCommandHandlerEndpointPerBoundedContext();
        }
    }

    public class ProjectionConsumableSettings : ConsumableSettings<IEvent>
    {
        public ProjectionConsumableSettings()
        {
            this.WithEventPipelinePerApplication();
            this.WithProjectionEndpointPerBoundedContext();
        }
    }

    public class PortConsumableSettings : ConsumableSettings<IEvent>
    {
        public PortConsumableSettings()
        {
            this.WithEventPipelinePerApplication();
            this.WithPortEndpointPerBoundedContext();
        }
    }

    public class EventStoreConsumableSettings : ConsumableSettings<DomainMessageCommit>
    {
        public EventStoreConsumableSettings()
        {
            this.WithEventStorePipelinePerApplication();
            this.WithEventStoreEndpointPerBoundedContext();
        }
    }

    public static class ConsumableSettingsExtensions
    {
        public static T SetNumberOfConsumers<T>(this T self, int numberOfConsumers) where T : IConsumableSettings
        {
            self.NumberOfWorkers = numberOfConsumers;
            return self;
        }

        public static T CommandConsumer<T>(this T self, Action<ConsumerSettings<ICommand>> configure = null)
            where T : IConsumableSettings<ICommand>
        {
            ConsumerSettings<ICommand> settings = new ConsumerSettings<ICommand>();
            settings.WithDefaultEndpointPostConsume(self);
            if (configure != null)
                configure(settings);
            self.Consumer = settings.GetInstanceLazy();
            return self;
        }

        public static T EventConsumer<T>(this T self, Action<ConsumerSettings<IEvent>> configure)
            where T : IConsumableSettings<IEvent>
        {
            ConsumerSettings<IEvent> settings = new ConsumerSettings<IEvent>();
            settings.WithDefaultEndpointPostConsume(self);
            if (configure != null)
                configure(settings);
            self.Consumer = settings.GetInstanceLazy();
            return self;
        }

        public static T EventStoreConsumer<T>(this T self, Action<ConsumerSettings<DomainMessageCommit>> configure)
            where T : IConsumableSettings<DomainMessageCommit>
        {
            ConsumerSettings<DomainMessageCommit> settings = new ConsumerSettings<DomainMessageCommit>();
            settings.WithDefaultEndpointPostConsume(self);
            if (configure != null)
                configure(settings);
            self.Consumer = settings.GetInstanceLazy();
            return self;
        }

        public static T SetConsumerBatchSize<T>(this T self, int consumerBatchSize) where T : IMessageProcessorWithSafeBatchSettings<IMessage>
        {
            self.ConsumerBatchSize = consumerBatchSize;
            return self;
        }

    }
}