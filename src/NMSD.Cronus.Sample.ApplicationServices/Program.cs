using System.Configuration;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Persitence.MSSQL.Config;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Pipelining.Host.Config;
using NMSD.Cronus.Pipelining.RabbitMQ.Config;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace NMSD.Cronus.Sample.ApplicationService
{
    class Program
    {

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            UseCronusHost();
            System.Console.WriteLine("Started command handlers");
            System.Console.ReadLine();
        }

        static void UseCronusHost()
        {
            var cfg = new CronusConfiguration();

            string IAA = "IdentityAndAccess";
            cfg.ConfigureEventStore(eventStore =>
            {
                eventStore.BoundedContext = IAA;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(AccountState));
                eventStore.MsSql(es => es.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
            });
            cfg.ConfigurePublisher<PipelinePublisher<DomainMessageCommit>>(IAA, publisher =>
            {
                publisher.RabbitMq();
            });
            cfg.ConfigureConsumer<EndpointConsumer<ICommand>>(IAA, consumer =>
            {
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)) };
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(AccountAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = new RabbitRepository((IAggregateRepository)cfg.GlobalSettings.eventStores[IAA], cfg.GlobalSettings.publishers[IAA][typeof(DomainMessageCommit)]);
                        }
                        return handler;
                    });
                consumer.RabbitMq();
            });

            string Collaboration = "Collaboration";
            cfg.ConfigureEventStore(eventStore =>
            {
                eventStore.BoundedContext = Collaboration;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(UserState));
                eventStore.MsSql(es => es.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
            });
            cfg.ConfigurePublisher<PipelinePublisher<DomainMessageCommit>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
            });
            cfg.ConfigureConsumer<EndpointConsumer<ICommand>>(Collaboration, consumer =>
            {
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateUser)) };
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = new RabbitRepository((IAggregateRepository)cfg.GlobalSettings.eventStores[Collaboration], cfg.GlobalSettings.publishers[IAA][typeof(DomainMessageCommit)]);
                        }
                        return handler;
                    });
                consumer.RabbitMq();
            });

            cfg.Start();
        }
    }
}