using System;
using System.Reflection;
using System.Threading;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Hosts;
using NMSD.Cronus.Pipeline.Transport.RabbitMQ.Config;
using NMSD.Cronus.Sample.Collaboration.Users;
using NMSD.Cronus.Sample.Collaboration.Users.Commands;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static IPublisher commandPublisher;

        static void Main(string[] args)
        {
            Thread.Sleep(10000);

            ConfigurePublisher();

            HostUI(/////////////////////////////////////////////////////////////////
                                publish: SingleCreationCommandFromUpstreamBC,
                    delayBetweenBatches: 100,
                              batchSize: 100,
                 numberOfMessagesToSend: 10000
                ///////////////////////////////////////////////////////////////////
                 );

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static void ConfigurePublisher()
        {
            log4net.Config.XmlConfigurator.Configure();

            var cfg = new CronusConfiguration();
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMq>();
                publisher.MessagesAssemblies = new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) };
            })
            .Build();

            commandPublisher = cfg.GlobalSettings.CommandPublisher;
        }

        private static void SingleCreationCommandFromUpstreamBC(int index)
        {
            AccountId accountId = new AccountId(Guid.NewGuid());
            var email = String.Format("cronus_{0}_@nmsd.com", index);
            commandPublisher.Publish(new RegisterAccount(accountId, email));
        }

        private static void SingleCreationCommandFromDownstreamBC(int index)
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = String.Format("cronus_{0}_@nmsd.com", index);
            commandPublisher.Publish(new CreateUser(userId, email));
        }

        private static void SingleCreateWithMultipleUpdateCommands()
        {
            AccountId accountId = new AccountId(Guid.NewGuid());
            var email = "cronus_0_@nmsd.com";
            commandPublisher.Publish(new RegisterAccount(accountId, email));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_1_@nmsd.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_2_@nmsd.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_3_@nmsd.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_4_@nmsd.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_5_@nmsd.com"));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void HostUI(Action<int> publish, int delayBetweenBatches = 0, int batchSize = 1, int numberOfMessagesToSend = Int32.MaxValue)
        {
            Console.WriteLine("Start sending commands...");
            if (batchSize == 1)
            {
                if (delayBetweenBatches == 0)
                {
                    for (int i = 0; i < numberOfMessagesToSend; i++)
                    {
                        publish(i);
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfMessagesToSend; i++)
                    {
                        publish(i);
                        Thread.Sleep(delayBetweenBatches);
                    }
                }
            }
            else
            {
                if (delayBetweenBatches == 0)
                {
                    for (int i = 0; i <= numberOfMessagesToSend - batchSize; i = i + batchSize)
                    {
                        for (int j = 0; j < batchSize; j++)
                        {
                            publish(i + j);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= numberOfMessagesToSend - batchSize; i = i + batchSize)
                    {
                        for (int j = 0; j < batchSize; j++)
                        {
                            publish(i + j);
                        }
                        Thread.Sleep(delayBetweenBatches);
                    }
                }
            }
        }
    }
}
