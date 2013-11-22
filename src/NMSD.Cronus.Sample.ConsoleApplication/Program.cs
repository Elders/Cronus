using System;
using System.Diagnostics;
using System.Reflection;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.Eventing;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using NMSD.Cronus.Sample.Ports;
using Protoreg;

namespace Cronus.Sample.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var eventBus = new InMemoryEventBus();
            eventBus.RegisterAllEventHandlersInAssembly(Assembly.GetAssembly(typeof(CollaboratorProjection)));

            var collaboratorId = new CollaboratorId(Guid.NewGuid());
            var email = "test@qqq.com";
            var cmd = new CreateNewCollaborator(collaboratorId, email);

            var commandBus = new InMemoryCommandBus();
            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();
            commandBus.RegisterAllCommandHandlersInAssembly(x =>
            {
                var instance = Activator.CreateInstance(x);
                var casted = instance as IAggregateRootApplicationService;
                if (casted != null)
                {
                    casted.EventBus = eventBus;
                    casted.EventStore = new InMemoryEventStore(serializer);
                }
                return casted as ICommandHandler;
            }, Assembly.GetAssembly(typeof(CollaboratorAppService)));
            commandBus.Publish(cmd);



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
            Stopwatch stopWatch = new Stopwatch();
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
            Stopwatch stopWatch = new Stopwatch();
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
