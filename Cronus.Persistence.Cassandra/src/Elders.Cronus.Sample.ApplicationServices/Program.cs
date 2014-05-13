using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Persistence.Cassandra.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace Elders.Cronus.Sample.ApplicationService
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            UseCronusHostWithCassandraEventStore();
            System.Console.WriteLine("Started command handlers");
            System.Console.ReadLine();
            host.Stop();
            host = null;
        }

        static void UseCronusHostWithCassandraEventStore()
        {
            var cfg = new CronusConfiguration();

            cfg.PipelineEventPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(AccountRegistered)), Assembly.GetAssembly(typeof(UserCreated)) };
            });

            const string IAA = "IdentityAndAccess";
            cfg.ConfigureEventStore<CassandraEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .SetDomainEventsAssembly(typeof(RegisterAccount))
                    .CreateStorage();
            });
            cfg.ConfigureConsumer<EndpointCommandConsumableSettings>(IAA, consumer =>
            {
                consumer.NumberOfWorkers = 1;
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(AccountAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = cfg.GlobalSettings.AggregateRepositories[IAA];
                        }
                        return handler;
                    });
                consumer.UseTransport<RabbitMq>();
            });

            const string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<CassandraEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)))
                    .SetDomainEventsAssembly(typeof(CreateUser))
                    .CreateStorage();
            });
            cfg.ConfigureConsumer<EndpointCommandConsumableSettings>(Collaboration, consumer =>
            {
                consumer.NumberOfWorkers = 1;
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = cfg.GlobalSettings.AggregateRepositories[Collaboration];
                        }
                        return handler;
                    });
                consumer.UseTransport<RabbitMq>();
            })
            .Build();

            host = new CronusHost(cfg);
            host.Start();
        }
    }
}