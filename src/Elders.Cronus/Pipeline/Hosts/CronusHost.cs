namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusHost
    {
        private readonly CronusConfiguration configuration;

        public CronusHost(CronusConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Start()
        {
            foreach (var consumer in configuration.GlobalSettings.Consumers)
            {
                consumer.Key.Start(consumer.Value);
            }
        }

        public void Stop()
        {
            foreach (var consumer in configuration.GlobalSettings.Consumers)
            {
                consumer.Key.Stop();
            }
        }
    }
}