using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Pipelining.Hosts.Config;
using NMSD.Cronus.Pipelining.Strategy;

namespace NMSD.Cronus.Pipelining.Config
{
    public interface IConsumerSettings<TTransport> where TTransport : TransportSettings
    {
        CronusGlobalSettings GlobalSettings { get; set; }

        Assembly[] MessagesAssemblies { get; set; }

        TTransport Transport { get; set; }

        IEndpointConsumable Build();

        string BoundedContext { get; set; }

        ScopeFactory ScopeFactory { get; set; }

        int NumberOfWorkers { get; set; }

        void UseTransport<T>(Action<T> configure) where T : TTransport;

        void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory);
    }

    public abstract class ConsumerSettings<TContract, TTransport> : IConsumerSettings<TTransport>
        where TContract : IMessage
        where TTransport : TransportSettings
    {
        protected Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>> registrations = new Dictionary<Type, List<Tuple<Type, Func<Type, Context, object>>>>();

        public ConsumerSettings()
        {
            ScopeFactory = new ScopeFactory();
            NumberOfWorkers = 1;
        }

        public string BoundedContext { get; set; }

        public CronusGlobalSettings GlobalSettings { get; set; }

        public Assembly[] MessagesAssemblies { get; set; }

        public int NumberOfWorkers { get; set; }

        public ScopeFactory ScopeFactory { get; set; }

        public TTransport Transport { get; set; }

        public void AddRegistration(Type messageType, Type messageHandlerType, Func<Type, Context, object> messageHandlerFactory)
        {
            if (!registrations.ContainsKey(messageType))
            {
                registrations.Add(messageType, new List<Tuple<Type, Func<Type, Context, object>>>());
            }
            registrations[messageType].Add(new Tuple<Type, Func<Type, Context, object>>(messageHandlerType, messageHandlerFactory));
        }

        public abstract IEndpointConsumable Build();

        public void UseTransport<T>(Action<T> configure = null) where T : TTransport
        {
            T transport = Activator.CreateInstance<T>();
            if (configure != null)
                configure(transport);
            Transport = transport;
        }

        protected abstract IEndpointConsumable BuildConsumer();

    }

    public interface IEndpointConsumerSetting : IConsumerSettings<PipelineTransportSettings>
    {
        PipelineSettings PipelineSettings { get; set; }
    }
    public abstract class EndpointConsumerSetting<TContract> : ConsumerSettings<TContract, PipelineTransportSettings>, IEndpointConsumerSetting where TContract : IMessage
    {
        public EndpointConsumerSetting()
        {
            PipelineSettings = new PipelineSettings();
        }

        public PipelineSettings PipelineSettings { get; set; }

        public override IEndpointConsumable Build()
        {
            Transport.Build(PipelineSettings);
            if (MessagesAssemblies != null)
                MessagesAssemblies.ToList().ForEach(ass => GlobalSettings.Protoreg.RegisterAssembly(ass));
            return BuildConsumer();
        }
    }

    public class EndpointCommandConsumableSettings : EndpointConsumerSetting<ICommand>
    {

        public EndpointCommandConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new CommandPipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new CommandHandlerEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            MessageHandlerCollection<ICommand> handlers = new MessageHandlerCollection<ICommand>();
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterCommonType(reg.Key);
                foreach (var item in reg.Value)
                {
                    handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<ICommand>(handlers, ScopeFactory, GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }

    public class EndpointEventStoreConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointEventStoreConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventStorePipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new EventStoreEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            var consumer = new EndpointEventStoreConsumer(GlobalSettings.EventStores.Single(es => es.BoundedContext == BoundedContext), GlobalSettings.EventPublisher, GlobalSettings.CommandPublisher, MessagesAssemblies.First().ExportedTypes.First(), GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }

    public class EndpointEventConsumableSettings : EndpointConsumerSetting<IEvent>
    {
        public EndpointEventConsumableSettings()
        {
            PipelineSettings.PipelineNameConvention = new EventPipelinePerApplication();
            PipelineSettings.EndpointNameConvention = new EventHandlerEndpointPerBoundedContext(PipelineSettings.PipelineNameConvention);
        }

        protected override IEndpointConsumable BuildConsumer()
        {
            MessageHandlerCollection<IEvent> handlers = new MessageHandlerCollection<IEvent>();
            foreach (var reg in registrations)
            {
                GlobalSettings.Protoreg.RegisterCommonType(reg.Key);
                foreach (var item in reg.Value)
                {
                    handlers.RegisterHandler(reg.Key, item.Item1, item.Item2);
                }
            }

            var consumer = new EndpointConsumer<IEvent>(handlers, ScopeFactory, GlobalSettings.Serializer);
            return new EndpointConsumable(Transport.EndpointFactory, consumer);
        }
    }
}
