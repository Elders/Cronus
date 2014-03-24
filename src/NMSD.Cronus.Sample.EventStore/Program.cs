using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Persistence.MSSQL;
using NMSD.Cronus.Persistence.MSSQL.Config;
using NMSD.Cronus.Pipeline.Config;
using NMSD.Cronus.Pipeline.Hosts;
using NMSD.Cronus.Pipeline.Transport.RabbitMQ.Config;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Users.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Events;

namespace NMSD.Cronus.Sample.EventStore
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            DatabaseManager.DeleteDatabase(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
            UseCronusHost();
            System.Console.WriteLine("Started Event store");
            System.Console.ReadLine();
        }

        static void UseCronusHost()
        {
            var cfg = new CronusConfiguration();

            const string IAA = "IdentityAndAccess";
            cfg.ConfigureEventStore<MsSqlEventStoreSettings>(eventStore =>
            {
                eventStore
                    .SetConnectionStringName("cronus-es")
                    .SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(AccountState)))
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
                    .CreateStorage();
            });
            cfg.ConfigureConsumer<EndpointEventStoreConsumableSettings>(Collaboration, consumer =>
            {
                consumer.NumberOfWorkers = 2;
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(UserCreated)) };
                consumer.UseTransport<RabbitMq>();
            })
            .Build();

            new CronusHost(cfg).Start();
        }
    }
}
