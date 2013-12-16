using System.Configuration;
using System.Reflection;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.UnitOfWork;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
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
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            var eventStore = new ProtoEventStore(connectionString, serializer);
            var eventStoreConsumer = new RabbitEventStoreConsumer(Assembly.GetAssembly(typeof(NewCollaboratorCreated)), serializer, eventStore);
            eventStoreConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            eventStoreConsumer.Start(1);
        }
    }
}
