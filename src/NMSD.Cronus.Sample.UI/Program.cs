using System;
using System.Threading;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.EventStoreEngine;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using Protoreg;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static RabbitCommandPublisher commandPublisher;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var protoRegistration = new ProtoRegistration();
            protoRegistration.RegisterAssembly<CreateNewCollaborator>();
            protoRegistration.RegisterAssembly<Wraper>();
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            commandPublisher = new RabbitCommandPublisher(serializer);

            HostUI();
        }

        private static void HostUI(int messageDelayInMilliseconds = 0)
        {
            var email = "test@qqq.commmmmmmm";
            for (int i = 0; i > -1; i++)
            {
                if (messageDelayInMilliseconds == 0)
                {
                    commandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));
                }
                else
                {
                    commandPublisher.Publish(new CreateNewCollaborator(new CollaboratorId(Guid.NewGuid()), email));
                    Thread.Sleep(messageDelayInMilliseconds);
                }
            }
        }
    }
}
