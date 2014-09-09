using System;
using System.Collections.Generic;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumer<TContract> : IConsumer<TContract>
        where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumer<TContract>));

        private readonly IMessageProcessor<TContract> messageHandlers;
        private readonly IEndpointPostConsume postConsume;

        public EndpointConsumer(IMessageProcessor<TContract> messageHandlers, IEndpointPostConsume postConsume)
        {
            this.postConsume = postConsume;
            this.messageHandlers = messageHandlers;
        }

        public bool Consume(List<TransportMessage> transportMessages)
        {
            var result = messageHandlers.Handle(transportMessages);

            foreach (var batch in result.FailedBatches)
            {
                foreach (var msg in batch.Items)
                {
                    if (msg.Age > 5)
                    {
                        log.Error(msg.Payload, batch.Error);
                        ErrorMessage errorMessage = new ErrorMessage(msg.Payload, batch.Error);
                        TransportMessage error = new TransportMessage(errorMessage);
                        postConsume.ErrorStrategy.Handle(error);
                    }
                    else
                    {
                        postConsume.RetryStrategy.Handle(msg);
                    }
                }
            }

            //  Post handle success
            //result.SuccessItems.ToList().ForEach(msg => postConsume.SuccessStrategy.Handle(msg));

            return true;
        }

        public IEnumerable<Type> GetRegisteredHandlers
        {
            get { return messageHandlers.GetRegisteredHandlers(); }
        }

        public IEndpointPostConsume PostConsume { get { return postConsume; } }
    }
}