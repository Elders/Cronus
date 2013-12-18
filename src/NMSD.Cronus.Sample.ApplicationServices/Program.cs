using System;
using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Core;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.UnitOfWork;
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

            var commandConsumer = new RabbitCommandConsumer(serializer);
            commandConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();


            var commaborationES = new ProtoEventStore("Collaboration", connectionString, serializer);
            var collaborationEventPublisher = new RabbitEventStorePublisher(serializer);
            commandConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)),
                type =>
                {
                    var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                    handler.EventStore = commaborationES;
                    handler.EventPublisher = collaborationEventPublisher;
                    return handler;
                });

            var iacES = new ProtoEventStore("IdentityAndAccess", connectionString, serializer);
            var iacEventPublisher = new RabbitEventStorePublisher(serializer);
            commandConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)),
                type =>
                {
                    var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                    handler.EventStore = iacES;
                    handler.EventPublisher = iacEventPublisher;
                    return handler;
                });



            commandConsumer.Start(1);

            Console.ReadLine();
        }
    }
}
