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
        static int delayBetweenBatches = 100;
        static int batchSize = 100;
        static int numberOfMessagesToSend = Int32.MaxValue;
        static Thread hostUIThread;
        static Thread analysisThread;

        static void Main(string[] args)
        {
            Thread.Sleep(5000);

            ConfigurePublisher();

            hostUIThread = new Thread(() => { HostUI(SingleCreationCommandFromUpstreamBC); });
            hostUIThread.Start();

            Console.WriteLine("Start analyzing...");
            analysisThread = new Thread(() => { StartAnalysis(5000, 1, 5000); });
            analysisThread.Start();

            Console.WriteLine("Done");
            Console.ReadLine();
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
        
            for (int i = 0; i <= numberOfMessagesToSend - batchSize; i = i + batchSize)
            {
                for (int j = 0; j < batchSize; j++)
                {
                    publish(i + j);
                }

                Thread.Sleep(delayBetweenBatches);
            }
        }

        private static void StartAnalysis(int analysisDelay, int timeToDetermineStabilityInMinutes, int maxMessagesInQueue)
        {
            var client = new RestClient("http://localhost:15672/api/");

            var request = new RestRequest("/queues", Method.GET);
            request.AddHeader("Authorization", "Basic Z3Vlc3Q6Z3Vlc3Q=");
            
            var timeToNextChangeInMiliseconds = 10000;
            var hasOverloaded = false;

            var initialStart = DateTime.UtcNow;

            while (true)
            {
                IRestResponse<List<Queue>> response = client.Execute<List<Queue>>(request);

                var queue = response.Data.FirstOrDefault(x => x.name.EndsWith("Elders.Cronus.Sample.IdentityAndAccess.Commands"));

                if (int.Parse(queue.messages_ready) < maxMessagesInQueue)
                {
                    if ((DateTime.UtcNow - initialStart).TotalMinutes >= timeToDetermineStabilityInMinutes)
                    {
                        if (!hasOverloaded)
                        {
                            Console.WriteLine("We are not reaching the capabilities of the machine.");

                            break;
                        }
                        else
                        {
                            Console.WriteLine(String.Format("We are able to send {0} message at a time with delay between the messages {1} miliseconds.", batchSize, delayBetweenBatches));

                            break;
                        }
                    }
                }
                else
                {
                    if ((DateTime.UtcNow - initialStart).TotalMilliseconds >= timeToNextChangeInMiliseconds)
                    {
                        delayBetweenBatches += 1;

                        hasOverloaded = true;

                        initialStart = DateTime.UtcNow;
                    }
                }

                Thread.Sleep(analysisDelay);
            }
        }

        private static void StartAnalyzing(int analysisDelay, int timeToDetermineStabilityInMinutes, Func<Queue> getQueue)
        {
            
        }
    }

    public class Queue
    {
        public string name { get; set; }
        public string messages_ready { get; set; }
    }
}