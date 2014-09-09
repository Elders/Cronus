using System;
using Elders.Cronus.Pipeline.Config;
using RabbitMQ.Client;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ.Config
{
    public interface IRabbitMqTransportSettings : ISettingsBuilder<IPipelineTransport>, IHavePipelineSettings
    {
        string Server { get; set; }
        int Port { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string VirtualHost { get; set; }
    }

    public class RabbitMqTransportSettings : IRabbitMqTransportSettings
    {
        public RabbitMqTransportSettings()
        {
            this.WithDefaultConnectionSettings();
        }

        string IRabbitMqTransportSettings.Password { get; set; }

        int IRabbitMqTransportSettings.Port { get; set; }

        string IRabbitMqTransportSettings.Server { get; set; }

        string IRabbitMqTransportSettings.Username { get; set; }

        string IRabbitMqTransportSettings.VirtualHost { get; set; }

        Lazy<IPipelineNameConvention> IHavePipelineSettings.PipelineNameConvention { get; set; }

        Lazy<IEndpointNameConvention> IHavePipelineSettings.EndpointNameConvention { get; set; }

        public static RabbitMqSession Session;

        Lazy<IPipelineTransport> ISettingsBuilder<IPipelineTransport>.Build()
        {
            IRabbitMqTransportSettings settings = this as IRabbitMqTransportSettings;

            if (Session == null)
            {
                var rabbitSessionFactory = new RabbitMqSessionFactory(settings.Server, settings.Port, settings.Username, settings.Password, settings.VirtualHost);
                Session = rabbitSessionFactory.OpenSession();
            }

            var pf = new RabbitMqPipelineFactory(Session, settings.PipelineNameConvention.Value);
            var ef = new RabbitMqEndpointFactory(Session, settings.EndpointNameConvention.Value);

            return new Lazy<IPipelineTransport>(() => new PipelineTransport(pf, ef));
        }
    }

    public static class RabbitMqTransportExtensions
    {
        public static T UseRabbitMqTransport<T>(this T self, Action<RabbitMqTransportSettings> configure = null)
                where T : IHaveTransport<IPipelineTransport>, IHavePipelineSettings
        {
            RabbitMqTransportSettings transportSettingsInstance = new RabbitMqTransportSettings();
            if (configure != null)
                configure(transportSettingsInstance);

            self.Transport = new Lazy<IPipelineTransport>(() =>
            {
                self.CopyPipelineSettingsTo(transportSettingsInstance);
                return transportSettingsInstance.GetInstanceLazy().Value;
            });

            return self;
        }

        public static T WithDefaultConnectionSettings<T>(this T self) where T : IRabbitMqTransportSettings
        {
            self.Server = "localhost";
            self.Port = 5672;
            self.Username = ConnectionFactory.DefaultUser;
            self.Password = ConnectionFactory.DefaultPass;
            self.VirtualHost = ConnectionFactory.DefaultVHost;
            return self;
        }
    }
}