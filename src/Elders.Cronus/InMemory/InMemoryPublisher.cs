using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.Pipeline;

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
            //Think about this in memory????
            //foreach (var batch in result.FailedBatches)
            //{
            //    foreach (var msg in batch.Items)
            //    {
            //        if (msg.Age > 5)
            //        {
            //            log.Error(msg.Payload, batch.Error);
            //            ErrorMessage errorMessage = new ErrorMessage(msg.Payload, batch.Error);
            //            TransportMessage error = new TransportMessage(errorMessage);
            //            postConsume.ErrorStrategy.Handle(error);
            //        }
            //        else
            //        {
            //            postConsume.RetryStrategy.Handle(msg);
            //        }
            //    }
            //}

            //Post handle success
            //result.SuccessItems.ToList().ForEach(msg => postConsume.SuccessStrategy.Handle(msg));
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
}
