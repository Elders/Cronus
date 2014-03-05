using System;
using NMSD.Cronus.Multithreading.Work;
using NMSD.Cronus.Pipelining;
using NMSD.Cronus.Transports;
using NMSD.Cronus.Transports.RabbitMQ;

namespace NMSD.Cronus.Messaging.MessageHandleScope
{
    public class RabbitMqConsumerWork : IWork
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RabbitMqConsumerWork));

        private IEndpointConsumer consumer;
        private readonly IEndpoint endpoint;

        public RabbitMqConsumerWork(IEndpointConsumer consumer, IEndpoint endpoint)
        {
            this.endpoint = endpoint;
            this.consumer = consumer;
        }

        public DateTime ScheduledStart { get; set; }

        public void Start()
        {
            try
            {
                endpoint.Open();
                while (true)
                {
                    consumer.Consume(endpoint);
                }
            }
            catch (EndpointClosedException ex)
            {
                log.Error("Endpoint Closed", ex);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected Exception.", ex);
            }
            finally
            {
                ScheduledStart = DateTime.UtcNow.AddMilliseconds(30);
                endpoint.Close();
            }
        }
    }
}
