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
        public IDictionary AcceptanceHeaders { get; private set; }
        public string RoutingKey { get; private set; }
        public string Name { get; private set; }

        public Endpoint(string endpointName, bool durable, bool exclusive, bool autoDelete, IDictionary arguments, RabbitMQConnection connection, string endpointRoutingKeyDefinition)
        {
            this.arguments = arguments;
            this.autoDelete = autoDelete;
            this.exclusive = exclusive;
            this.durable = durable;
            this.connection = connection;
            RoutingKey = endpointRoutingKeyDefinition;
            this.Name = endpointName;
            Declare();
            this.connection.OnReconnect += Declare;
        }
        public Endpoint(string endpointName, bool durable, bool exclusive, bool autoDelete, IDictionary arguments, RabbitMQConnection connection, IDictionary acceptanceHeaders)
        {
            this.AcceptanceHeaders = acceptanceHeaders;
            this.arguments = arguments;
            this.autoDelete = autoDelete;
            this.exclusive = exclusive;
            this.durable = durable;
            this.connection = connection;
            this.Name = endpointName;
            Declare();
            this.connection.OnReconnect += Declare;
        }

        private void Declare()
        {
            connection.Channel.QueueDeclare(Name, durable, exclusive, autoDelete, arguments);
        }

        public void Dispose()
        {
            connection.Dispose();
            connection = null;
        }

        public void SubscirbeConsumer()
        {
            var consumer = new QueueingBasicConsumer(this.connection.Channel);
            this.connection.Channel.BasicConsume(Name, false, consumer);
            consumer.
        }
    }
}
