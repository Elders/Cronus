using System.Reflection;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.EventSourcing.Config;
using Elders.Cronus.Persistence.MSSQL.Config;

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
            var cfg = new CronusSettings();
            cfg.UseContractsFromAssemblies(new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)), Assembly.GetAssembly(typeof(UserState)), Assembly.GetAssembly(typeof(AccountState)) });

            const string IAA = "IdentityAndAccess";

            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState)));

            cfg.UsePipelineEventStorePublisher(publisher => publisher.UseRabbitMqTransport());
            cfg.UseCommandConsumable(IAA, consumable => consumable
                    .SetNumberOfConsumers(2)
                    .UseRabbitMqTransport()
                    .CommandConsumer(consumer => consumer
                        .UseCommandHandler(h => h
                            .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(AccountAppService)), (type, context) =>
                                {
                                    var handler = FastActivator.CreateInstance(type, null);
                                    var repositoryHandler = handler as IAggregateRootApplicationService;
                                    if (repositoryHandler != null)
                                    {
                                        repositoryHandler.Repository = new RabbitRepository((cfg as IHaveEventStores).EventStores[IAA].Value.AggregateRepository, (cfg as IHaveEventStorePublisher).EventStorePublisher.Value);
                                    }
                                    return handler;
                                }))));

            const string Collaboration = "Collaboration";
            cfg.UseMsSqlEventStore(eventStore => eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState))));

            cfg.UseCommandConsumable(Collaboration, consumable => consumable
                    .SetNumberOfConsumers(2)
                    .UseRabbitMqTransport()
                    .CommandConsumer(consumer => consumer
                        .UseCommandHandler(h => h
                            .RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                                {
                                    var handler = FastActivator.CreateInstance(type, null);
                                    var repositoryHandler = handler as IAggregateRootApplicationService;
                                    if (repositoryHandler != null)
                                    {
                                        repositoryHandler.Repository = new RabbitRepository((cfg as IHaveEventStores).EventStores[Collaboration].Value.AggregateRepository, (cfg as IHaveEventStorePublisher).EventStorePublisher.Value);
                                    }
                                    return handler;
                                }))));


            host = new CronusHost(cfg.GetInstance());
            host.Start();
        }
    }
}