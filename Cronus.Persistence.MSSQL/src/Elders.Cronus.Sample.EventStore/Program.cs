using System.Reflection;
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
            //DatabaseManager.DeleteDatabase(ConfigurationManager.ConnectionStrings["cronus_es"].ConnectionString);
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
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(typeof(AccountState))
                    .SetDomainEventsAssembly(typeof(RegisterAccount))
                    .CreateStorage();
            });
            cfg.PipelineEventPublisher(publisher =>
            {
                publisher.UseRabbitMqTransport();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(AccountRegistered)), Assembly.GetAssembly(typeof(UserCreated)) };
            });
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseRabbitMqTransport();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureConsumer<EndpointEventStoreConsumableSettings>(IAA, consumer =>
            {
                consumer.ConsumerBatchSize = 100;
                consumer
                    .SetNumberOfWorkers(2)
                    .UseRabbitMqTransport();
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)) };
            });

            const string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus_es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)))
                    .SetDomainEventsAssembly(typeof(UserCreated))
                    .CreateStorage();
            });
            cfg.ConfigureConsumer<EndpointEventStoreConsumableSettings>(Collaboration, consumer =>
            {

                consumer.ConsumerBatchSize = 100;
                consumer
                    .SetNumberOfWorkers(2)
                    .UseRabbitMqTransport();
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(UserCreated)) };
            })
            .Build();

            host = new CronusHost(cfg);
            host.Start();
        }

    }
}
