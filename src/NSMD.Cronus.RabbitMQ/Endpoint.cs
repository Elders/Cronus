using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public class Endpoint : IDisposable
    {
        private readonly IDictionary arguments;

        private readonly bool autoDelete;

        private RabbitMQConnection connection;

        private readonly bool durable;

        private readonly bool exclusive;

        public Endpoint(string endpointName, bool durable, bool exclusive, bool autoDelete, RabbitMQConnection connection, string endpointRoutingKeyDefinition, IDictionary acceptanceHeaders)
        {
            AcceptanceHeaders = acceptanceHeaders;
            this.autoDelete = autoDelete;
            this.exclusive = exclusive;
            this.durable = durable;
            this.connection = connection;
            RoutingKey = endpointRoutingKeyDefinition;
            this.Name = endpointName;
            Declare();
            this.connection.OnReconnect += Declare;
        }

        public IDictionary AcceptanceHeaders { get; private set; }

        public string Name { get; private set; }

        public string RoutingKey { get; private set; }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        public void SubscirbeConsumer()
        {
            var consumer = new QueueingBasicConsumer(this.connection.Channel);
            this.connection.Channel.BasicConsume(Name, false, consumer);
            //consumer.

        }

        private void Declare()
        {
            connection.Channel.QueueDeclare(Name, durable, exclusive, autoDelete, AcceptanceHeaders);
        }

    }
}
