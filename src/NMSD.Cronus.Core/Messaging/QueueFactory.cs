using System;
using System.Collections.Generic;
using System.Reflection;
using RabbitMQ.Client;

namespace NMSD.Cronus.Core.Messaging
{
    public abstract class QueueFactory
    {
        protected Dictionary<string, IDictionary<string, object>> handlerQueues = new Dictionary<string, IDictionary<string, object>>();

        protected string pipeline = null;

        public Dictionary<string, IDictionary<string, object>> Headers { get { return handlerQueues; } }

        public void Compile(IConnection rabbitConnection)
        {
            foreach (var item in handlerQueues)
            {
                CreateEndpoint(rabbitConnection, pipeline, item.Key, item.Value);
            }
        }

        public abstract void Register(Type messageType, Type messageHandlerType);

        protected void BuildPipeline(Type messageType)
        {
            pipeline = pipeline ?? MessagingHelper.GetPipelineName(messageType);
        }

        protected void CreateEndpoint(IConnection rabbitConnection, string pipelineName, string endpointName, IDictionary<string, object> headers)
        {
            using (var channel = rabbitConnection.CreateModel())
            {
                channel.ExchangeDeclare(pipelineName, "headers");
                channel.QueueDeclare(endpointName, true, false, false, headers);
                channel.QueueBind(endpointName, pipelineName, String.Empty, headers);
            }
        }

        protected virtual string GetPipeline(Assembly assembly)
        {
            return null;
        }
    }

    public class QueuePerHandlerFactory : QueueFactory
    {
        public override void Register(Type messageType, Type messageHandlerType)
        {
            BuildPipeline(messageType);

            var handlerQueueName = MessagingHelper.GetHandlerQueueName(messageHandlerType);
            var messageId = MessagingHelper.GetMessageId(messageType);

            IDictionary<string, object> headers;
            if (handlerQueues.TryGetValue(handlerQueueName, out headers))
                headers.Add(messageId, String.Empty);
            else
                handlerQueues.Add(handlerQueueName, new Dictionary<string, object>() { { "x-match", "any" }, { messageId, String.Empty } });
        }
    }

    public class CommonQueueFactory : QueueFactory
    {
        public override void Register(Type messageType, Type messageHandlerType)
        {
            BuildPipeline(messageType);

            var handlerQueueName = String.Format("{0}.Commands", MessagingHelper.GetBoundedContextNamespace(messageHandlerType));
            var messageId = MessagingHelper.GetMessageId(messageType);

            IDictionary<string, object> headers;
            if (handlerQueues.TryGetValue(handlerQueueName, out headers))
                headers.Add(messageId, String.Empty);
            else
                handlerQueues.Add(handlerQueueName, new Dictionary<string, object>() { { "x-match", "any" }, { messageId, String.Empty } });
        }
    }

    public class EventStoreQueueFactory : QueueFactory
    {
        public EventStoreQueueFactory(Assembly assembly)
        {
            var boundedContext = MessagingHelper.GetBoundedContext(assembly);
            pipeline = MessagingHelper.GetEventStorePipelineName(assembly);

            var handlerQueueName = String.Format("{0}.EventStore", MessagingHelper.GetBoundedContextNamespace(assembly));
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers[boundedContext] = String.Empty;
            handlerQueues.Add(handlerQueueName, headers);
        }

        public override void Register(Type messageType, Type messageHandlerType)
        {
            //  Do nothing here
        }
    }
}