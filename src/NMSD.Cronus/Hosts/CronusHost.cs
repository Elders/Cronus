using System;
using System.Collections.Generic;
using System.Reflection;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Transport.InMemory;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Protoreg;

namespace NMSD.Cronus.Hosts
{
    public class CronusHost
    {
        private HashSet<Assembly> appServicesAssemblies = new HashSet<Assembly>();

        private List<CommandConsumerConfiguration> commandConsumerConfigurations = new List<CommandConsumerConfiguration>();

        private List<CommandConsumer> commandConsumers = new List<CommandConsumer>();

        private IEndpointFactory commandEndpointFactory;

        private IEndpointNameConvention commandHandlersEndpointConvention;

        private IPipelineNameConvention commandPipelineConvention;

        private IPipelineFactory<IPipeline> commandPipelineFactory;

        private CommandPublisherConfiguration commandPublisherConfiguration;

        private CommandPublisher commandPublisher;

        private List<EventConsumer> eventConsumers = new List<EventConsumer>();

        private IEndpointFactory eventEndpointFactory;

        private HashSet<Assembly> eventHandlersAssemblies = new HashSet<Assembly>();

        private List<EventHandlersConfiguration> eventHandlersConfigurations = new List<EventHandlersConfiguration>();

        private IEndpointNameConvention eventHandlersEnpointConvention;

        private IPipelineNameConvention eventHandlersPipelineConvention;

        private IPipelineFactory<IPipeline> eventPipelineFactory;

        private HashSet<Assembly> eventsAssemblies = new HashSet<Assembly>();

        private List<EventStoreConsumerConfiguration> eventStoreConsumerConfigurations = new List<EventStoreConsumerConfiguration>();

        private List<EventStoreConsumer> eventStoreConsumers = new List<EventStoreConsumer>();

        private IEndpointNameConvention eventStoreEndpointConvention;

        private IEndpointFactory eventStoreEndpointFactory;

        private IPipelineNameConvention eventStorePipelineConvention = new EventStorePipelinePerApplication();

        private IPipelineFactory<IPipeline> eventStorePipelineFactory;

        private RabbitMqSessionFactory rabbitMqSessionFactory;

        private RabbitMqSession session;

        public CronusHost()
        {
            log4net.Config.XmlConfigurator.Configure();
            this.ProtoRegistration = new ProtoRegistration(); ;
            this.Serializer = new ProtoregSerializer(ProtoRegistration);
            ProtoRegistration.RegisterAssembly<Wraper>();
        }

        public CommandPublisher CommandPublisher
        {
            get
            {
                if (commandPublisher == null)
                    throw new CronusConfigurationException("At least one commands assembly is required to use CommandPublisher. Example: 'RegisterCommandsAssembly(Assembly assembly)'");
                return commandPublisher;
            }
        }

        public ProtoRegistration ProtoRegistration { get; private set; }

        public ProtoregSerializer Serializer { get; private set; }

        public void BuildCommandPublisher()
        {
            if (commandPublisherConfiguration.CommandsAssemblies.Count <= 0)
                throw new CronusConfigurationException("At least one commands assembly is required. Example: 'RegisterCommandsAssembly(Assembly assembly)'.");

            commandPublisher = new CommandPublisher(commandPipelineFactory, Serializer);
        }

        public void BuildSerializer()
        {
            if (commandPublisherConfiguration != null)
                foreach (Assembly assembly in commandPublisherConfiguration.CommandsAssemblies)
                {
                    ProtoRegistration.RegisterAssembly(assembly);
                }
            foreach (var item in commandConsumerConfigurations)
            {
                ProtoRegistration.RegisterAssembly(item.CommandsAssembly);
                ProtoRegistration.RegisterAssembly(item.EventsAssembly);
                ProtoRegistration.RegisterAssembly(item.AggregateStatesAssembly);
            }
            foreach (var item in eventStoreConsumerConfigurations)
            {
                ProtoRegistration.RegisterAssembly(item.AssemblyContainingEventsByEventType);
                ProtoRegistration.RegisterAssembly(item.AggregateStatesAssembly);
            }
            foreach (var item in eventHandlersConfigurations)
            {
                foreach (Assembly eventsAssembly in item.EventsAssemblies)
                {
                    ProtoRegistration.RegisterAssembly(eventsAssembly);
                }

            }
            Serializer.Build();
        }

