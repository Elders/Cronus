using System.Configuration;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.RabbitMQ.Config;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Users.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Events;
using NMSD.Cronus.Sample.Player;

namespace NMSD.Cronus.Sample.EventStore
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            UseCronusHost();
            System.Console.WriteLine("Started Event store");
            System.Console.ReadLine();
        }

        static void UseCronusHost()
        {
            var cfg = new CronusConfiguration();

            string IAA = "IdentityAndAccess";
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = IAA;
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(AccountState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<IEvent>>(IAA, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(AccountRegistered)) };
            });
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>(IAA, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)) };
            });
            cfg.ConfigureEventStoreConsumer<EndpointEventStoreConsumer>(IAA, consumer =>
            {
                consumer.AssemblyEventsWhichWillBeIntercepted = typeof(RegisterAccount);
                consumer.RabbitMq();
            });

            string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = Collaboration;
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(UserState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<IEvent>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(UserCreated)) };
            });
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
            });
            cfg.ConfigureEventStoreConsumer<EndpointEventStoreConsumer>(Collaboration, consumer =>
            {
                consumer.AssemblyEventsWhichWillBeIntercepted = typeof(UserCreated);
                consumer.RabbitMq();
            });

            cfg.Start();
        }
    }
}
