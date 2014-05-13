using System;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Pipeline.Config;
using Elders.Cronus.Pipeline.Transport.Config;
using RabbitMQ.Client;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ.Config
{
    public static class asd
    {
        public static T UseRabbitMqTransport<T>(this T consumer, Action<RabbitMqTransportSettings> configure = null)
                where T : IHavePipelineTransport
        {
            RabbitMqTransportSettings transportInstance = new RabbitMqTransportSettings();
            if (configure != null)
                configure(transportInstance);
            consumer.TransportSettings = transportInstance;

            return consumer;
        }
    }

    public class RabbitMqTransportSettings : PipelineTransportSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }

        public RabbitMqTransportSettings()
        {
            Server = "localhost";
            Port = 5672;
            Username = ConnectionFactory.DefaultUser;
            Password = ConnectionFactory.DefaultPass;
            VirtualHost = ConnectionFactory.DefaultVHost;
            PipelineSettings = new PipelineSettings();
        }

        public static RabbitMqSession Session;

        public override IPipelineTransport BuildPipelineTransport()
        {
            var rabbitSessionFactory = new RabbitMqSessionFactory(Server, Port, Username, Password, VirtualHost);
            if (Session == null)
                Session = rabbitSessionFactory.OpenSession();

            if (PipelineFactory == null)
                PipelineFactory = new RabbitMqPipelineFactory(Session, PipelineSettings.BuildPipelineNameConvention());

            if (EndpointFactory == null)
                EndpointFactory = new RabbitMqEndpointFactory(Session, PipelineFactory as RabbitMqPipelineFactory, PipelineSettings.BuildEndpointNameConvention());

            return new PipelineTransport(PipelineFactory, EndpointFactory);
        }
    }
}