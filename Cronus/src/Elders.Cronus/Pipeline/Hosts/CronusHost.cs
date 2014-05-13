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
            foreach (var consumer in configuration.GlobalSettings.Consumers)
            {
                consumer.Start();
            }
        }

        public void Stop()
        {
            if (RabbitMqTransportSettings.Session != null)
            {
                RabbitMqTransportSettings.Session.Close();// PLS fix this
                RabbitMqTransportSettings.Session = null;
            }
            foreach (var consumer in configuration.GlobalSettings.Consumers)
            {
                consumer.Stop();
            }
        }
    }
}