using System;
using System.Linq;
using System.Threading;
using Elders.Cronus.Pipeline.Transport.InMemory;

namespace Elders.Cronus.Pipeline.Hosts
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

            //configuration.GlobalSettings.Consumers.Single().Start(1);
            var publisher = configuration.EventPublisher;
            int totalMessagesPublished = 0;
            Thread.Sleep(2000);//   Test sleep. Remove it later if that is the bug.
            foreach (var evnt in configuration.EventStores.Single().Value.Player.GetEventsFromStart())
            {
                totalMessagesPublished++;
                //publisher.Publish(evnt);
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
            // configuration.GlobalSettings.Consumers.Single().Stop();
        }

    }
}
