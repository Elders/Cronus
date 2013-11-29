using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public class Pipeline : IDisposable
    {
        public string Name { get; set; }

        RabbitMQConnection connection;

        public Pipeline(string name, RabbitMQConnection connection, string PipelineType)
        {
            Name = name;
            this.connection = connection;
            this.connection.Channel.ExchangeDeclare(Name, PipelineType);
        }

        public void AttachEndpoint(Endpoint endpoint)
        {
            connection.Channel.QueueBind(endpoint.Name, Name, endpoint.RoutingKey, endpoint.AcceptanceHeaders);
        }

        public void Dispose()
        {
            connection.Dispose();
            connection = null;
        }
        public class PipelineType
        {
            private PipelineType() { }

            public const string Direct = "direct";

            public const string Fanout = "fanout";

            public const string Headers = "headers";

            public const string Topic = "topic";
        }

    }
}
