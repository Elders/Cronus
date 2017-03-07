using System;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Config;

namespace Elders.Cronus.Pipeline.Hosts
{
    public interface ICronusSettings : ICanConfigureSerializer, ISettingsBuilder { }

    public class CronusSettings : ICronusSettings
    {
        public CronusSettings(IContainer container)
        {
            // TODO: This is temporary instantiated here. We need to think where is the best place to set the default EventStreamIntegrityPolicy.
            container.RegisterSingleton<IntegrityValidation.IIntegrityPolicy<EventStore.EventStream>>(() => new EventStore.EventStreamIntegrityPolicy());

            (this as ISettingsBuilder).Container = container;
        }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        void ISettingsBuilder.Build()
        {
            var builder = this as ISettingsBuilder;
            var consumers = builder.Container.ResolveAll<ICronusConsumer>();
            CronusHost host = new CronusHost();
            host.Consumers = consumers;
            builder.Container.RegisterSingleton(typeof(CronusHost), () => host);
        }
    }
}

public static class CronusConfigurationExtensions
{
    public static T UsePipelineEventPublisher<T>(this T self, Action<EventPipelinePublisherSettings> configure = null)
        where T : IConsumerSettings
    {
        return UsePipelineEventPublisher(self, null, configure);
    }

    public static T UsePipelineEventPublisher<T>(this T self, string name,
        Action<EventPipelinePublisherSettings> configure = null)
        where T : IConsumerSettings
    {
        EventPipelinePublisherSettings settings = new EventPipelinePublisherSettings(self, name);
        if (configure != null)
            configure(settings);
        (settings as ISettingsBuilder).Build();
        return self;
    }

    public static T UsePipelineCommandPublisher<T>(this T self, Action<CommandPipelinePublisherSettings> configure = null)
        where T : IConsumerSettings
    {
        return UsePipelineCommandPublisher(self, null, configure);
    }

    public static T UsePipelineCommandPublisher<T>(this T self, string name,
        Action<CommandPipelinePublisherSettings> configure = null)
        where T : IConsumerSettings
    {
        CommandPipelinePublisherSettings settings = new CommandPipelinePublisherSettings(self, name);
        if (configure != null)
            configure(settings);
        (settings as ISettingsBuilder).Build();
        return self;
    }

    public static T UsePipelineSagaPublisher<T>(this T self, Action<SagaPipelinePublisherSettings> configure = null)
        where T : IConsumerSettings
    {
        return UsePipelineSagaPublisher(self, null, configure);
    }

    public static T UsePipelineSagaPublisher<T>(this T self, string name,
        Action<SagaPipelinePublisherSettings> configure = null)
        where T : IConsumerSettings
    {
        SagaPipelinePublisherSettings settings = new SagaPipelinePublisherSettings(self, name);
        if (configure != null)
            configure(settings);
        (settings as ISettingsBuilder).Build();
        return self;
    }

    public static T WithDefaultPublishers<T>(this T self) where T : IConsumerSettings
    {
        self
            .UsePipelineEventPublisher()
            .UsePipelineCommandPublisher()
            .UsePipelineSagaPublisher();
        return self;
    }
}
