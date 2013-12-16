using System.Reflection;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using Protoreg;
using NMSD.Cronus.Core.UnitOfWork;

namespace NMSD.Cronus.Sample.Handlers
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            var eventConsumer = new RabbitEventConsumer(serializer);
            eventConsumer.UnitOfWorkFactory = new NullUnitOfWorkFactory();
            eventConsumer.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
            eventConsumer.Start(2);
        }
    }
}
