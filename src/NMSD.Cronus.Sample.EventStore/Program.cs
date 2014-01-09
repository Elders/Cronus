using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
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

            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;

            var iacES = new MssqlEventStore("IdentityAndAccess", connectionString, serializer);
            var iacEventStoreConsumer = new RabbitEventStoreConsumer(Assembly.GetAssembly(typeof(NewUserRegistered)), serializer, iacES);
            iacEventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            iacEventStoreConsumer.Start(1);

            var collaborationES = new MssqlEventStore("Collaboration", connectionString, serializer);
            var collaborationEventStoreConsumer = new RabbitEventStoreConsumer(Assembly.GetAssembly(typeof(NewCollaboratorCreated)), serializer, collaborationES);
            collaborationEventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            collaborationEventStoreConsumer.Start(1);


        }
    }
}
