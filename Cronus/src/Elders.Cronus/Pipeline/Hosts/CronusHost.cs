using Elders.Cronus.Pipeline.Transport.RabbitMQ.Config;

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
            foreach (var consumer in configuration.Consumers)
            {
                consumer.Start();
            }
        }

        public void Stop()
        {
            if (RabbitMqTransportSettings.Session != null)
            {
                RabbitMqTransportSettings.Session.Close();// PLS fix this, This should not be static call so the .Session prop should not be static
                RabbitMqTransportSettings.Session = null;
            }
            foreach (var consumer in configuration.Consumers)
            {
                consumer.Stop();
            }
        }
    }
}