using System;
using System.Collections.Generic;
using System.IO;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{

    public class EndpointConsumer<T> : BatchConsumer<T>, IEndpointConsumer<T>
        where T : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumer<T>));

        private readonly IMessageProcessor<T> messageHandlers;
        private readonly ScopeFactory scopeFactory;
        private readonly ProtoregSerializer serialiser;

        public EndpointConsumer(IMessageProcessor<T> messageHandlers, ScopeFactory scopeFactory, ProtoregSerializer serialiser, IEndpointConsumerSuccessStrategy successStrategy, IEndpointConsumerErrorStrategy errorStrategy)
            : base(messageHandlers)
        {
            SuccessStrategy = successStrategy;
            ErrorStrategy = errorStrategy;
            this.serialiser = serialiser;
            this.scopeFactory = scopeFactory;
            this.messageHandlers = messageHandlers;
            this.messageHandlers.ScopeFactory = scopeFactory;
        }

        public bool Consume(IEndpoint endpoint)
        {
            try
            {
                List<EndpointMessage> rawMessages = new List<EndpointMessage>();
                List<T> messages = new List<T>();
                for (int i = 0; i < messageHandlers.BatchSize; i++)
                {
                    EndpointMessage rawMessage;
                    endpoint.BlockDequeue(30, out rawMessage);
                    if (rawMessage == null)
                        break;

                    T message;

                    using (var stream = new MemoryStream(rawMessage.Body))
                    {
                        message = (T)serialiser.Deserialize(stream);
                    }

                    messages.Add(message);
                    rawMessages.Add(rawMessage);
                }
                if (messages.Count == 0)
                    return true;
                var result = Consume(messages);

                if (ErrorStrategy != null)
                {
                    foreach (var fail in result.FailedItems)
                    {
                        log.Error(fail);
                        ErrorStrategy.Handle(new ErrorMessage(fail, null));
                    }
                }

                if (SuccessStrategy != null)
                {
                    foreach (var success in result.SuccessItems)
                    {
                        SuccessStrategy.Handle(success);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                endpoint.AcknowledgeAll();
            }
        }

        public IEnumerable<Type> GetRegisteredHandlers
        {
            get { return messageHandlers.GetRegisteredHandlers(); }
        }


        public IEndpointConsumerErrorStrategy ErrorStrategy { get; private set; }
        public IEndpointConsumerSuccessStrategy SuccessStrategy { get; private set; }
    }
}