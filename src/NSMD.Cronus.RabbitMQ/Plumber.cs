using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public sealed class Plumber
    {
        private ConnectionFactory factory;

        private readonly string hostname;

        private IConnection connection;
        private readonly string password;

        private readonly int port;

        private readonly string username;

        private readonly string virtualHost;

        public Plumber() : this("192.168.16.53") { }

        public Plumber(string hostname, string username = ConnectionFactory.DefaultUser, string password = ConnectionFactory.DefaultPass, int port = 5672, string virtualHost = ConnectionFactory.DefaultVHost)
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

            try
            {
                connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {

            }
        }

        //var retry = Retry.RetryExponential(10, new TimeSpan(100000000), new TimeSpan(100000000000), new TimeSpan(10000000));

        public IConnection RabbitConnection { get { return connection; } }

        //public Endpoint GetEndpoint(string name, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> acceptanceHeaders)
        //{
        //    return new Endpoint(name, durable, exclusive, autoDelete, new RabbitMQSession(factory), String.Empty, acceptanceHeaders);
        //}

        //public Endpoint GetEndpoint(string name, IDictionary<string, object> acceptanceHeaders)
        //{
        //    return GetEndpoint(name, true, false, false, acceptanceHeaders);
        //}

        //public Pipeline GetPipeline(string name)
        //{
        //    return new Pipeline(name, new RabbitMQSession(factory), Pipeline.PipelineType.Headers);
        //}

    }


}