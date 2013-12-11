using System;
using System.Threading;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Cqrs;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Events;
using Protoreg;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<NewCollaboratorCreated>();
            protoRegistration.RegisterAssembly<Wraper>();
            //protoRegistration.RegisterCommonType(typeof(AggregateRootState<CollaboratorId>));
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            var commandPublisher = new RabbitCommandPublisher(serializer);

            var email = "test@qqq.commmmmmmm";

            for (int i = 0; i > -1; i++)
            {
                //for (int j = 0; j < 100; j++)
                {
                    commandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));
                }
                //Thread.Sleep(50);
            }
        }
    }
}
