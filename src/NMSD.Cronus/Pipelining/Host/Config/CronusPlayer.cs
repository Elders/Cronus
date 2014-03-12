using System;
using System.Linq;

namespace NMSD.Cronus.Pipelining.Host.Config
{
    public class CronusPlayer
    {
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
            foreach (var evnt in configuration.GlobalSettings.EventStores.Single().GetEventsFromStart())
            {
                publisher.Publish(evnt);
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
