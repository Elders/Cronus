using System;
using System.Reflection;
using System.Threading;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Pipeline.Host.Config;
using NMSD.Cronus.Pipeline.RabbitMQ.Config;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts;
using NMSD.Cronus.Sample.IdentityAndAccess.Accounts.Commands;

namespace NMSD.Cronus.Sample.UI
{
    class Program
    {
        static IPublisher commandPublisher;

        static void Main(string[] args)
        {
            ConfigurePublisher();

            HostUI(/////////////////////////////////////////////////////////////////
                               publish: SingleCreationCommand,
                   delayBetweenBatches: 1000,
                             batchSize: 1,
                numberOfMessagesToSend: Int32.MaxValue
                ///////////////////////////////////////////////////////////////////
                );
        }

        private static void ConfigurePublisher()
        {
            log4net.Config.XmlConfigurator.Configure();

            var cfg = new CronusConfiguration();
            cfg.PipelineCommandPublisher(publisher =>
            {
                publisher.UseTransport<RabbitMqTransportSettings>();
                publisher.MessagesAssemblies = new[] { Assembly.GetAssembly(typeof(RegisterAccount)) };
            })
            .Build();

            commandPublisher = cfg.GlobalSettings.CommandPublisher;
        }

        private static void SingleCreationCommand()
        {
            AccountId accountId = new AccountId(Guid.NewGuid());
            var email = "cronus_0_@nmsd.com";
            commandPublisher.Publish(new RegisterAccount(accountId, email));
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

        private static void HostUI(Action publish, int delayBetweenBatches = 0, int batchSize = 1, int numberOfMessagesToSend = Int32.MaxValue)
        {
            Console.WriteLine("Start sending commands...");
            if (batchSize == 1)
            {
                if (delayBetweenBatches == 0)
                {
                    for (int i = 0; i < numberOfMessagesToSend; i++)
                    {
                        publish();
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfMessagesToSend; i++)
                    {
                        publish();
                        Thread.Sleep(delayBetweenBatches);
                    }
                }
            }
            else
            {
                if (delayBetweenBatches == 0)
                {
                    for (int i = 0; i < numberOfMessagesToSend; i = i + batchSize)
                    {
                        for (int j = 0; j < batchSize; j++)
                        {
                            publish();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfMessagesToSend; i = i + batchSize)
                    {
                        for (int j = 0; j < batchSize; j++)
                        {
                            publish();
                        }
                        Thread.Sleep(delayBetweenBatches);
                    }
                }
            }
        }
    }
}
