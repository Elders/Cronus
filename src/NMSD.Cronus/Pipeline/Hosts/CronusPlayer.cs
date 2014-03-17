using System;
using System.Linq;
using System.Threading;
using NMSD.Cronus.Transport.InMemory;

namespace NMSD.Cronus.Pipelining.Hosts
{
    public class CronusPlayer
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CronusPlayer));

        private readonly CronusConfiguration configuration;

        public CronusPlayer(CronusConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Replay()
        {
            Console.WriteLine("Start replaying events...");

            configuration.GlobalSettings.Consumers.Single().Key.Start(1);
            var publisher = configuration.GlobalSettings.EventPublisher;
            int totalMessagesPublished = 0;
            foreach (var evnt in configuration.GlobalSettings.EventStores.Single().GetEventsFromStart())
            {
                totalMessagesPublished++;
                publisher.Publish(evnt);
            }

            //  HACK: We do not know when all messages are consumed
            while (InMemoryQueue.TotalMessagesConsumed < totalMessagesPublished)
            {

                Thread.Sleep(2000);
            }

            Console.WriteLine("Replay finished.");
            Stop();
        }

        public void Stop()
        {
            configuration.GlobalSettings.Consumers.Single().Key.Stop();
        }

    }
}
