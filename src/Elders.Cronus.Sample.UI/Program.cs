using System;
using System.Reflection;
using System.Threading;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace Elders.Cronus.Sample.UI
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
                 numberOfMessagesToSend: int.MaxValue
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
            var email = String.Format("cronus_{0}_@Elders.com", index);
            commandPublisher.Publish(new RegisterAccount(accountId, email));
        }

        private static void SingleCreationCommandFromDownstreamBC(int index)
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = String.Format("cronus_{0}_@Elders.com", index);
            commandPublisher.Publish(new CreateUser(userId, email));
        }

        private static void SingleCreateWithMultipleUpdateCommands()
        {
            AccountId accountId = new AccountId(Guid.NewGuid());
            var email = "cronus_0_@Elders.com";
            commandPublisher.Publish(new RegisterAccount(accountId, email));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_1_@Elders.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_2_@Elders.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_3_@Elders.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_4_@Elders.com"));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, "cronus_5_@Elders.com"));
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
