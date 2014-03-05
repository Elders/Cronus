using System;
using System.Reflection;
using System.Threading;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Sample.Collaboration.Collaborators.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;
using NMSD.Cronus.Sample.Player;
using NMSD.Cronus.Pipelining.RabbitMQ.Config;
using NMSD.Cronus.Pipelining.Transport.Config;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static IPublisher commandPublisher;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            string IAA = "IdentityAndAccess";

            var cfg = new CronusConfiguration();
            cfg.ConfigurePublisher<PipelinePublisher<ICommand>>(IAA, publisher =>
            {
                publisher.RabbitMq();
                //publisher.Transport<RabbitMq>(x=>x.);
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterNewUser)) };
            })
            .Start();

            commandPublisher = cfg.publishers[IAA][typeof(ICommand)];
            Console.WriteLine("Start sending commands...");
            HostUI(5000, 1);
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

            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail3@gmail.com"));
            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail4@gmail.com"));
            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail5@gmail.com"));
            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail6@gmail.com"));
            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail7@gmail.com"));
            //commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail8@gmail.com"));
        }
    }
}
