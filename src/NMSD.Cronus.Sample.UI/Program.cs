using System;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using Protoreg;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<CreateNewCollaborator>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            var commandPublisher = new RabbitCommandPublisher(serializer);

            var email = "test@qqq.commmmmmmm";

            for (int i = 0; i > -1; i++)
            {
                commandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));


                //if (commandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email)))
                //    Thread.Sleep(1000);
                //else
                //    Thread.Sleep(10000);
            }
        }
    }
}
