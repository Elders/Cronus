using System;
using System.Collections.Generic;
using System.IO;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Elders.Cronus.Pipeline.Transport.RabbitMQ
{
    public class RabbitMqEndpoint : IEndpoint, IDisposable
    {
        private RabbitMqSafeChannel safeChannel;

        private QueueingBasicConsumer consumer;

        private Dictionary<EndpointMessage, BasicDeliverEventArgs> dequeuedMessages;

        private RabbitMqSession session;

        public RabbitMqEndpoint(EndpointDefinition endpointDefinition, RabbitMqSession session)
        {
            RoutingHeaders = new Dictionary<string, object>(endpointDefinition.RoutingHeaders);
            AutoDelete = false;
            Exclusive = false;
            Durable = true;
            this.session = session;
            RoutingKey = endpointDefinition.RoutingKey;
            Name = endpointDefinition.EndpointName;
        }

        public IDictionary<string, object> RoutingHeaders { get; set; }

        public bool AutoDelete { get; set; }

        public bool Durable { get; private set; }

        public bool Exclusive { get; private set; }

        public string Name { get; private set; }

        public string RoutingKey { get; set; }

        public void Acknowledge(EndpointMessage message)
        {
            try
            {
                safeChannel.Channel.BasicAck(dequeuedMessages[message].DeliveryTag, false);
                dequeuedMessages.Remove(message);
            }
            catch (EndOfStreamException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (AlreadyClosedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (OperationInterruptedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public void Acknowledge(IEnumerable<EndpointMessage> messages)
        {
            foreach (EndpointMessage message in messages)
            {
                Acknowledge(message);
            }

        }

        public void AcknowledgeAll()
        {
            try
            {
                foreach (KeyValuePair<EndpointMessage, BasicDeliverEventArgs> dequeuedMessage in dequeuedMessages)
                {
                    safeChannel.Channel.BasicAck(dequeuedMessage.Value.DeliveryTag, false);
                }
                dequeuedMessages.Clear();

            }
            catch (EndOfStreamException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (AlreadyClosedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (OperationInterruptedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            if (safeChannel != null)
                safeChannel.Close();
            safeChannel = null;
            dequeuedMessages.Clear();
        }

        public EndpointMessage DequeueNoWait()
        {
            if (consumer == null) throw new EndpointClosedException(String.Format("The Endpoint '{0}' is closed", Name));

            try
            {
                var result = consumer.Queue.DequeueNoWait(null);
                if (result == null)
                    return null;
                var msg = new EndpointMessage(result.Body, result.RoutingKey, result.BasicProperties.Headers);
                dequeuedMessages.Add(msg, result);
                return msg;
            }
            catch (EndOfStreamException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (AlreadyClosedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (OperationInterruptedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (Exception ex)
            {
                Close();
                throw ex;
            }
        }

        public void Dispose()
        {
            if (safeChannel != null)
                safeChannel.Close();
        }

        public void Declare()
        {
            if (safeChannel == null)
                safeChannel = session.OpenSafeChannel();

            safeChannel.Channel.QueueDeclare(Name, Durable, Exclusive, AutoDelete, RoutingHeaders);
            safeChannel.Close();
            safeChannel = null;
        }

        public void Open()
        {
            safeChannel = session.OpenSafeChannel();
            safeChannel.Channel.BasicQos(0, 1500, false);
            consumer = new QueueingBasicConsumer(safeChannel.Channel);

            safeChannel.Channel.BasicConsume(Name, false, consumer);
            dequeuedMessages = new Dictionary<EndpointMessage, BasicDeliverEventArgs>();
        }

        public bool BlockDequeue(uint timeoutInMiliseconds, out EndpointMessage msg)
        {
            msg = null;
            BasicDeliverEventArgs result;
            if (consumer == null) throw new EndpointClosedException(String.Format("The Endpoint '{0}' is closed", Name));

            try
            {
                if (!consumer.Queue.Dequeue((int)timeoutInMiliseconds, out result))
                    return false;
                msg = new EndpointMessage(result.Body, result.RoutingKey, result.BasicProperties.Headers);
                dequeuedMessages.Add(msg, result);
                return true;
            }
            catch (EndOfStreamException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (AlreadyClosedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (OperationInterruptedException ex)
            { Close(); throw new EndpointClosedException(String.Format("The Endpoint '{0}' was closed", Name), ex); }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public bool Equals(IEndpoint other)
        {
            throw new NotImplementedException();
        }
    }
}