using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using NHibernate;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Persistence.MSSQL.Config;
using NMSD.Cronus.Sample.Collaboration;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;
using NMSD.Cronus.Sample.InMemoryServer.Nhibernate;
using NMSD.Cronus.Pipeline.InMemory.Config;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Pipeline.Transport.Config;
using NMSD.Cronus.Pipeline.Host.Config;
using NMSD.Cronus.Sample.CommonFiles;

namespace NMSD.Cronus.Sample.InMemoryServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            ISessionFactory nhSessionFactory = BuildNHibernateSessionFactory();

            var cfg = new CronusConfiguration();
            cfg.PipelineCommandPublisher(publisher =>
                {
                    publisher.UseTransport<InMemoryTransportSettings>();
                    publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
                })
                .ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
                {
                    eventStore
                        .SetConnectionStringName("cronus-es")
                        .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)))
                        .CreateStorage();
                })
                .ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
                {
                    eventStore
                        .SetConnectionStringName("cronus-es")
                        .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(AccountState)))
                        .CreateStorage();
                })
                .PipelineEventPublisher(publisher =>
                {
                    publisher.UseTransport<InMemoryTransportSettings>();
                    publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
                })
                .PipelineEventStorePublisher(publisher =>
                {
                    publisher.UseTransport<InMemoryTransportSettings>();
                })
                .ConfigureConsumer<EndpointEventStoreConsumableSettings>("Collaboration", consumer =>
                {
                    consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
                    consumer.UseTransport<InMemoryTransportSettings>();
                })
                .ConfigureConsumer<EndpointCommandConsumableSettings>("Collaboration", consumer =>
                {
                    consumer.ScopeFactory.CreateHandlerScope = () => new NHibernateHandlerScope(nhSessionFactory);
                    consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                        {
                            var handler = FastActivator.CreateInstance(type, null);
                            var repositoryHandler = handler as IAggregateRootApplicationService;
                            if (repositoryHandler != null)
                            {
                                repositoryHandler.Repository = new RabbitRepository((IAggregateRepository)cfg.GlobalSettings.EventStores.Single(es => es.BoundedContext == "Collaboration"), cfg.GlobalSettings.EventStorePublisher);
                            }
                            return handler;
                        });
                    consumer.UseTransport<InMemoryTransportSettings>();
                })
                .ConfigureConsumer<EndpointEventConsumableSettings>("Collaboration", consumer =>
                {
                    consumer.ScopeFactory.CreateHandlerScope = () => new NHibernateHandlerScope(nhSessionFactory);
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
                    consumer.UseTransport<InMemoryTransportSettings>();
                })
                .Build();

            var host = new CronusHost(cfg);
            host.Start();

            Thread.Sleep(2000);

            host.Stop();

           // HostUI(cfg.GlobalSettings.CommandPublisher, 1000, 1);
            Console.WriteLine("Started");
            //Console.ReadLine();
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

        static ISessionFactory BuildNHibernateSessionFactory()
        {
            var typesThatShouldBeMapped = Assembly.GetAssembly(typeof(UserProjection)).GetExportedTypes().Where(t => t.Namespace.EndsWith("DTOs"));
            var cfg = new NHibernate.Cfg.Configuration()
                .AddAutoMappings(typesThatShouldBeMapped)
                .Configure()
                .CreateDatabase();

            return cfg.BuildSessionFactory();
        }
    }
}