        public void ConfigureCommandConsumer(Action<CommandConsumerConfiguration> configure)
        {
            var commandConsumerConfiguration = new CommandConsumerConfiguration();
            configure(commandConsumerConfiguration);

            if (commandConsumerConfiguration.CommandsAssembly == null) throw new CronusConfigurationException("Commands assembly is required. Example: 'SetCommandsAssembly(Assembly assembly)'.");
            if (commandConsumerConfiguration.EventsAssembly == null) throw new CronusConfigurationException("Events assembly is required. Example: 'SetEventsAssembly(Assembly assembly)'.");
            if (commandConsumerConfiguration.AggregateStatesAssembly == null) throw new CronusConfigurationException("AggregateStates assembly is required. Example: 'SetAggregateStatesAssembly(Assembly assembly)'.");
            if (commandConsumerConfiguration.CommandHandlersAssembly == null) throw new CronusConfigurationException("CommandHandlers assembly is required. Example: 'SetCommandHandlersAssembly(Assembly assembly)'.");
            if (String.IsNullOrEmpty(commandConsumerConfiguration.EventStoreConnectionString)) throw new CronusConfigurationException("EventStoreConnectionString  is required. Example: 'SetEventStoreConnectionString(string connectionString)'.");

            commandConsumerConfigurations.Add(commandConsumerConfiguration);


            var boundedContext = commandConsumerConfiguration.EventsAssembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            var eventStorePublisher = new EventStorePublisher(eventStorePipelineFactory, Serializer);
            var es = new RabbitRepository(boundedContext, commandConsumerConfiguration.EventStoreConnectionString, eventStorePublisher, Serializer);
            //var es = new MssqlEventStore(boundedContext, commandConsumerConfiguration.EventStoreConnectionString, Serializer);

            var commandConsumer = new CommandConsumer(commandEndpointFactory, Serializer, es);
            commandConsumer.UnitOfWorkFactory = commandConsumerConfiguration.UnitOfWorkFacotry;
            commandConsumer.RegisterAllHandlersInAssembly(commandConsumerConfiguration.CommandHandlersAssembly);
            commandConsumers.Add(commandConsumer);
        }

        public void ConfigureCommandPublisher(Action<CommandPublisherConfiguration> configure)
        {
            commandPublisherConfiguration = new CommandPublisherConfiguration();
            configure(commandPublisherConfiguration);
        }

        public void ConfigureEventHandlersConsumer(Action<EventHandlersConfiguration> configure)
        {
            var cfg = new EventHandlersConfiguration();
            configure(cfg);

            if (cfg.EventsAssemblies.Count <= 0) throw new CronusConfigurationException("Events assembly is required. Example: 'SetEventsAssembly(Assembly assembly)'.");
            if (cfg.EventHandlersAssembly == null) throw new CronusConfigurationException("EventHandlersAssembly assembly is required. Example: 'SetEventHandlersAssembly(Assembly assembly)'.");
            if (eventHandlersEnpointConvention == null) throw new CronusConfigurationException("EventHandlersEnpointConvention is required. Example: 'UseEventHandlerPerEndpoint()'.");
            if (commandPublisher == null) throw new CronusConfigurationException("CommandPublisher is required. Example: 'BuildCommandPublisher()'.");

            eventHandlersConfigurations.Add(cfg);

            var eventConsumer = new EventConsumer(eventEndpointFactory, Serializer, commandPublisher);
            eventConsumer.UnitOfWorkFactory = cfg.UnitOfWorkFacotry;
            eventConsumer.RegisterAllHandlersInAssembly(cfg.EventHandlersAssembly);
            eventConsumers.Add(eventConsumer);
        }

