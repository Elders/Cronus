using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Persistence.MSSQL.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.InMemory.Config;
using Elders.Cronus.Sample.Collaboration;
using Elders.Cronus.Sample.Collaboration.Projections;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;
using Elders.Cronus.Sample.InMemoryServer.Nhibernate;
using NHibernate;
using Elders.Cronus.Sample.CommonFiles;

namespace Elders.Cronus.Sample.InMemoryServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            ISessionFactory nhSessionFactory = BuildNHibernateSessionFactory();

            var cfg = new CronusConfiguration();
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
                {
                    eventStore
                        .SetConnectionStringName("cronus_es")
                        .SetDomainEventsAssembly(typeof(UserCreated))
                        .SetAggregateStatesAssembly(typeof(UserState))
                        .CreateStorage();
                })
                .ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
                {
                    eventStore
                        .SetConnectionStringName("cronus_es")
                        .SetDomainEventsAssembly(typeof(AccountRegistered))
                        .SetAggregateStatesAssembly(typeof(AccountState))
                        .CreateStorage();
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
                                repositoryHandler.Repository = cfg.GlobalSettings.AggregateRepositories["Collaboration"];
                            }
                            return handler;
                        });
                    //consumer.UseTransport<InMemory>();
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

        private static void HostUI(IPublisher<ICommand> commandPublisher, int messageDelayInMilliseconds = 0, int batchSize = 1)
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

        private static void PublishCommands(IPublisher<ICommand> commandPublisher)
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