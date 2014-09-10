namespace Elders.Cronus.Pipeline.Hosts
{
    public class CronusHost
    {
        private readonly CronusConfiguration configuration;

        public CronusHost(CronusConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected virtual void OnHostStop() { }

        public void Start()
        {
            foreach (var consumer in configuration.Consumers)
            {
                consumer.Start();
            }
        }

        public void Stop()
        {
            OnHostStop();
            foreach (var consumer in configuration.Consumers)
            {
                consumer.Stop();
            }
        }
    }
}