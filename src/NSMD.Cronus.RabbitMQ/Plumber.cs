using System;
using System.Collections;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public sealed class Plumber
    {
        private ConnectionFactory factory;

        private readonly string hostname;

        private readonly string password;

        private readonly int port;

        private readonly string username;

        private readonly string virtualHost;

        public Plumber() : this("localhost") { }

        public Plumber(string hostname, string username = "guest", string password = "guest", int port = 5672, string virtualHost = "/")
        {
            this.hostname = hostname;
            this.username = username;
            this.password = password;
            this.port = port;
            this.virtualHost = virtualHost;
            factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost
            };
        }

        public Endpoint GetEndpoint(string name, bool durable, bool exclusive, bool autoDelete, IDictionary acceptanceHeaders)
        {
            var connection = new RabbitMQConnection(factory);
            return new Endpoint(name, durable, exclusive, autoDelete, connection, String.Empty, acceptanceHeaders);
        }

        public Endpoint GetEndpoint(string name, IDictionary acceptanceHeaders)
        {
            return GetEndpoint(name, true, false, false, acceptanceHeaders);
        }

        public Pipeline GetPipeline(string name)
        {
            return new Pipeline(name, new RabbitMQConnection(factory), Pipeline.PipelineType.Headers);
        }

    }
}