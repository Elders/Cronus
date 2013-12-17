using System;
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
using Protoreg;

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

            var eventPublisher = new RabbitEventStorePublisher(serializer);
            var commandConsumer = new RabbitCommandConsumer(serializer);
            commandConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)),
                type =>
                {
                    var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                    handler.EventPublisher = eventPublisher;
                    return handler;
                });
            commandConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(UserAppService)),
                type =>
                {
                    var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                    handler.EventPublisher = eventPublisher;
                    return handler;
                });
            commandConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            commandConsumer.Start(1);

            Console.ReadLine();
        }
    }
}
