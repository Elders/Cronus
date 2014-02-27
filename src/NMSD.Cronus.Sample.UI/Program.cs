using System;
using System.Threading;
using NMSD.Cronus.Commanding;
using NMSD.Cronus.Hosts;
using NMSD.Cronus.Sample.IdentityAndAccess.Users;
using NMSD.Cronus.Sample.IdentityAndAccess.Users.Commands;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static CommandPublisher commandPublisher;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            CronusHost host = new CronusHost();

            host.UseCommandPipelinePerApplication();
            host.ConfigureCommandPublisher(cfg => cfg.RegisterCommandsAssembly<RegisterNewUser>());
            host.UseRabbitMqTransport();
            host.BuildSerializer();
            host.BuildCommandPublisher();
            commandPublisher = host.CommandPublisher;
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

            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail3@gmail.com"));
            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail4@gmail.com"));
            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail5@gmail.com"));
            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail6@gmail.com"));
            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail7@gmail.com"));
            commandPublisher.Publish(new ChangeUserEmail(userId, email, "newEmail8@gmail.com"));
        }
    }
}
