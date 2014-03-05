using System;
using System.Collections.Generic;
using System.IO;
using NMSD.Cronus.DomainModelling;
using NMSD.Cronus.Messaging;
using NMSD.Cronus.Messaging.MessageHandleScope;
using NMSD.Cronus.Transports;
using NMSD.Protoreg;

namespace NMSD.Cronus.Pipelining
{
    public class EndpointConsumer<T> : IEndpointConsumer<T> where T : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumer<T>));

        private readonly MessageHandlerCollection<T> messageHandlers;
        private readonly ScopeFactory scopeFactory;
        private readonly ProtoregSerializer serialiser;

        public EndpointConsumer(MessageHandlerCollection<T> messageHandlers, ScopeFactory scopeFactory, ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            this.scopeFactory = scopeFactory;
            this.messageHandlers = messageHandlers;
            this.messageHandlers.ScopeFactory = scopeFactory;
        }

        public bool Consume(IEndpoint endpoint)
        {
            return scopeFactory.UseBatchScope(batch =>
            {
                for (int i = 0; i < batch.Size; i++)
                {
                    var rawMessage = endpoint.BlockDequeue(batch);

                    T message;
                    using (var stream = new MemoryStream(rawMessage.Body))
                    {
                        message = (T)serialiser.Deserialize(stream);
                    }

                    try
                    {
                        if (!messageHandlers.Handle(message))
                        {
                            throw new Exception("TODO: Build a retry strategy for errors.");
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = String.Format("Error while handling message '{0}'", message.ToString());
                        log.Error(error, ex);
                    }
                    endpoint.Acknowledge(rawMessage);
                }
                return true;
            });
        }

        public IEnumerable<Type> GetRegisteredHandlers
        {
            get { return messageHandlers.GetRegisteredHandlers(); }
        }
    }
}