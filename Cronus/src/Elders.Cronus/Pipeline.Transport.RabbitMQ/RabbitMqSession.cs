using System;
using RabbitMQ.Client;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ
{
    public sealed class RabbitMqSession : IDisposable
    {
        private IConnection connection = null;

        private readonly ConnectionFactory factory;

        public RabbitMqSession(ConnectionFactory factory)
        {
            this.factory = factory;
        }

        public IModel OpenChannel()
        {
            Connect();
            return connection.CreateModel();
        }

        public RabbitMqSafeChannel OpenSafeChannel()
        {
            Connect();
            var channel = new RabbitMqSafeChannel(this);
            channel.Reconnect();
            return channel;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (connection != null)
            {
                if (connection.IsOpen)
                    connection.Dispose();
                connection = null;
            }
        }

        private void Connect()
        {
            if (IsConnected())
                return;
            if (connection != null && !connection.IsOpen)
            {
                connection = null;
            }
            if (connection == null)
            {
                connection = factory.CreateConnection();
            }
        }

        bool IsConnected()
        {
            return connection != null && connection.IsOpen;
        }

    }
}