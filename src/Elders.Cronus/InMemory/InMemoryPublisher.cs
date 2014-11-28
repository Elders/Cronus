using System.Collections.Generic;
using System.Linq;
using Elders.Cronus.DomainModeling;

namespace Elders.Cronus.InMemory
{
    public class InMemoryCommandPublisher : Publisher<ICommand>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InMemoryCommandPublisher));

        IMessageProcessor<ICommand> messageProcessor;

        public InMemoryCommandPublisher(IMessageProcessor<ICommand> messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }
        protected override bool PublishInternal(ICommand message)
        {
            var result = messageProcessor.Handle(new List<TransportMessage>() { new TransportMessage(message) });
            if (result.FailedBatches != null && result.FailedBatches.Count() > 0)
                return false;
            return true;
        }
    }

    public class InMemoryEventPublisher : Publisher<IEvent>
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InMemoryEventPublisher));

        IMessageProcessor<IEvent> messageProcessor;

        public InMemoryEventPublisher(IMessageProcessor<IEvent> messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }
        protected override bool PublishInternal(IEvent message)
        {
            var result = messageProcessor.Handle(new List<TransportMessage>() { new TransportMessage(message) });
            if (result.FailedBatches != null && result.FailedBatches.Count() > 0)
                return false;
            return true;
        }
    }

    public class InMemoryPublisher<TContract> : Publisher<TContract> where TContract : IMessage
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InMemoryEventPublisher));

        IMessageProcessor<TContract> messageProcessor;

        public InMemoryPublisher(IMessageProcessor<TContract> messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }
        protected override bool PublishInternal(TContract message)
        {
            var result = messageProcessor.Handle(new List<TransportMessage>() { new TransportMessage(message) });
            if (result.FailedBatches != null && result.FailedBatches.Count() > 0)
                return false;
            return true;
        }
    }
}
