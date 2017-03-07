using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Serializer;
using Elders.Cronus.EventStore;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.IntegrityValidation;
using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class PipelineConsumerSettings<TContract> : SettingsBuilder, IConsumerSettings<TContract>
        where TContract : IMessage
    {
        public PipelineConsumerSettings(ISettingsBuilder settingsBuilder, string name)
            : base(settingsBuilder, name)
        {
            this.SetNumberOfConsumerThreads(2);
        }

        int IConsumerSettings.NumberOfWorkers { get; set; }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            Func<ITransport> transport = () => builder.Container.Resolve<ITransport>(builder.Name);
            Func<ISerializer> serializer = () => builder.Container.Resolve<ISerializer>();
            Func<SubscriptionMiddleware> messageHandlerProcessor = () => builder.Container.Resolve<SubscriptionMiddleware>(builder.Name);
            Func<ICronusConsumer> consumer = () => new CronusConsumer((this as IConsumerSettings<TContract>).Name, transport(), messageHandlerProcessor(), serializer());
            builder.Container.RegisterSingleton<ICronusConsumer>(() => consumer(), builder.Name);
        }
    }

    public class CommandConsumerSettings : PipelineConsumerSettings<ICommand>
    {
        public CommandConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }

        public override void Build()
        {
            base.Build();

            var builder = this as ISettingsBuilder;
            Func<IAggregateRootAtomicAction> atomicAction = () => builder.Container.Resolve<IAggregateRootAtomicAction>();
            Func<IIntegrityPolicy<EventStream>> eventStreamIntegrityPolicy = () => builder.Container.Resolve<IIntegrityPolicy<EventStream>>();
            Func<IAggregateRepository> aggregateRepository = () => new AggregateRepository(builder.Container.Resolve<IEventStore>(builder.Name), atomicAction(), eventStreamIntegrityPolicy());
            builder.Container.RegisterSingleton<IAggregateRepository>(() => aggregateRepository(), builder.Name);
        }
    }

    public class ProjectionConsumerSettings : PipelineConsumerSettings<IEvent>
    {
        public ProjectionConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class PortConsumerSettings : PipelineConsumerSettings<IEvent>
    {
        public PortConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class SagaConsumerSettings : PipelineConsumerSettings<IEvent>
    {
        public SagaConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public static class ConsumerSettingsExtensions
    {
        public static T SetNumberOfConsumerThreads<T>(this T self, int numberOfConsumers) where T : IConsumerSettings
        {
            self.NumberOfWorkers = numberOfConsumers;
            return self;
        }

        public static T UseCommandConsumer<T>(this T self, Action<CommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseCommandConsumer(self, "AppServices", configure);
        }

        public static T UseCommandConsumer<T>(this T self, string name, Action<CommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            CommandConsumerSettings settings = new CommandConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseProjectionConsumer<T>(this T self, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseProjectionConsumer(self, "Projections", configure);
        }

        public static T UseProjectionConsumer<T>(this T self, string name, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            ProjectionConsumerSettings settings = new ProjectionConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePortConsumer<T>(this T self, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UsePortConsumer(self, "Ports", configure);
        }

        public static T UsePortConsumer<T>(this T self, string name, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            PortConsumerSettings settings = new PortConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseSagaConsumer<T>(this T self, Action<SagaConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseSagaConsumer(self, "Sagas", configure);
        }

        public static T UseSagaConsumer<T>(this T self, string name, Action<SagaConsumerSettings> configure = null) where T : ICronusSettings
        {
            SagaConsumerSettings settings = new SagaConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
