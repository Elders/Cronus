using System.Configuration;
using System.Reflection;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.Player;
using NMSD.Cronus.Pipelining.RabbitMQ.Config;
using NMSD.Cronus.Pipelining.Transport.Config;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

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
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = IAA;
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(UserState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<DomainMessageCommit>>(IAA, publisher =>
            {
                publisher.RabbitMq();
            });
            cfg.ConfigureConsumer<EndpointConsumer<ICommand>>(IAA, consumer =>
            {
                consumer.ScopeFactory = new ScopeFactory();
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterNewUser)) };
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = new RabbitRepository((IAggregateRepository)cfg.eventStores[IAA], cfg.publishers[IAA][typeof(DomainMessageCommit)]);
                        }
                        return handler;
                    });
                consumer.RabbitMq();
            });

            string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = Collaboration;
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(CollaboratorState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<DomainMessageCommit>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
            });
            cfg.ConfigureConsumer<EndpointConsumer<ICommand>>(Collaboration, consumer =>
            {
                consumer.ScopeFactory = new ScopeFactory();
                consumer.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateNewCollaborator)) };
                consumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)), (type, context) =>
                    {
                        var handler = FastActivator.CreateInstance(type, null);
                        var repositoryHandler = handler as IAggregateRootApplicationService;
                        if (repositoryHandler != null)
                        {
                            repositoryHandler.Repository = new RabbitRepository((IAggregateRepository)cfg.eventStores[Collaboration], cfg.publishers[IAA][typeof(DomainMessageCommit)]);
                        }
                        return handler;
                    });
                consumer.RabbitMq();
            });

            cfg.Start();
        }
    }
}