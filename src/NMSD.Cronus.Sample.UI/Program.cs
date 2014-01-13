using System;
using System.Threading;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.EventSourcing;
using NMSD.Cronus.Transports.Conventions;
using NMSD.Cronus.Transports.RabbitMQ;
using NMSD.Cronus.RabbitMQ;
using NMSD.Cronus.Sample.Collaboration.Collaborators;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Protoreg;
using NMSD.Cronus.Hosts;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static CommandPublisher commandPublisher;

        static void Main(string[] args)
        {
            //    log4net.Config.XmlConfigurator.Configure();

            //    var protoRegistration = new ProtoRegistration();
            //    protoRegistration.RegisterAssembly<RegisterNewUser>();
            //    protoRegistration.RegisterAssembly<Wraper>();
            //    ProtoregSerializer serializer = new ProtoregSerializer(protoRegistration);
            //    serializer.Build();

            //    var rabbitMqSessionFactory = new RabbitMqSessionFactory();
            //    var session = rabbitMqSessionFactory.OpenSession();
            //    commandPublisher = new CommandPublisher(new CommandPipelinePerApplication(), new RabbitMqPipelineFactory(session), serializer);

            //HostUI(1000, 2600); //  Target
            // HostUI(1000, 800);  //  With Snapshot Delition right after new snapshots

            CronusHost host = new CronusHost();
            host.UseRabbitMqTransport();
            host.UseCommandPipelinePerApplication();
            host.ConfigureCommandPublisher(cfg => cfg.RegisterCommandsAssembly<RegisterNewUser>());
            host.BuildSerializer();
            host.BuildCommandPublisher();
            commandPublisher = host.CommandPublisher;
            HostUI(2000);
            //session.Close();
        }

        private static void HostUI(int messageDelayInMilliseconds = 0, int batchSize = 1)
        {

            for (int i = 0; i > -1; i++)
            {
                if (messageDelayInMilliseconds == 0)
                {
                    PublishCommands();
                }
                else
                {
                    for (int j = 0; j < batchSize; j++)
                    {
                        PublishCommands();
                    }

                    Thread.Sleep(messageDelayInMilliseconds);
                }
            }
        }

        private static void PublishCommands()
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = "mynkow@gmail.com";
            commandPublisher.Publish(new RegisterNewUser(userId, email));
            Thread.Sleep(500);

            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail@gmail.com"));
        }
    }
}
