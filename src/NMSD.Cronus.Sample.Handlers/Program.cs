using System.Reflection;
using NMSD.Cronus;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Protoreg;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {
        private static CronusHost host;
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            //var protoRegistration = new ProtoRegistration();
            //protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            //protoRegistration.RegisterAssembly<NewUserRegistered>();
            //protoRegistration.RegisterAssembly<Wraper>();
            //ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            //serializer.Build();

            //var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            //var session = rabbitMqSessionFactory.OpenSession();
            //var commandPublisher = new CommandPublisher(new CommandPipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);

            //var eventConsumer = new EventConsumer(new EventHandlerPerEndpoint(new EventHandlersPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer, commandPublisher);
            //eventConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            //eventConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
            //eventConsumer.Start(2);
            UseCronusHost();
            System.Console.ReadLine();
            //session.Close();
        }

        static void UseCronusHost()
        {
            host = new CronusHost();
            host.UseRabbitMqTransport();
            host.UseEventHandlersPipelinePerApplication();
            host.UseEventHandlerPerEndpoint();
            host.UseCommandPipelinePerApplication();
            host.ConfigureCommandPublisher(x =>
            {
                x.RegisterCommandsAssembly<RegisterNewUser>();
                x.RegisterCommandsAssembly<CreateNewCollaborator>();
            });
       //     host.BuildCommandPublisher();
            host.ConfigureEventHandlersConsumer(cfg =>
            {
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.RegisterEventsAssembly(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
                cfg.RegisterEventsAssembly(Assembly.GetAssembly(typeof(NewUserRegistered)));
                cfg.SetEventHandlersAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
            });
            host.BuildSerializer();
            host.HostEventHandlerConsumers(1);
        }
    }
}
