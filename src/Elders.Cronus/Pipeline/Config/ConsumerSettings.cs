using System;
using System.Reflection;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport;
using Elders.Cronus.Serializer;
using Elders.Cronus.UnitOfWork;
namespace Elders.Cronus.Pipeline.Config
{
    public abstract class ConsumerSettings<TContract> : HideObectMembers, IConsumerSettings<TContract>
        where TContract : IMessage
    {
        Lazy<IMessageProcessor<TContract>> IHaveMessageProcessor<TContract>.MessageHandlerProcessor { get; set; }

        int IConsumerSettings.NumberOfWorkers { get; set; }

        Lazy<IPipelineTransport> IHaveTransport<IPipelineTransport>.Transport { get; set; }

        Lazy<IEndpointConsumer> ISettingsBuilder<IEndpointConsumer>.Build()
        {
            IConsumerSettings<TContract> settings = this as IConsumerSettings<TContract>;
            return new Lazy<IEndpointConsumer>(() => new EndpointConsumer<TContract>(settings.Transport.Value, settings.MessageHandlerProcessor.Value, settings.Serializer, settings.MessageTreshold, settings.CircuitBreakerFactory.Value));
        }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        ISerializer IHaveSerializer.Serializer { get; set; }

        MessageThreshold IConsumerSettings.MessageTreshold { get; set; }

        Lazy<CircuitBreaker.IEndpontCircuitBreakerFactrory> IHaveCircuitBreaker.CircuitBreakerFactory { get; set; }
    }

    public class CommandConsumerSettings : ConsumerSettings<ICommand>
    {
        public CommandConsumerSettings()
        {
            this.WithCommandPipelinePerApplication();
            this.WithCommandHandlerEndpointPerBoundedContext();
        }
    }

    public class ProjectionConsumerSettings : ConsumerSettings<IEvent>
    {
        public ProjectionConsumerSettings()
        {
            this.WithEventPipelinePerApplication();
            this.WithProjectionEndpointPerBoundedContext();
        }
    }

    public class PortConsumerSettings : ConsumerSettings<IEvent>
    {
        public PortConsumerSettings()
        {
            this.WithEventPipelinePerApplication();
            this.WithPortEndpointPerBoundedContext();
        }
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
            CommandConsumerSettings settings = new CommandConsumerSettings();
            self.CopySerializerTo(settings);
            settings
                .SetNumberOfConsumerThreads(2)
                .WithDefaultCircuitBreaker();
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UseProjectionConsumer<T>(this T self, Action<ProjectionConsumerSettings> configure = null) where T : ICronusSettings
        {
            ProjectionConsumerSettings settings = new ProjectionConsumerSettings();
            self.CopySerializerTo(settings);
            settings.WithDefaultCircuitBreaker();
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }

        public static T UsePortConsumer<T>(this T self, Action<PortConsumerSettings> configure = null) where T : ICronusSettings
        {
            PortConsumerSettings settings = new PortConsumerSettings();
            self.CopySerializerTo(settings);
            settings.WithDefaultCircuitBreaker();
            if (configure != null)
                configure(settings);
            self.Consumers.Add(settings.GetInstanceLazy());
            return self;
        }
        public static T WithProjections<T>(this T self, Assembly asselbyContaintingPorts, Func<Type, Context, object> messageHandlerFactory = null, UnitOfWorkFactory unitOfWorkFactory = null) where T : ProjectionConsumerSettings, IHaveMessageProcessor<IEvent>
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


        public static T WithPorts<T>(this T self, Assembly asselbyContaintingPorts, Func<Type, Context, object> messageHandlerFactory = null, UnitOfWorkFactory unitOfWorkFactory = null) where T : PortConsumerSettings, IHaveMessageProcessor<IEvent>
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

        public static T WithApplicationServices<T>(this T self, Assembly asselbyContaintingPorts, Func<Type, Context, object> messageHandlerFactory = null, UnitOfWorkFactory unitOfWorkFactory = null) where T : CommandConsumerSettings, IHaveMessageProcessor<ICommand>
        {
            if (messageHandlerFactory == null)
                messageHandlerFactory = (x, y) => FastActivator.CreateInstance(x);
            if (unitOfWorkFactory == null)
                unitOfWorkFactory = new UnitOfWorkFactory();
            self.UseApplicationServices(configure => configure
              .UseUnitOfWork(unitOfWorkFactory)
            .RegisterAllHandlersInAssembly(asselbyContaintingPorts, messageHandlerFactory)
             );
            return self;
        }
    }
}