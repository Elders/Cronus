using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using Cronus.Core;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Collaboration.Projections;
using Protoreg;

namespace Cronus.Sample.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();



            var collaboratorId = new CollaboratorId(Guid.NewGuid());// Parse("66ada31c-a098-47a6-921c-428a9f3fd485"));
            var email = "test@qqq.commmmmmmm";
            var cmd = new CreateNewCollaborator(collaboratorId, email);
            //var cmd = new RenameCollaborator(collaboratorId, "", "");


            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<CollaboratorState>();
            protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            string connectionString = ConfigurationManager.ConnectionStrings["cronus-es"].ConnectionString;
            var eventStore = new InMemoryEventStore(connectionString, serializer);

            var eventBus = new RabbitEventStorePublisher(serializer);

            var commandConsumer = new RabbitCommandConsumer(serializer);
            commandConsumer.RegisterAllHandlersInAssembly(type =>
                                                                {
                                                                    var handler = FastActivator.CreateInstance(type) as IAggregateRootApplicationService;
                                                                    handler.EventPublisher = eventBus;
                                                                    handler.EventStore = eventStore;
                                                                    return handler;
                                                                },
                                                                Assembly.GetAssembly(typeof(CollaboratorAppService)));
            commandConsumer.Start(1);


            //var result = MeasureExecutionTime.Start(() =>
            //{
            //    //int current = 0;
            //    foreach (IEvent @event in eventStore.GetEventsFromStart("Collaboration"))
            //    {
            //        //current++;
            //        ////Console.WriteLine(@event.ToString());
            //        //if (current % 100 == 0)
            //        //    Console.WriteLine(current);
            //    }
            //});
            //Console.WriteLine(result);


            //commandBus.RegisterAllCommandHandlersInAssembly(x =>
            //{
            //    var instance = Activator.CreateInstance(x);
            //    var casted = instance as IAggregateRootApplicationService;
            //    if (casted != null)
            //    {
            //        casted.EventBus = eventBus;
            //        casted.EventStore = eventStore;
            //    }
            //    return casted as ICommandHandler;
            //}, Assembly.GetAssembly(typeof(CollaboratorAppService)));

            var commandPublisher = new RabbitCommandPublisher(serializer);
            commandPublisher.Publish(cmd);





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

    public static class MeasureExecutionTime
    {
        public static string Start(Action action)
        {
            string result = string.Empty;
#if DEBUG
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            action();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
#endif
            return result;
        }

        public static string Start(Action action, int repeat)
        {
            string result = string.Empty;
#if DEBUG
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < repeat; i++)
            {
                action();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            result = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
#endif
            return result;
        }
    }
}
