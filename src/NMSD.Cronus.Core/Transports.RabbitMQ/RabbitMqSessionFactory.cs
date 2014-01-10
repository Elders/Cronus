using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace NMSD.Cronus.Core.Transports.RabbitMQ
{
    public sealed class RabbitMqSessionFactory
    {
        private ConnectionFactory factory;

        private readonly string hostname;

        private readonly string password;

        private readonly int port;

        private readonly string username;

        private readonly string virtualHost;

        public RabbitMqSessionFactory() : this("192.168.16.69") { }

        public RabbitMqSessionFactory(string hostname, string username = ConnectionFactory.DefaultUser, string password = ConnectionFactory.DefaultPass, int port = 5672, string virtualHost = ConnectionFactory.DefaultVHost)
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

        public RabbitMqSession OpenSession()
        {
            return new RabbitMqSession(factory);
        }
    }


}