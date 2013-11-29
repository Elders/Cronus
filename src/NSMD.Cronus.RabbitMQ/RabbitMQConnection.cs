using System;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public sealed class RabbitMQConnection : IDisposable
    {
        private IModel channel = null;

        private IConnection connection = null;

        private readonly ConnectionFactory factory;

        public event Action OnReconnect;

        public RabbitMQConnection(ConnectionFactory factory)
        {
            this.factory = factory;
            channel = null;
            connection = null;
        }

        public IModel Channel
        {
            get
            {
                Connect();
                return channel;
            }
        }

        public void Dispose()
        {
            if (channel != null)
            {
                channel.Close();
                channel.Dispose();
                channel = null;
            }
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        void channel_ModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            //  Connect();
            if (OnReconnect != null)
                OnReconnect();
        }

        private void Connect()
        {
            if (IsConnected())
                return;

            if (connection != null && !connection.IsOpen)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }

            if (connection == null)
            {
                connection = factory.CreateConnection();
                connection.ConnectionShutdown += connection_ConnectionShutdown;
            }

            if (channel != null)
            {
                channel.Close();
                channel.Dispose();
            }

            channel = connection.CreateModel();
            channel.ModelShutdown += channel_ModelShutdown;
        }

        void connection_ConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            // Connect();
            if (OnReconnect != null)
                OnReconnect();
        }

        bool IsConnected()
        {
            return channel != null && channel.IsOpen && connection.IsOpen;
        }
    }
}