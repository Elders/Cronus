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
using Protoreg;

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
            var eventStore = new ProtoEventStore(connectionString, serializer);
            var bcAssemblies = new List<Assembly>();
            bcAssemblies.Add(Assembly.GetAssembly(typeof(NewCollaboratorCreated)));
            bcAssemblies.Add(Assembly.GetAssembly(typeof(NewUserRegistered)));
            var eventStoreConsumer = new RabbitEventStoreConsumer(bcAssemblies, serializer, eventStore);
            eventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            eventStoreConsumer.Start(2);
        }
    }
}
