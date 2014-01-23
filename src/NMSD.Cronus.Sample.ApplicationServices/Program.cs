using System;
using System.Configuration;
using System.Reflection;
using NMSD.Cronus;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.UnitOfWork;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Protoreg;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.ApplicationService
{
    class Program
    {
        static CronusHost host;
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            //var protoRegistration = new ProtoRegistration();
            //protoRegistration.RegisterAssembly<CollaboratorState>();
            //protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            //protoRegistration.RegisterAssembly<UserState>();
            //protoRegistration.RegisterAssembly<NewUserRegistered>();
            //protoRegistration.RegisterAssembly<Wraper>();
            //ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            //serializer.Build();

            //string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;

            //var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            //var session = rabbitMqSessionFactory.OpenSession();
            //var commaborationES = new RabbitEventStore("Collaboration", connectionString, session, serializer);
            //var commandConsumerCollaboration = new CommandConsumer(new CommandHandlersPerBoundedContext(new CommandPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer, commaborationES);
            //commandConsumerCollaboration.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            //commandConsumerCollaboration.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)));


            //var iacES = new RabbitEventStore("IdentityAndAccess", connectionString, session, serializer);
            //var commandConsumerIaC = new CommandConsumer(new CommandHandlersPerBoundedContext(new CommandPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer, iacES);
            //commandConsumerIaC.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            //commandConsumerIaC.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)));



            //commandConsumerCollaboration.Start(1);
            //commandConsumerIaC.Start(1);
            UseCronusHost();
            System.Console.WriteLine("Started");
            System.Console.ReadLine();
            host.Release();

            // session.Close();
        }
        static void UseCronusHost()
        {
            host = new CronusHost();
            host.UseRabbitMqTransport();
            host.UseCommandPipelinePerApplication();
            host.UseCommandHandlersPerBoundedContext();
            host.ConfigureCommandConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(CollaboratorState)));
                cfg.SetEventsAssembly(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
                cfg.SetCommandsAssembly(Assembly.GetAssembly(typeof(CreateNewCollaborator)));
                cfg.SetCommandHandlersAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)));
            });
            host.ConfigureCommandConsumer(cfg =>
            {
                cfg.SetEventStoreConnectionString(ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString);
                cfg.SetUnitOfWorkFacotry(new NullUnitOfWorkFactory());
                cfg.SetAggregateStatesAssembly(Assembly.GetAssembly(typeof(UserState)));
                cfg.SetEventsAssembly(Assembly.GetAssembly(typeof(NewUserRegistered)));
                cfg.SetCommandsAssembly(Assembly.GetAssembly(typeof(RegisterNewUser)));
                cfg.SetCommandHandlersAssembly(Assembly.GetAssembly(typeof(UserAppService)));
            });
            host.BuildSerializer();
            host.HostCommandConsumers(1);
        }
    }
}
