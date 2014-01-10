using System;
using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Core;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using NMSD.Cronus.Core.UnitOfWork;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Protoreg;

namespace NMSD.Cronus.Sample.ApplicationService
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<CollaboratorState>();
            protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            protoRegistration.RegisterAssembly<UserState>();
            protoRegistration.RegisterAssembly<NewUserRegistered>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;

            var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            var session = rabbitMqSessionFactory.OpenSession();
            var commaborationES = new RabbitEventStore("Collaboration", connectionString, session, serializer);
            var commandConsumerCollaboration = new CommandConsumer(new CommandHandlersPerBoundedContext(new CommandPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer, commaborationES);
            commandConsumerCollaboration.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            commandConsumerCollaboration.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)));


            var iacES = new RabbitEventStore("IdentityAndAccess", connectionString, session, serializer);
            var commandConsumerIaC = new CommandConsumer(new CommandHandlersPerBoundedContext(new CommandPipelinePerApplication()), new RabbitMqEndpointFactory(session), serializer, iacES);
            commandConsumerIaC.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            commandConsumerIaC.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)));



            commandConsumerCollaboration.Start(1);
            commandConsumerIaC.Start(1);

            Console.ReadLine();
            session.Close();
        }
    }
}
