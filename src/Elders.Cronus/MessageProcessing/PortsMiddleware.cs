using System;
using System.Collections.Generic;
using Elders.Cronus.Middleware;

namespace Elders.Cronus.MessageProcessing
{
    public class PortsMiddleware : MessageHandlerMiddleware
    {
        public PortsMiddleware(IHandlerFactory factory, IPublisher<ICommand> commandPublisher)
            : base(factory)
        {
            BeginHandle.Use((execution) =>
            {
                IPublisher<ICommand> cronusCommandPublisher = new CronusPublisher<ICommand>(commandPublisher, execution.Context.CronusMessage);
                execution.Context.HandlerInstance.AssignPropertySafely<IPort>(x => x.CommandPublisher = cronusCommandPublisher);
            });
        }
    }

    public class SagasMiddleware : MessageHandlerMiddleware
    {
        public SagasMiddleware(IHandlerFactory factory, IPublisher<ICommand> commandPublisher, IPublisher<IScheduledMessage> schedulePublisher)
            : base(factory)
        {
            BeginHandle.Use((execution) =>
            {
                IPublisher<ICommand> cronusCommandPublisher = new CronusPublisher<ICommand>(commandPublisher, execution.Context.CronusMessage);
                IPublisher<IScheduledMessage> cronusSchedulePublisher = new CronusPublisher<IScheduledMessage>(schedulePublisher, execution.Context.CronusMessage);

                execution.Context.HandlerInstance.AssignPropertySafely<ISaga>(x => x.CommandPublisher = cronusCommandPublisher);
                execution.Context.HandlerInstance.AssignPropertySafely<ISaga>(x => x.TimeoutRequestPublisher = cronusSchedulePublisher);
            });
        }
    }

    internal class CronusPublisher<TMessage> : IPublisher<TMessage> where TMessage : IMessage
    {
        readonly IPublisher<TMessage> publisher;
        readonly CronusMessage cronusMessage;

        public CronusPublisher(IPublisher<TMessage> publisher, CronusMessage cronusMessage)
        {
            this.cronusMessage = cronusMessage;
            this.publisher = publisher;
        }

        public bool Publish(TMessage message, Dictionary<string, string> messageHeaders = null)
        {
            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
            AddTrackHeaders(messageHeaders, cronusMessage);
            return publisher.Publish(message, messageHeaders);
        }

        public bool Publish(TMessage message, TimeSpan publishAfter, Dictionary<string, string> messageHeaders = null)
        {
            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
            AddTrackHeaders(messageHeaders, cronusMessage);
            return publisher.Publish(message, publishAfter, messageHeaders);
        }

        public bool Publish(TMessage message, DateTime publishAt, Dictionary<string, string> messageHeaders = null)
        {
            messageHeaders = messageHeaders ?? new Dictionary<string, string>();
            AddTrackHeaders(messageHeaders, cronusMessage);
            return publisher.Publish(message, publishAt, messageHeaders);
        }

        Dictionary<string, string> AddTrackHeaders(Dictionary<string, string> messageHeaders, CronusMessage triggeredBy)
        {
            messageHeaders.Add(MessageHeader.CausationId, triggeredBy.MessageId);
            messageHeaders.Add(MessageHeader.CorelationId, triggeredBy.CorelationId);

            return messageHeaders;
        }
    }
}
