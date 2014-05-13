using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Persistence.MSSQL.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Pipeline;

namespace Elders.Cronus.Sample.ApplicationService
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            UseCronusHost();
            System.Console.WriteLine("Started command handlers");
            System.Console.ReadLine();
            host.Stop();
            host = null;
        }

        static void UseCronusHost()
        {
            var cfg = new CronusConfiguration();

            const string IAA = "IdentityAndAccess";
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .SetDomainEventsAssembly(typeof(RegisterAccount));
            });
            cfg.PipelineEventStorePublisher(publisher =>
            {
                publisher.UseRabbitMqTransport();
            });
            cfg.ConfigureConsumer<EndpointCommandConsumableSettings>(IAA, consumer =>
            {
                consumer
                    .SetNumberOfWorkers(2)
                    .UseRabbitMqTransport()
                    .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(AccountAppService)), (type, context) =>
                        {
                            var handler = FastActivator.CreateInstance(type, null);
                            var repositoryHandler = handler as IAggregateRootApplicationService;
                            if (repositoryHandler != null)
                            {
                                repositoryHandler.Repository = new RabbitRepository(cfg.GlobalSettings.AggregateRepositories[IAA], cfg.GlobalSettings.EventStorePublisher);
                            }
                            return handler;
                        });
            });

            const string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)))
                    .SetDomainEventsAssembly(typeof(CreateUser));
            });
            cfg.ConfigureConsumer<EndpointCommandConsumableSettings>(Collaboration, consumer =>
            {
                consumer
                    .SetNumberOfWorkers(2)
                    .UseRabbitMqTransport()
                    .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                        {
                            var handler = FastActivator.CreateInstance(type, null);
                            var repositoryHandler = handler as IAggregateRootApplicationService;
                            if (repositoryHandler != null)
                            {
                                repositoryHandler.Repository = new RabbitRepository(cfg.GlobalSettings.AggregateRepositories[Collaboration], cfg.GlobalSettings.EventStorePublisher);
                            }
                            return handler;
                        });
            })
            .Build();

            host = new CronusHost(cfg);
            host.Start();
        }
    }
}