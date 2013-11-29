using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace NSMD.Cronus.RabbitMQ
{
    public class RabbitMQConnection
    {
        private IModel channel = null;
        private IConnection connection = null;
        private readonly ConnectionFactory factory;

        public IModel Channel
        {
            get
            {
                Connect();
                return channel;
            }
        }

        public RabbitMQConnection(ConnectionFactory factory)
        {
            this.factory = factory;
            channel = null;
            connection = null;
        }

        private void Connect()
        {
            if (channel != null && channel.IsOpen && connection.IsOpen)
                return;
            else
            {
                if (connection == null || !connection.IsOpen)
                {
                    if (connection != null)
                        connection.Dispose();
                    connection = factory.CreateConnection();
                    connection.ConnectionShutdown += connection_ConnectionShutdown;
                }

                if (channel != null)
                    channel.Dispose();
                channel = connection.CreateModel();
                channel.ModelShutdown += channel_ModelShutdown;

                return;
            }
        }
        public event Action OnReconnect;
        void channel_ModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            Connect();
            OnReconnect();
        }

        void connection_ConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            Connect();
            OnReconnect();
        }
        public void Dispose()
        {
            if (channel != null)
            {
                channel.Close();
                channel = null;
            }
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }
    }
}
