using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.InMemory
{
    public class InMemoryCommandPublisher : Publisher<ICommand>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InMemoryCommandPublisher));

        IMessageProcessor messageProcessor;

        public InMemoryCommandPublisher(IMessageProcessor messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }
        protected override bool PublishInternal(ICommand message)
        {
            var result = messageProcessor.Feed(new List<TransportMessage>() { new TransportMessage(message) });
            if (result.FailedMessages != null && result.FailedMessages.Count() > 0)
                return false;
            return true;
        }
    }

    public class InMemoryEventPublisher : Publisher<IEvent>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InMemoryEventPublisher));

        IMessageProcessor messageProcessor;

        public InMemoryEventPublisher(IMessageProcessor messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }
        protected override bool PublishInternal(IEvent message)
        {
            var result = messageProcessor.Feed(new List<TransportMessage>() { new TransportMessage(message) });
            if (result.FailedMessages != null && result.FailedMessages.Count() > 0)
                return false;
            return true;
        }
    }

    public class InMemoryPublisher<TContract> : Publisher<TContract> where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InMemoryEventPublisher));

        IMessageProcessor messageProcessor;

        public InMemoryPublisher(IMessageProcessor messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }
        protected override bool PublishInternal(TContract message)
        {
            var result = messageProcessor.Feed(new List<TransportMessage>() { new TransportMessage(message) });
            if (result.FailedMessages != null && result.FailedMessages.Count() > 0)
                return false;
            return true;
        }
    }
}
