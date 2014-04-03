using System.Configuration;
using System.Reflection;
using Elders.Cronus.EventSourcing;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Cronus.Persistence.MSSQL;
using Elders.Cronus.Persistence.MSSQL.Config;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.Collaboration.Users.Events;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace Elders.Cronus.Sample.EventStore
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            DatabaseManager.DeleteDatabase(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
            UseCronusHost();
            System.Console.WriteLine("Started Event store");
            System.Console.ReadLine();
            host.Stop();
        }

        static void UseCronusHost()
        {
            var cfg = new CronusConfiguration();

            const string IAA = "IdentityAndAccess";
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus-es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .SetDomainEventsAssembly(typeof(RegisterAccount))
                    .CreateStorage();
            });
            cfg.PipelineEventPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(AccountRegistered)), Assembly.GetAssembly(typeof(UserCreated)) };
            });
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointEventStoreConsumableSettings>(IAA, consumer =>
            {
                consumer.ConsumerBatchSize = 100;
                consumer.NumberOfWorkers = 2;
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)) };
                consumer.UseTransport<RabbitMq>();
            });

            const string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus-es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)))
                    .SetDomainEventsAssembly(typeof(UserCreated))
                    .CreateStorage();
            });
            cfg.ConfigureConsumer<EndpointEventStoreConsumableSettings>(Collaboration, consumer =>
            {
                consumer.ConsumerBatchSize = 100;
                consumer.NumberOfWorkers = 2;
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(UserCreated)) };
                consumer.UseTransport<RabbitMq>();
            })
            .Build();

            host = new CronusHost(cfg);
            host.Start();
        }
    }
}
