using System;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;

namespace Elders.Cronus.InMemory.Config
{
    public class InMemoryConsumerSettings<TContract> : SettingsBuilder, IConsumerSettings<TContract> where TContract : IMessage
    {
        public InMemoryConsumerSettings(ISettingsBuilder settingsBuilder, string name)
            : base(settingsBuilder, name)
        { }

        int IConsumerSettings.NumberOfWorkers { get; set; }

        MessageThreshold IConsumerSettings.MessageTreshold { get; set; }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            Func<IMessageProcessor> messageHandlerProcessor = () => builder.Container.Resolve<IMessageProcessor>(builder.Name);
            Func<IPublisher<TContract>> consumer = () => new InMemoryPublisher<TContract>(messageHandlerProcessor());
            builder.Container.RegisterSingleton<IPublisher<TContract>>(() => consumer(), builder.Name);
        }
    }

    public class InMemoryCommandConsumerSettings : InMemoryConsumerSettings<ICommand>
    {
        public InMemoryCommandConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class InMemoryProjectionConsumerSettings : InMemoryConsumerSettings<IEvent>
    {
        public InMemoryProjectionConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public class InMemoryPortConsumerSettings : InMemoryConsumerSettings<IEvent>
    {
        public InMemoryPortConsumerSettings(ISettingsBuilder settingsBuilder, string name) : base(settingsBuilder, name) { }
    }

    public static class InMemoryConsumerSettingsExtensions
    {
        public static T UseInMemoryCommandConsumer<T>(this T self, Action<InMemoryCommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseInMemoryCommandConsumer(self, null, configure);
        }

        public static T UseInMemoryCommandConsumer<T>(this T self, string name, Action<InMemoryCommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            InMemoryCommandConsumerSettings settings = new InMemoryCommandConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseInMemoryProjectionConsumer<T>(this T self, Action<InMemoryProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseInMemoryProjectionConsumer(self, null, configure);
        }

        public static T UseInMemoryProjectionConsumer<T>(this T self, string name, Action<InMemoryProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            InMemoryProjectionConsumerSettings settings = new InMemoryProjectionConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseInMemoryPortConsumer<T>(this T self, Action<InMemoryPortConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseInMemoryPortConsumer(self, null, configure);
        }

        public static T UseInMemoryPortConsumer<T>(this T self, string name, Action<InMemoryPortConsumerSettings> configure = null) where T : ICronusSettings
        {
            InMemoryPortConsumerSettings settings = new InMemoryPortConsumerSettings(self, name);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }
    }
}
