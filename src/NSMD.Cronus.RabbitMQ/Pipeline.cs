using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace NSMD.Cronus.RabbitMQ
{
    public sealed class Pipeline : IDisposable
    {
        private RabbitMQSession connection;

        private string name;

        public Pipeline(string name, RabbitMQSession connection, PipelineType pipelineType)
        {
            this.name = name;
            this.connection = connection;
            this.connection.Channel.ExchangeDeclare(name, pipelineType.ToString());
        }

        public void AttachEndpoint(Endpoint endpoint)
        {
            connection.Channel.QueueBind(endpoint.Name, name, endpoint.RoutingKey, endpoint.AcceptanceHeaders);
        }

        public void DetachEndpoint(Endpoint endpoint)
        {
            connection.Channel.QueueUnbind(endpoint.Name, name, endpoint.RoutingKey, endpoint.AcceptanceHeaders);
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        public void KickIn(byte[] message, string messageId)
        {
            var properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object>();
            properties.Headers.Add(messageId, String.Empty);
            connection.Channel.BasicPublish(name, String.Empty, properties, message);
        }

        public sealed class PipelineType
        {
            private readonly string name;
            private readonly int value;

            public static readonly PipelineType Direct = new PipelineType(1, "direct");
            public static readonly PipelineType Fanout = new PipelineType(2, "fanout");
            public static readonly PipelineType Headers = new PipelineType(3, "headers");
            public static readonly PipelineType Topics = new PipelineType(4, "topic");

            private PipelineType(int value, string name)
            {
                this.name = name;
                this.value = value;
            }

            public override String ToString()
            {
                return name;
            }

        }
    }
}