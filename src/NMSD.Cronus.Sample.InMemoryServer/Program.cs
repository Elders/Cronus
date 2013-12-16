using System;
using System.Configuration;
using System.Reflection;
using Cronus.Core;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Projections;
using Protoreg;

namespace NMSD.Cronus.Sample.InMemoryServer
{
    class Program
    {
        private static void HostApplicationServices()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            var eventStore = new ProtoEventStore(connectionString, serializer);

            commandBus = new InMemoryCommandBus();
            commandBus.RegisterAllHandlersInAssembly(type =>
                                                    {
                                                        var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                                                        handler.EventPublisher = eventBus;
                                                        return handler;
                                                    },
                                                    Assembly.GetAssembly(typeof(CollaboratorAppService)));



        }

        static InMemoryEventBus eventBus;
        private static void HostEventHandlers()
        {
            //string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            //var eventStore = new ProtoEventStore(connectionString, serializer);

            eventBus = new InMemoryEventBus();
            eventBus.RegisterAllHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));
        }

        private static void HostUI()
        {
            var email = "test@qqq.commmmmmmm";
            for (int i = 0; i > -1; i++)
            {
                commandBus.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));
                //if (commandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email)))
                //    Thread.Sleep(1000);
                //else
                //    Thread.Sleep(10000);
            }
        }

        static InMemoryCommandBus commandBus;
        static ProtoregSerializer serializer;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<CollaboratorState>();
            protoRegistration.RegisterAssembly<CreateNewCollaborator>();
            protoRegistration.RegisterAssembly<Wraper>();
            serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            HostEventHandlers();
            HostApplicationServices();
            HostUI();

            //IEventBus bus = new InMemoryEventBus();
            //bus.RegisterAllEventHandlersInAssembly(System.Reflection.Assembly.GetAssembly(typeof(Program)));
            //bus.OnErrorHandlingEvent((x, y, z) =>
            //{
            //    Console.WriteLine("{0} | {1} | {2}", z.Message, y.GetType().Name, x.GetType().Name);

            //});
            //for (int i = 0; i < 100; i++)
            //{
            //    bus.PublishAsync(new TestEvent());
            //}

            // Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}