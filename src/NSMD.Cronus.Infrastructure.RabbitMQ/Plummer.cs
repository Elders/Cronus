using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public class EndpointFactory
    {
        private readonly string hostname;
        private readonly string username;
        private readonly string password;
        private readonly int port;
        private readonly string virtualHost;
        private ConnectionFactory factory;

        public EndpointFactory(string hostname, string username = "guest", string password = "guest", int port = 5672, string virtualHost = "/")
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
        public EndpointFactory() : this("localhost") { }

        private Endpoint GetEndpoint(string name, bool durable, bool exclusive, bool autoDelete, IDictionary arguments, string endpointRoutingKeyDefinition)
        {
            var connection = new RabbitMQConnection(factory);
            return new Endpoint(name, durable, exclusive, autoDelete, arguments, connection, endpointRoutingKeyDefinition);
        }

        private Endpoint GetEndpoint(string name, bool durable, bool exclusive, bool autoDelete, IDictionary arguments, IDictionary acceptanceHeaders)
        {
            var connection = new RabbitMQConnection(factory);
            return new Endpoint(name, durable, exclusive, autoDelete, arguments, connection, acceptanceHeaders);
        }

        private Endpoint GetEndpoint(string name, string endpointRoutingKeyDefinition)
        {
            return GetEndpoint(name, true, false, false, null, String.Empty);
        }

        private Endpoint GetEndpoint(string name,IDictionary acceptanceHeaders)
        {
            return GetEndpoint(name, true, false, false, null, String.Empty);
        }
    }
}
