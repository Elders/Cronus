using System;
using System.Collections.Generic;
using System.IO;
using Cronus.Core.Eventing;
using NMSD.Cronus.Core.Commanding;
using NMSD.Cronus.Core.Messaging;
using NMSD.Cronus.Core.Multithreading.Work;
using NSMD.Cronus.RabbitMQ;
using Protoreg;
using RabbitMQ.Client;

namespace NMSD.Cronus.Core.Eventing
{
    public class RabbitEventConsumer : IConsumer<IMessageHandler>
    {
        private string commandsQueueName;

        private Dictionary<Type, List<Func<IEvent, bool>>> handlers = new Dictionary<Type, List<Func<IEvent, bool>>>();

        private IDictionary<string, object> headers;

        private string pipeName;

        private WorkPool pool;

        private readonly ProtoregSerializer serialiser;

        public RabbitEventConsumer(ProtoregSerializer serialiser)
        {
            this.serialiser = serialiser;
            headers = new Dictionary<string, object>();
            headers.Add("x-match", "any");
        }

        public void RegisterHandler(Type eventType, Type eventHandlerType, Func<Type, IMessageHandler> handlerFactory)
        {
            if (!handlers.ContainsKey(eventType))
                handlers[eventType] = new List<Func<IEvent, bool>>();

            handlers[eventType].Add(x => Handle(x, eventHandlerType, handlerFactory));

            AddMessageContractToQueueHeaders(eventType, eventHandlerType);
        }

        public void Start(int numberOfWorkers)
        {
            Plumber plumber = new Plumber();

            //  Think about this
            foreach (var item in handlersQueues)
            {
                CreateEndpoint(plumber.RabbitConnection, pipeName, item.Key, item.Value);
            }

            //  Think about this

            //pool = new WorkPool("defaultPool", numberOfWorkers);
            //for (int i = 0; i < numberOfWorkers; i++)
            //{
            //    pool.AddWork(new ConsumerWork(this, plumber.RabbitConnection, commandsQueueName));
            //}

            //pool.StartCrawlers();
        }

        public void Stop()
        {
            pool.Stop();
        }

        Dictionary<string, IDictionary<string, object>> handlersQueues = new Dictionary<string, IDictionary<string, object>>();

        private void AddMessageContractToQueueHeaders(Type messageType, Type messageHandlerType)
        {
            pipeName = MessagingHelper.GetPipelineName(messageType);

            var handlerQueueName = MessagingHelper.GetHandlerQueueName(messageHandlerType);
            var messageId = MessagingHelper.GetMessageId(messageType);

            IDictionary<string, object> headers;
            if (handlersQueues.TryGetValue(handlerQueueName, out headers))
                headers.Add(messageId, String.Empty);
            else
                handlersQueues.Add(handlerQueueName, new Dictionary<string, object>() { { messageId, String.Empty } });
        }

        private void CreateEndpoint(IConnection rabbitConnection, string pipelineName, string endpointName, IDictionary<string, object> headers)
        {
            using (var channel = rabbitConnection.CreateModel())
            {
                channel.ExchangeDeclare(pipelineName, "headers");
                channel.QueueDeclare(endpointName, true, false, false, headers);
                channel.QueueBind(endpointName, pipelineName, String.Empty, headers);
            }
        }

        private bool Handle(IEvent message)
        {
            List<Func<IEvent, bool>> availableHandlers;
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

        private bool Handle(IEvent message, Type eventHandlerType, Func<Type, IMessageHandler> handlerFactory)
        {
            dynamic handler = null;
            try
            {
                handler = handlerFactory(eventHandlerType);
                handler.Handle((dynamic)message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private class ConsumerWork : IWork
        {
            private RabbitEventConsumer consumer;
            private readonly string endpointName;
            private readonly IConnection rabbitConnection;

            public ConsumerWork(RabbitEventConsumer consumer, IConnection rabbitConnection, string endpointName)
            {
                this.endpointName = endpointName;
                this.rabbitConnection = rabbitConnection;
                this.consumer = consumer;
            }

            public DateTime ScheduledStart { get; set; }

            public void Start()
            {
                using (var channel = rabbitConnection.CreateModel())//ConnectionClosedException??
                {
                    IEvent @event;
                    BasicGetResult result = channel.BasicGet(endpointName, false);
                    if (result != null)
                    {
                        using (var stream = new MemoryStream(result.Body))
                        {
                            @event = consumer.serialiser.Deserialize(stream) as IEvent;
                        }
                        if (consumer.Handle(@event))
                            channel.BasicAck(result.DeliveryTag, false);
                    }
                }
            }
        }
    }

    public class RabbitEventPublisher : RabbitPublisher<IEvent>
    {
        public RabbitEventPublisher(ProtoregSerializer serialiser)
            : base(serialiser)
        {

        }
    }
}
