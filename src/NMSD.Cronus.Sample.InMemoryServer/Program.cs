using System;
using System.Configuration;
using System.Reflection;
using System.Threading;
using NMSD.Cronus;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Eventing;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Projections;
using NMSD.Protoreg;

namespace NMSD.Cronus.Sample.InMemoryServer
{
    class Program
    {
        static InMemoryCommandBus commandBus;

        static InMemoryEventBus eventBus;

        static ProtoregSerializer serializer;

        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            BuildProtoregSerializer();

            HostEventHandlers();
            HostApplicationServices();
            HostUI(0);

            Console.ReadLine();
        }

        private static void BuildProtoregSerializer()
        {
            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<CollaboratorState>();
            protoRegistration.RegisterAssembly<CreateNewCollaborator>();
            protoRegistration.RegisterAssembly<Wraper>();
            serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();
        }

        private static void HostApplicationServices()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            //IAggregateRepository eventStore = new ProtoEventStore(connectionString, serializer);

            commandBus = new InMemoryCommandBus();
            commandBus.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorAppService)),
                type =>
                {
                    var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                    //handler.EventPublisher = eventBus;
                    return handler;
                });

        }

        private static void HostEventHandlers()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            var eventStore = new MssqlEventStore("", connectionString, serializer);

            eventBus = new InMemoryEventBus(eventStore);
            eventBus.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
        }

        private static void HostUI(int messageDelayInMilliseconds = 0)
        {
            var email = "test@qqq.commmmmmmm";
            for (int i = 0; i > -1; i++)
            {
                if (messageDelayInMilliseconds == 0)
                {
                    commandBus.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));
                }
                else
                {
                    commandBus.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));
                    Thread.Sleep(messageDelayInMilliseconds);
                }
            }
        }

    }
}