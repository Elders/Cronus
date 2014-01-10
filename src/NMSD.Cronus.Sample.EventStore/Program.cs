using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Transports.Conventions;
using NMSD.Cronus.Core.Transports.RabbitMQ;
using NMSD.Cronus.Core.UnitOfWork;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Events;
using NMSD.Protoreg;

namespace NMSD.Cronus.Sample.EventStore
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
            var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            var session = rabbitMqSessionFactory.OpenSession();
            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            var eventPublisher = new EventPublisher(new EventHandlersPipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);
            var iacES = new MssqlEventStore("IdentityAndAccess", connectionString, serializer);
            var iacEventStoreConsumer = new EventStoreConsumer(new EventStorePerBoundedContext(new EventStorePipelinePerApplication()), new RabbitMqEndpointFactory(session), Assembly.GetAssembly(typeof(NewUserRegistered)), serializer, iacES, eventPublisher);
            iacEventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            iacEventStoreConsumer.Start(5);

            var collaborationES = new MssqlEventStore("Collaboration", connectionString, serializer);
            var collaborationEventStoreConsumer = new EventStoreConsumer(new EventStorePerBoundedContext(new EventStorePipelinePerApplication()), new RabbitMqEndpointFactory(session), Assembly.GetAssembly(typeof(NewCollaboratorCreated)), serializer, collaborationES, eventPublisher);
            collaborationEventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            collaborationEventStoreConsumer.Start(5);
            System.Console.ReadLine();
            session.Close();


        }
    }
}
