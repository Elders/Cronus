using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace NMSD.Cronus.Core.Messaging
{
    public class RabbitConsumer<TMessage, TMessageHandler> : IConsumer<TMessageHandler>
        where TMessage : IMessage
        where TMessageHandler : IMessageHandler
    {
        protected Dictionary<Type, List<Func<TMessage, bool>>> handlers = new Dictionary<Type, List<Func<TMessage, bool>>>();

        protected Dictionary<string, IDictionary<string, object>> handlerQueues = new Dictionary<string, IDictionary<string, object>>();

        protected string pipeName;

        protected void CreateEndpoint(IConnection rabbitConnection, string pipelineName, string endpointName, IDictionary<string, object> headers)
        {
            using (var channel = rabbitConnection.CreateModel())
            {
                channel.ExchangeDeclare(pipelineName, "headers");
                channel.QueueDeclare(endpointName, true, false, false, headers);
                channel.QueueBind(endpointName, pipelineName, String.Empty, headers);
            }
        }

        protected void AddMessageContractToQueueHeaders(Type messageType, Type messageHandlerType)
        {
            pipeName = MessagingHelper.GetPipelineName(messageType);

            var handlerQueueName = MessagingHelper.GetHandlerQueueName(messageHandlerType);
            var messageId = MessagingHelper.GetMessageId(messageType);

            IDictionary<string, object> headers;
            if (handlerQueues.TryGetValue(handlerQueueName, out headers))
                headers.Add(messageId, String.Empty);
            else
                handlerQueues.Add(handlerQueueName, new Dictionary<string, object>() { { "x-match", "any" }, { messageId, String.Empty } });

        }

        public void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, TMessageHandler> handlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
                handlers[eventType] = new List<Func<TMessage, bool>>();

            handlers[eventType].Add(x => Handle(x, eventHandlerType, handlerFactory));

            AddMessageContractToQueueHeaders(eventType, eventHandlerType);
        }

        protected bool Handle(TMessage message)
        {
            List<Func<TMessage, bool>> availableHandlers;
            if (handlers.TryGetValue(message.GetType(), out availableHandlers))
            {
                foreach (var handleMethod in availableHandlers)
                {
                    var result = handleMethod(message);
                    if (result == false)
                        return result;
                }
            }
            return true;
        }

        protected bool Handle(TMessage message, Type eventHandlerType, Func<Type, TMessageHandler> handlerFactory)
        {
            dynamic handler = null;
            try
            {
                handler = handlerFactory(eventHandlerType);
                handler.Handle((dynamic)message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
