using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Sample.Collaboration;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.InMemoryServer.Nhibernate;
using NMSD.Cronus.Sample.Player;
using NMSD.Cronus.Pipelining.InMemory.Config;

namespace NMSD.Cronus.Sample.InMemoryServer
{
    class Program
    {

        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var sf = BuildSessionFactory();
            var cfg = new CronusConfiguration();
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = "Collaboration";
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(UserState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>("Collaboration", publisher =>
            {
                publisher.InMemory();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigurePublisher<PipelinePublisher<IEvent>>("Collaboration", publisher =>
            {
                publisher.InMemory();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigurePublisher<PipelinePublisher<DomainMessageCommit>>("Collaboration", publisher =>
            {
                publisher.InMemory();
            });
            cfg.ConfigureEventStoreConsumer<EndpointEventStoreConsumer>("Collaboration", consumer =>
            {
                consumer.AssemblyEventsWhichWillBeIntercepted = typeof(CreateUser);
                consumer.InMemory();
            });
            cfg.ConfigureConsumer<EndpointConsumer<ICommand>>("Collaboration", consumer =>
            {
                consumer.ScopeFactory = new ScopeFactory();
                consumer.ScopeFactory.CreateHandlerScope = () => new NHibernateHandlerScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = new RabbitRepository((IAggregateRepository)cfg.eventStores["Collaboration"], cfg.publishers["Collaboration"][typeof(DomainMessageCommit)]);
                        }
                        return handler;
                    });
                consumer.InMemory();
            });
            cfg.ConfigureConsumer<EndpointConsumer<IEvent>>("Collaboration", consumer =>
            {
                consumer.ScopeFactory = new ScopeFactory();
                consumer.ScopeFactory.CreateHandlerScope = () => new NHibernateHandlerScope(sf);
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserProjection)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var nhHandler = handler as IHaveNhibernateSession;
                        if (nhHandler != null)
                        {
                            nhHandler.Session = context.HandlerScopeContext.Get<ISession>();
                        }
                        return handler;
                    });
                consumer.InMemory();
            });
            cfg.Start();

            HostUI(cfg.publishers["Collaboration"][typeof(ICommand)], 1000, 1);
            Console.WriteLine("Started");
            Console.ReadLine();
        }

        private static void HostUI(IPublisher commandPublisher, int messageDelayInMilliseconds = 0, int batchSize = 1)
        {

            for (int i = 0; i > -1; i++)
            {
                if (messageDelayInMilliseconds == 0)
                {
                    PublishCommands(commandPublisher);
                }
                else
                {
                    for (int j = 0; j < batchSize; j++)
                    {
                        PublishCommands(commandPublisher);
                    }

                    Thread.Sleep(messageDelayInMilliseconds);
                }
            }
        }

        private static void PublishCommands(IPublisher commandPublisher)
        {
            UserId collaboratorId = new UserId(Guid.NewGuid());
            var email = "mynkow@gmail.com";
            commandPublisher.Publish(new CreateUser(collaboratorId, email));
            //Thread.Sleep(1000);

            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail@gmail.com"));
        }


        static ISessionFactory BuildSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration();
            cfg = cfg.AddAutoMappings(typesThatShouldBeMapped);
            cfg.CreateDatabaseTables();
            return cfg.BuildSessionFactory();
        }
    }
}