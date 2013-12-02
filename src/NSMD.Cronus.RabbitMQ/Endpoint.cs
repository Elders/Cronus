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
        private readonly bool autoDelete;

        private RabbitMQSession session;

        private readonly bool durable;

        private readonly bool exclusive;

        public Endpoint(string endpointName, bool durable, bool exclusive, bool autoDelete, RabbitMQSession session, string endpointRoutingKeyDefinition, IDictionary acceptanceHeaders)
        {
            AcceptanceHeaders = acceptanceHeaders;
            this.autoDelete = autoDelete;
            this.exclusive = exclusive;
            this.durable = durable;
            this.session = session;
            RoutingKey = endpointRoutingKeyDefinition;
            this.Name = endpointName;
            Declare();
            this.session.OnReconnect += Declare;
        }

        public IDictionary AcceptanceHeaders { get; private set; }

        public string Name { get; private set; }

        public string RoutingKey { get; private set; }

        public void Dispose()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
        }

        public void Aknowledge(dynamic message)
        {
            this.session.Channel.BasicAck(message.DeliveryTag, false);
        }

        public object Dequeue()
        {
            return this.session.Channel.BasicGet(Name, false);
        }

        private void Declare()
        {
            session.Channel.QueueDeclare(Name, durable, exclusive, autoDelete, AcceptanceHeaders);
        }

    }
}
