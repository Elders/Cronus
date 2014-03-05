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
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

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
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(UserState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<IEvent>>(IAA, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(NewUserRegistered)) };
            });
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>(IAA, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterNewUser)) };
            });
            cfg.ConfigureEventStoreConsumer<EndpointEventStoreConsumer>(IAA, consumer =>
            {
                consumer.AssemblyEventsWhichWillBeIntercepted = typeof(RegisterNewUser);
                consumer.RabbitMq();
            });

            string Collaboration = "Collaboration";
            cfg.ConfigureEventStore<MssqlEventStore>(eventStore =>
            {
                eventStore.BoundedContext = Collaboration;
                eventStore.ConnectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
                eventStore.AggregateStatesAssembly = Assembly.GetAssembly(typeof(CollaboratorState));
            });
            cfg.ConfigurePublisher<PipelinePublisher<IEvent>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(NewCollaboratorCreated)) };
            });
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>(Collaboration, publisher =>
            {
                publisher.RabbitMq();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(CreateNewCollaborator)) };
            });
            cfg.ConfigureEventStoreConsumer<EndpointEventStoreConsumer>(Collaboration, consumer =>
            {
                consumer.AssemblyEventsWhichWillBeIntercepted = typeof(NewCollaboratorCreated);
                consumer.RabbitMq();
            });

            cfg.Start();
        }
    }
}
