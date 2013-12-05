using System;
using System.Threading;
using NMSD.Cronus.Core.Commanding;
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
            ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            serializer.Build();

            var commandPublisher = new RabbitCommandPublisher(serializer);

            var collaboratorId = new CollaboratorId(Guid.NewGuid());// Parse("66ada31c-a098-47a6-921c-428a9f3fd485"));
            var email = "test@qqq.commmmmmmm";
            var cmd = new CreateNewCollaborator(collaboratorId, email);
            //var cmd = new RenameCollaborator(collaboratorId, "", "");

            commandPublisher.Publish(cmd);
        }
    }
}
