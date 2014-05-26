using System;
using System.Reflection;
using System.Threading;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Hosts;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Sample.Collaboration.Users;
using Elders.Cronus.Sample.Collaboration.Users.Commands;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts;
using Elders.Cronus.Sample.IdentityAndAccess.Accounts.Commands;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Cronus.Sample.PerfAnalyzer
{
    class Program
    {
        static IPublisher<ICommand> commandPublisher;
        static int delayBetweenBatches = 150;
        static int batchSize = 100;
        static int numberOfMessagesToSend = Int32.MaxValue;
        static Thread hostUIThread;
        static Thread analysisThread;
        static int changeRate = 10;

        static void Main(string[] args)
        {
            Thread.Sleep(20000);

            ConfigurePublisher();

            queueOverloaded = IncreaseDelay;
            queueNotReachingMaximumCapacity = DencreaseDelay;

            hostUIThread = new Thread(() => { HostUI(SingleCreationCommandFromUpstreamBC); });
            hostUIThread.Start();

            Console.WriteLine("Start analyzing...");
            analysisThread = new Thread(() => { StartAnalysis(5000, 5000); });
            analysisThread.Start();

            Console.ReadLine();
        }

        private static void IncreaseDelay(string queueName)
        {
            delayBetweenBatches -= changeRate;

            Console.WriteLine(String.Format("Batch delay decreased with {1} miliseconds to {0} miliseconds", delayBetweenBatches, changeRate));
        }

        private static void DencreaseDelay(string queueName)
        {
            delayBetweenBatches += 2 * changeRate;

            Console.WriteLine(String.Format("Batch delay increased with {1} miliseconds to {0} miliseconds", delayBetweenBatches, 2 * changeRate));
        }

        private static void ConfigurePublisher()
        {
            log4net.Config.XmlConfigurator.Configure();

            var cronusCfg = new CronusSettings()
                .UseContractsFromAssemblies(new Assembly[] { Assembly.GetAssembly(typeof(RegisterAccount)), Assembly.GetAssembly(typeof(CreateUser)) })
                .WithDefaultPublishers()
                .GetInstance();

            commandPublisher = cronusCfg.CommandPublisher;
        }

        private static void SingleCreationCommandFromUpstreamBC(int index)
        {
            AccountId accountId = new AccountId(Guid.NewGuid());
            var email = String.Format("cronus_{0}_{1}_@Elders.com", index, DateTime.Now);
            commandPublisher.Publish(new RegisterAccount(accountId, email));
        }

        private static void SingleCreationCommandFromDownstreamBC(int index)
        {
            UserId userId = new UserId(Guid.NewGuid());
            var email = String.Format("cronus_{0}_@Elders.com", index);
            commandPublisher.Publish(new CreateUser(userId, email));
        }

        private static void SingleCreateWithMultipleUpdateCommands(int index)
        {
            AccountId accountId = new AccountId(Guid.NewGuid());
            var email = String.Format("cronus_{0}_@Elders.com", index);
            commandPublisher.Publish(new RegisterAccount(accountId, email));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, String.Format("cronus_{0}_{0}_@Elders.com", index)));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, String.Format("cronus_{0}_{0}_{0}_@Elders.com", index)));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, String.Format("cronus_{0}_{0}_{0}_{0}_@Elders.com", index)));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, String.Format("cronus_{0}_{0}_{0}_{0}_{0}_@Elders.com", index)));
            commandPublisher.Publish(new ChangeAccountEmail(accountId, email, String.Format("cronus_{0}_{0}_{0}_{0}_{0}_{0}_@Elders.com", index)));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void HostUI(Action<int> publish)
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

        public static Action<string> queueOverloaded;
        public static Action<string> queueNotReachingMaximumCapacity;

        private static void StartAnalysis(int analysisDelay, int maxMessagesInQueue)
        {
            var client = new RestClient("http://localhost:15672/api/");

            var request = new RestRequest("/queues", Method.GET);
            request.AddHeader("Authorization", "Basic Z3Vlc3Q6Z3Vlc3Q=");

            var timeToNextChangeInMiliseconds = 30000;
            var lastChanged = DateTime.UtcNow;

            while (true)
            {
                IRestResponse<List<Queue>> response = client.Execute<List<Queue>>(request);

                var queue = response.Data.FirstOrDefault(x => x.name.EndsWith("Elders.Cronus.Sample.IdentityAndAccess.Commands"));

                if (int.Parse(queue.messages_ready) < maxMessagesInQueue)
                {
                    if ((DateTime.UtcNow - lastChanged).TotalMilliseconds >= timeToNextChangeInMiliseconds)
                    {
                        if (queueOverloaded != null)
                        { 
                            queueOverloaded("Elders.Cronus.Sample.IdentityAndAccess.Commands");

                            lastChanged = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    if ((DateTime.UtcNow - lastChanged).TotalMilliseconds >= timeToNextChangeInMiliseconds)
                    {
                        if (queueNotReachingMaximumCapacity != null)
                        { 
                            queueNotReachingMaximumCapacity("Elders.Cronus.Sample.IdentityAndAccess.Commands");

                            lastChanged = DateTime.UtcNow;

                            Thread.Sleep(timeToNextChangeInMiliseconds);
                        }

                    }
                }

                Thread.Sleep(analysisDelay);
            }
        }
    }

    public class Queue
    {
        public string name { get; set; }
        public string messages_ready { get; set; }
    }
}