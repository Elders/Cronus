using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModelling;
using Elders.Cronus.Messaging.MessageHandleScope;
using Elders.Protoreg;

namespace Elders.Cronus.Pipeline
{
    public class EndpointConsumer<T> : IConsumer<T>
        where T : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EndpointConsumer<T>));

        private readonly IMessageProcessor<T> messageHandlers;
        private readonly ScopeFactory scopeFactory;
        private readonly IEndpointPostConsume postConsume;

        public EndpointConsumer(IMessageProcessor<T> messageHandlers, IEndpointPostConsume postConsume)
        {
            this.postConsume = postConsume;
            this.messageHandlers = messageHandlers;
        }

        public bool Consume(List<T> messages)
        {
            var result = messageHandlers.Handle(messages);


            //  Post handle errors
            foreach (var batch in result.FailedBatches)
            {
                postConsume.ErrorStrategy.Handle(new ErrorMessage(batch.Items.Select(x => (object)x).ToList(), batch.Error));
                log.Error(batch, batch.Error);
            }

            //  Post handle success
            result.SuccessItems.ToList().ForEach(msg => postConsume.SuccessStrategy.Handle(msg));

            return true;
        }

        public bool Consume(T message)
        {
            return true;
        }

        public IEnumerable<Type> GetRegisteredHandlers
        {
            get { return messageHandlers.GetRegisteredHandlers(); }
        }

        public IEndpointPostConsume PostConsume { get { return postConsume; } }
    }
}