        public void ConfigureEventStoreConsumer(Action<EventStoreConsumerConfiguration> configure)
        {
            var cfg = new EventStoreConsumerConfiguration();
            configure(cfg);

            if (cfg.AssemblyContainingEventsByEventType == null) throw new CronusConfigurationException("Events assembly is required. Example: 'SetEventsAssembly(Assembly assembly)'.");
            if (cfg.AggregateStatesAssembly == null) throw new CronusConfigurationException("AggregateStates assembly is required. Example: 'SetAggregateStatesAssembly(Assembly assembly)'.");
            if (String.IsNullOrEmpty(cfg.EventStoreConnectionString)) throw new CronusConfigurationException("EventStoreConnectionString  is required. Example: 'SetEventStoreConnectionString(string connectionString)'.");
            if (eventStoreEndpointConvention == null) throw new CronusConfigurationException("EventStoreEndpointConvention is required. Example: 'UseEventStoreEndpointConventionBoundedContext()'.");
            if (eventStorePipelineConvention == null) throw new CronusConfigurationException("EventStorePipelineConvention is required. Example: 'UseEventStorePipelineConventionPerApplication()'.");

            eventStoreConsumerConfigurations.Add(cfg);
            var boundedContext = cfg.AssemblyContainingEventsByEventType.Assembly.GetAssemblyAttribute<BoundedContextAttribute>().BoundedContextName;
            var es = new MssqlEventStore(boundedContext, cfg.EventStoreConnectionString, Serializer);

            var eventPublisher = new EventPublisher(eventPipelineFactory, Serializer);
            var eventStoreConsumer = new EventStoreConsumer(eventStoreEndpointFactory, cfg.AssemblyContainingEventsByEventType, Serializer, es, eventPublisher);
            eventStoreConsumer.UnitOfWorkFactory = cfg.UnitOfWorkFacotry;
            eventStoreConsumers.Add(eventStoreConsumer);

        }

        public void HostCommandConsumers(int consumerWorkers)
        {
            if (commandConsumers.Count == 0) throw new CronusConfigurationException("Configuration is required. Use: 'ConfigureCommandConsumer(Action<CommandConsumerConfiguration> configure)'");

            foreach (CommandConsumer commandCosumer in commandConsumers)
            {
                commandCosumer.Start(consumerWorkers);
            }
        }

        public void HostEventHandlerConsumers(int consumerWorkers)
        {
            if (eventConsumers.Count == 0) throw new CronusConfigurationException("Configuration is required. Use: 'ConfigureEventHandlersConsumer(Action<EventHandlersConfiguration> configure)'");

            foreach (var eHandlerConsumer in eventConsumers)
            {
                eHandlerConsumer.Start(consumerWorkers);
            }
        }

        public void HostEventStoreConsumers(int consumerWorkers)
        {
            if (eventStoreConsumers.Count == 0) throw new CronusConfigurationException("Configuration is required. Use: 'ConfigureEventStoreConsumer(Action<EventStoreConsumerConfiguration> configure)'");

            foreach (EventStoreConsumer esConsumer in eventStoreConsumers)
            {
                esConsumer.Start(consumerWorkers);
            }
        }

        public void Release()
        {
            foreach (CommandConsumer commandConsumer in commandConsumers)
            {
                commandConsumer.Stop();
            }
            foreach (EventConsumer eventConsumer in eventConsumers)
            {
                eventConsumer.Stop();
            }
            foreach (EventStoreConsumer eventStoreConsumer in eventStoreConsumers)
            {
                eventStoreConsumer.Stop();
            }
            session.Close();
        }

