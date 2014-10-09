using System;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.IocContainer;
using Elders.Cronus.Pipeline.CircuitBreaker;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;
using Elders.Cronus.UnitOfWork;

namespace Elders.Cronus.Pipeline.Config
{
    public abstract class ConsumerSettings<TContract> : SettingsBuilder, IConsumerSettings<TContract>
        where TContract : IMessage
    {
        public ConsumerSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder)
        {
            this.WithDefaultCircuitBreaker();
            this.SetNumberOfConsumerThreads(2);
        }

        int IConsumerSettings.NumberOfWorkers { get; set; }

        MessageThreshold IConsumerSettings.MessageTreshold { get; set; }

        IContainer ISettingsBuilder.Container { get; set; }

        string ISettingsBuilder.Name { get; set; }

        public override void Build()
        {
            var builder = this as ISettingsBuilder;
            Func<IPipelineTransport> transport = () => builder.Container.Resolve<IPipelineTransport>(builder.Name);
            Func<ISerializer> serializer = () => builder.Container.Resolve<ISerializer>();
            Func<IMessageProcessor<TContract>> messageHandlerProcessor = () => builder.Container.Resolve<IMessageProcessor<TContract>>(builder.Name);
            Func<IEndpontCircuitBreakerFactrory> endpointCircuitBreaker = () => builder.Container.Resolve<IEndpontCircuitBreakerFactrory>(builder.Name);
            Func<IEndpointConsumer> consumer = () => new EndpointConsumer<TContract>(transport(), messageHandlerProcessor(), serializer(), (this as IConsumerSettings<TContract>).MessageTreshold, endpointCircuitBreaker());
            builder.Container.RegisterSingleton<IEndpointConsumer>(() => consumer(), builder.Name);
        }
    }

    public class CommandConsumerSettings : ConsumerSettings<ICommand>
    {
        public CommandConsumerSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder) { }
    }

    public class ProjectionConsumerSettings : ConsumerSettings<IEvent>
    {
        public ProjectionConsumerSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder) { }
    }

    public class PortConsumerSettings : ConsumerSettings<IEvent>
    {
        public PortConsumerSettings(ISettingsBuilder settingsBuilder) : base(settingsBuilder) { }
    }

    public static class ConsumerSettingsExtensions
    {
        public static T SetNumberOfConsumerThreads<T>(this T self, int numberOfConsumers) where T : IConsumerSettings
        {
            self.NumberOfWorkers = numberOfConsumers;
            return self;
        }

        public static T SetMessageThreshold<T>(this T self, uint size, uint delay) where T : IConsumerSettings
        {
            self.MessageTreshold = new MessageThreshold(size, delay);
            return self;
        }

        public static T UseCommandConsumer<T>(this T self, Action<CommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseCommandConsumer(self, null, configure);
        }

        public static T UseCommandConsumer<T>(this T self, string name, Action<CommandConsumerSettings> configure = null) where T : ICronusSettings
        {
            CommandConsumerSettings settings = new CommandConsumerSettings(self);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UseProjectionConsumer<T>(this T self, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UseProjectionConsumer(self, null, configure);
        }

        public static T UseProjectionConsumer<T>(this T self, string name, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            ProjectionConsumerSettings settings = new ProjectionConsumerSettings(self);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T UsePortConsumer<T>(this T self, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            return UsePortConsumer(self, null, configure);
        }

        public static T UsePortConsumer<T>(this T self, string name, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            PortConsumerSettings settings = new PortConsumerSettings(self);
            if (configure != null)
                configure(settings);
            (settings as ISettingsBuilder).Build();
            return self;
        }

        public static T WithProjections<T>(this T self, Assembly asselbyContaintingPorts, Func<Type, Context, object> messageHandlerFactory = null, UnitOfWorkFactory unitOfWorkFactory = null) where T : ProjectionConsumerSettings, IMessageProcessorSettings<IEvent>
        {
            if (messageHandlerFactory == null)
                messageHandlerFactory = (x, y) => FastActivator.CreateInstance(x);
            if (unitOfWorkFactory == null)
                unitOfWorkFactory = new UnitOfWorkFactory();
            self.UseProjections(configure => configure
                .UseUnitOfWork(unitOfWorkFactory)
                .RegisterAllHandlersInAssembly(asselbyContaintingPorts, messageHandlerFactory)
            );

            return self;
        }

        public static T WithPorts<T>(this T self, Assembly asselbyContaintingPorts, Func<Type, Context, object> messageHandlerFactory = null, UnitOfWorkFactory unitOfWorkFactory = null) where T : PortConsumerSettings, IMessageProcessorSettings<IEvent>
        {
            if (messageHandlerFactory == null)
                messageHandlerFactory = (x, y) => FastActivator.CreateInstance(x);
            if (unitOfWorkFactory == null)
                unitOfWorkFactory = new UnitOfWorkFactory();
            self.UsePorts(configure => configure
              .UseUnitOfWork(unitOfWorkFactory)
              .RegisterAllHandlersInAssembly(asselbyContaintingPorts, messageHandlerFactory)
            );
            return self;
        }

        public static T WithApplicationServices<T>(this T self, Assembly asselbyContaintingPorts, Func<Type, Context, object> messageHandlerFactory = null, UnitOfWorkFactory unitOfWorkFactory = null) where T : CommandConsumerSettings, IMessageProcessorSettings<ICommand>
        {
            if (messageHandlerFactory == null)
                messageHandlerFactory = (x, y) => FastActivator.CreateInstance(x);
            if (unitOfWorkFactory == null)
                unitOfWorkFactory = new UnitOfWorkFactory();
            self.UseApplicationServices(configure => configure
                .UseUnitOfWork(unitOfWorkFactory)
                .RegisterAllHandlersInAssembly(asselbyContaintingPorts, messageHandlerFactory));
            return self;
        }
    }
}