        public void UseCommandHandlersPerBoundedContext()
        {
            if (commandPipelineConvention == null)
                throw new CronusConfigurationException("CommandPipelineConvention is required. Example: 'UseCommandPipelinePerApplication()'.");
            commandHandlersEndpointConvention = new CommandHandlersPerBoundedContext(commandPipelineConvention);
        }

        public void UseCommandPipelinePerApplication()
        {
            commandPipelineConvention = new CommandPipelinePerApplication();
        }

        public void UseEventHandlerPerEndpoint()
        {
            if (eventHandlersPipelineConvention == null)
                throw new CronusConfigurationException("EventHandlersPipelineConvention is required. Example: 'UseEventHandlersPipelineConventionPerApplication()'.");
            this.eventHandlersEnpointConvention = new EventHandlerPerEndpoint(eventHandlersPipelineConvention);
        }

        public void UseEventHandlersPerBoundedContext()
        {
            if (eventHandlersPipelineConvention == null)
                throw new CronusConfigurationException("EventHandlersPipelineConvention is required. Example: 'UseEventHandlersPipelineConventionPerApplication()'.");
            this.eventHandlersEnpointConvention = new EventHandlersPerBoundedContext(eventHandlersPipelineConvention);
        }

        public void UseEventHandlersPipelinePerApplication()
        {
            eventHandlersPipelineConvention = new EventHandlersPipelinePerApplication();
        }

        public void UseEventStorePerBoundedContext()
        {
            if (eventStorePipelineConvention == null)
                throw new CronusConfigurationException("EventStorePipelineConvention is required. Example: 'UseEventStorePipelinePerApplication()'.");
            eventStoreEndpointConvention = new EventStorePerBoundedContext(eventStorePipelineConvention);
        }

        public void UseEventStorePipelinePerApplication()
        {
            eventStorePipelineConvention = new EventStorePipelinePerApplication();
        }

        public void UseRabbitMqTransport()
        {
            rabbitMqSessionFactory = new RabbitMqSessionFactory();
            session = rabbitMqSessionFactory.OpenSession();

            commandPipelineFactory = new RabbitMqPipelineFactory(session, commandPipelineConvention);
            commandEndpointFactory = new RabbitMqEndpointFactory(session, commandHandlersEndpointConvention, (RabbitMqPipelineFactory)commandPipelineFactory);

            eventStorePipelineFactory = new RabbitMqPipelineFactory(session, eventStorePipelineConvention);
            eventStoreEndpointFactory = new RabbitMqEndpointFactory(session, eventStoreEndpointConvention, (RabbitMqPipelineFactory)eventStorePipelineFactory);

            eventPipelineFactory = new RabbitMqPipelineFactory(session, eventHandlersPipelineConvention);
            eventEndpointFactory = new RabbitMqEndpointFactory(session, eventHandlersEnpointConvention, (RabbitMqPipelineFactory)eventPipelineFactory);
        }

        public void UseInMemoryTransport()
        {
            //rabbitMqSessionFactory = new RabbitMqSessionFactory();
            //session = rabbitMqSessionFactory.OpenSession();

            commandPipelineFactory = new InMemoryPipelineFactory(commandPipelineConvention);
            commandEndpointFactory = new InMemoryEndpointFactory(commandPipelineFactory as InMemoryPipelineFactory, commandHandlersEndpointConvention);

            eventStorePipelineFactory = new InMemoryPipelineFactory(eventStorePipelineConvention);
            eventStoreEndpointFactory = new InMemoryEndpointFactory(eventStorePipelineFactory as InMemoryPipelineFactory, eventStoreEndpointConvention);

            eventPipelineFactory = new InMemoryPipelineFactory(eventHandlersPipelineConvention);
            eventEndpointFactory = new InMemoryEndpointFactory(eventPipelineFactory as InMemoryPipelineFactory, eventHandlersEnpointConvention);
        }

    }
}